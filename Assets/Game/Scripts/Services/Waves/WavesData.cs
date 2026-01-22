using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField;
using SaintsField.Playa;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Scripts.Services.Waves
{
    [System.Serializable]
    public class Wave
    {
        [Header("Info")]
        public int WaveNumber;

        [Header("Difficulty")]
        public int WaveValue;
        public int TargetEnemyCount;
        public int WaveDuration;

        [Header("Spawn")]
        public float SpawnInterval;
        public List<GameObject> EnemiesToSpawn = new();

        [Header("Special")]
        public List<int> EliteIndices = new();
    }

    [CreateAssetMenu(fileName = "WavesData", menuName = "Game/Waves/WavesData")]
    public class WavesData : ScriptableObject
    {
        [Header("Enemy Pool (fallback only)")]
        [SerializeField] private List<EnemyData> _enemies = new();

        [Header("Generation Settings")]
        [SerializeField] private int _totalWaves = 20;
        [SerializeField] private int _minWaveDuration = 30;
        [SerializeField] private int _maxWaveDuration = 60;

        [SerializeField] private AnimationCurve _difficultyCurve =
            AnimationCurve.EaseInOut(0, 0.3f, 1, 1f);

        [Header("Wave Templates")]
        [Expandable]
        [SerializeField] private List<WaveTemplate> _templates = new();

        [Header("Template Progression")]
        [SerializeField] private List<TemplateProgressionStage> _templateStages = new();

        [Header("Generated Waves")]
        [SerializeField] private List<Wave> _waves = new();
        public IReadOnlyList<Wave> Waves => _waves;

        [Header("Debug")]
        [SerializeField] private int _randomSeed = 0;

        // =========================
        // Generation
        // =========================

        [Button]
        public void GenerateAllWaves()
        {
            if (_randomSeed == 0)
                _randomSeed = Guid.NewGuid().GetHashCode();

            Random.InitState(_randomSeed);

            _waves.Clear();

            for (int i = 1; i <= _totalWaves; i++)
            {
                float t = _totalWaves <= 1
                    ? 1f
                    : (float)(i - 1) / (_totalWaves - 1);

                var wave = new Wave
                {
                    WaveNumber = i,
                    WaveValue = Mathf.RoundToInt(
                        _difficultyCurve.Evaluate(t) * Mathf.Lerp(10, 200, t)
                    ),
                    TargetEnemyCount = Mathf.RoundToInt(Mathf.Lerp(5, 80, t)),
                    WaveDuration = Mathf.RoundToInt(Mathf.Lerp(_minWaveDuration, _maxWaveDuration, t))
                };

                GenerateEnemiesForWave(wave);
                CalculateSpawnInterval(wave);
                GenerateEliteIndices(wave, t);

                _waves.Add(wave);
            }
        }

        // =========================
        // Enemy generation
        // =========================

        private void GenerateEnemiesForWave(Wave wave)
        {
            var templates = GetAvailableTemplates(wave.WaveNumber);
            if (templates.Count == 0)
            {
                FallbackGenerate(wave);
                return;
            }

            var template = SelectWeightedTemplate(templates);
            var result = new List<GameObject>();

            int remainingValue = wave.WaveValue;
            int attempts = 0;
            int maxAttempts = Mathf.Max(50, wave.TargetEnemyCount * 3);

            while (remainingValue > 0 && attempts < maxAttempts)
            {
                attempts++;

                var affordable = template.EnemyWeights
                    .Where(w => w.Enemy.Cost <= remainingValue)
                    .ToList();

                if (affordable.Count == 0)
                    break;

                EnemyData chosen = PickEnemyByWeight(affordable);
                result.Add(chosen.EnemyPrefab);
                remainingValue -= chosen.Cost;
            }

            // Минимальное заполнение
            if (result.Count < wave.TargetEnemyCount / 2)
            {
                var cheapest = template.EnemyWeights
                    .OrderBy(w => w.Enemy.Cost)
                    .First().Enemy;

                while (remainingValue >= cheapest.Cost &&
                       result.Count < wave.TargetEnemyCount)
                {
                    result.Add(cheapest.EnemyPrefab);
                    remainingValue -= cheapest.Cost;
                }
            }

            wave.EnemiesToSpawn = result;
        }

        private EnemyData PickEnemyByWeight(List<EnemyWeight> weights)
        {
            float total = weights.Sum(w => w.RelativeWeight);
            float roll = Random.value * total;
            float acc = 0f;

            foreach (var w in weights)
            {
                acc += w.RelativeWeight;
                if (roll <= acc)
                    return w.Enemy;
            }

            return weights[^1].Enemy;
        }

        // =========================
        // Templates
        // =========================

        private List<WaveTemplate> GetAvailableTemplates(int waveNumber)
        {
            if (_templateStages.Count == 0)
                return _templates;

            var stage = _templateStages
                .Where(s => waveNumber >= s.MinWave)
                .OrderByDescending(s => s.MinWave)
                .FirstOrDefault();

            return stage != null
                ? stage.AvailableTemplates.Where(t => t != null).ToList()
                : new List<WaveTemplate>();
        }

        private WaveTemplate SelectWeightedTemplate(List<WaveTemplate> templates)
        {
            float total = templates.Sum(t => t.Weight);
            float roll = Random.value * total;
            float acc = 0f;

            foreach (var t in templates)
            {
                acc += t.Weight;
                if (roll <= acc)
                    return t;
            }

            return templates[^1];
        }

        // =========================
        // Fallback
        // =========================

        private void FallbackGenerate(Wave wave)
        {
            if (_enemies.Count == 0)
                return;

            var cheapest = _enemies.OrderBy(e => e.Cost).First();
            int remaining = wave.WaveValue;

            for (int i = 0; i < wave.TargetEnemyCount; i++)
            {
                if (remaining < cheapest.Cost)
                    break;

                wave.EnemiesToSpawn.Add(cheapest.EnemyPrefab);
                remaining -= cheapest.Cost;
            }
        }

        // =========================
        // Spawn / Elite
        // =========================

        private void CalculateSpawnInterval(Wave wave)
        {
            wave.SpawnInterval = wave.EnemiesToSpawn.Count == 0
                ? 0f
                : (float)wave.WaveDuration / wave.EnemiesToSpawn.Count;
        }

        private void GenerateEliteIndices(Wave wave, float t)
        {
            wave.EliteIndices.Clear();
            float chance = Mathf.Lerp(0.05f, 0.3f, t);

            for (int i = 0; i < wave.EnemiesToSpawn.Count; i++)
            {
                if (Random.value < chance)
                    wave.EliteIndices.Add(i);
            }
        }

        // =========================
        // API
        // =========================

        public Wave GetWave(int index) =>
            index < 0 || index >= _waves.Count ? null : _waves[index];

        public int GetTotalWaves() => _waves.Count;
    }

    // =========================
    // Data
    // =========================

    [System.Serializable]
    public class EnemyData
    {
        public GameObject EnemyPrefab;
        public int Cost;
    }

    [System.Serializable]
    public class EnemyWeight
    {
        public EnemyData Enemy;
        [Range(0f, 1f)] public float RelativeWeight = 1f;
    }

    [System.Serializable]
    public class TemplateProgressionStage
    {
        public int MinWave = 1;
        [Expandable] public List<WaveTemplate> AvailableTemplates = new();
    }
}
