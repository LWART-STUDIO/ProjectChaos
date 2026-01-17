using System.Collections.Generic;
using SaintsField.Playa;
using UnityEngine;

namespace Game.Scripts.Services.Waves
{
    [System.Serializable]
    public class Wave
    {
        [Header("Info")]
        public int WaveNumber;

        [Header("Difficulty")]
        public int WaveValue;              // Бюджет сложности
        public int TargetEnemyCount;       // Желаемое количество врагов
        public int WaveDuration;           // Длительность волны (сек)

        [Header("Spawn")]
        public float SpawnInterval;         // Интервал спауна (рассчитан)
        public List<GameObject> EnemiesToSpawn = new();

        [Header("Special")]
        public List<int> EliteIndices = new(); // Индексы элитных врагов
    }
     [CreateAssetMenu(fileName = "WavesData", menuName = "Custom/Waves/WavesData")]
    public class WavesData : ScriptableObject
    {
        [Header("Enemy Pool")]
        [SerializeField] private List<EnemyData> _enemies = new();

        [Header("Generation Settings")]
        [SerializeField] private int _totalWaves = 20;
        [SerializeField] private int _minWaveDuration = 30;
        [SerializeField] private int _maxWaveDuration = 60;

        [SerializeField] private AnimationCurve _difficultyCurve =
            AnimationCurve.EaseInOut(0, 0.3f, 1, 1f);

        [Header("Generated Waves")]
        [SerializeField] private List<Wave> _waves = new();
        public IReadOnlyList<Wave> Waves => _waves;

        // =========================
        // Generation
        // =========================

        [Button]
        public void GenerateAllWaves()
        {
            _waves.Clear();

            for (int i = 1; i <= _totalWaves; i++)
            {
                float t = (float)(i - 1) / (_totalWaves - 1);

                int waveValue = Mathf.RoundToInt(
                    _difficultyCurve.Evaluate(t) * Mathf.Lerp(10, 200, t)
                );

                int targetCount = Mathf.RoundToInt(
                    Mathf.Lerp(5, 80, t)
                );

                int duration = Mathf.RoundToInt(
                    Mathf.Lerp(_minWaveDuration, _maxWaveDuration, t)
                );

                var wave = new Wave
                {
                    WaveNumber = i,
                    WaveValue = waveValue,
                    TargetEnemyCount = targetCount,
                    WaveDuration = duration
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
            var result = new List<GameObject>();
            int remainingValue = wave.WaveValue;

            var sortedEnemies = new List<EnemyData>(_enemies);
            sortedEnemies.Sort((a, b) => a.Cost.CompareTo(b.Cost));

            if (sortedEnemies.Count == 0)
                return;

            // 1. Массовка (дешёвые враги)
            EnemyData cheapest = sortedEnemies[0];

            for (int i = 0; i < wave.TargetEnemyCount; i++)
            {
                if (remainingValue < cheapest.Cost)
                    break;

                result.Add(cheapest.EnemyPrefab);
                remainingValue -= cheapest.Cost;
            }

            // 2. Усиление волны (элитные / сильные)
            int upgradeTries = result.Count / 4;

            for (int i = 0; i < upgradeTries; i++)
            {
                var candidates = sortedEnemies.FindAll(e => e.Cost <= remainingValue);
                if (candidates.Count == 0)
                    break;

                EnemyData strong = candidates[Random.Range(0, candidates.Count)];
                result.Add(strong.EnemyPrefab);
                remainingValue -= strong.Cost;
            }

            wave.EnemiesToSpawn = result;
        }

        // =========================
        // Spawn timing
        // =========================

        private void CalculateSpawnInterval(Wave wave)
        {
            if (wave.EnemiesToSpawn.Count == 0)
            {
                wave.SpawnInterval = 0f;
                return;
            }

            wave.SpawnInterval =
                (float)wave.WaveDuration / wave.EnemiesToSpawn.Count;
        }

        // =========================
        // Elite enemies
        // =========================

        private void GenerateEliteIndices(Wave wave, float t)
        {
            wave.EliteIndices.Clear();

            float eliteChance = Mathf.Lerp(0.05f, 0.3f, t);

            for (int i = 0; i < wave.EnemiesToSpawn.Count; i++)
            {
                if (Random.value < eliteChance)
                    wave.EliteIndices.Add(i);
            }
        }

        // =========================
        // Public API
        // =========================

        public Wave GetWave(int index)
        {
            if (index < 0 || index >= _waves.Count)
                return null;

            return _waves[index];
        }

        public int GetTotalWaves() => _waves.Count;
    }

    // =========================
    // Enemy data
    // =========================

    [System.Serializable]
    public class EnemyData
    {
        public GameObject EnemyPrefab;
        public int Cost;
    }
}