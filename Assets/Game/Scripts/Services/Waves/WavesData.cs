using System.Collections.Generic;
using SaintsField.Playa;
using UnityEngine;

namespace Game.Scripts.Services.Waves
{
    [System.Serializable]
    public class Wave
    {
        // Номер волны (для удобства)
        public int WaveNumber;

        // Список врагов для этой волны
        public List<GameObject> EnemiesToSpawn = new List<GameObject>();

        // Продолжительность волны (в секундах)
        public int WaveDuration;

        // Интервал спауна (рассчитывается автоматически)
        public float SpawnInterval;

        // Значение сложности (стоимость волны), рассчитанное по кривой
        public int WaveValue;
    }
    [CreateAssetMenu(fileName = "WavesData", menuName = "Custom/Waves/WavesData")]
    public class WavesData : ScriptableObject
    {
        [Header("Enemy Pool")]
        [SerializeField] private List<EnemyData> _enemies = new List<EnemyData>();

        [Header("Generation Settings")]
        [SerializeField] private int _totalWaves = 10; // Количество волн для генерации
        [SerializeField] private AnimationCurve _difficultyCurve = AnimationCurve.Linear(0, 1, 1, 1); // Кривая сложности
        [SerializeField] private int _maxWaveDuration = 60; // Максимальная продолжительность волны (в секундах)

        [Header("Generated Waves")]
        [SerializeField] private List<Wave> _waves = new List<Wave>();
        public List<Wave> Waves => _waves;

        [Button]
        public void GenerateAllWaves()
        {
            _waves.Clear();

            for (int i = 1; i <= _totalWaves; i++)
            {
                int waveValue = Mathf.RoundToInt(_difficultyCurve.Evaluate((float)(i - 1) / (_totalWaves - 1)) * i);
                int waveDuration = Mathf.RoundToInt(Mathf.Lerp(30, _maxWaveDuration, (float)(i - 1) / (_totalWaves - 1)));

                Wave newWave = new Wave
                {
                    WaveNumber = i,
                    WaveValue = waveValue,
                    WaveDuration = waveDuration
                };

                GenerateEnemiesForWave(newWave);

                if (newWave.EnemiesToSpawn.Count > 0)
                {
                    newWave.SpawnInterval = (float)newWave.WaveDuration / newWave.EnemiesToSpawn.Count;
                }
                else
                {
                    newWave.SpawnInterval = 0; // На случай, если волна пустая
                }

                _waves.Add(newWave);
            }
        }

        private void GenerateEnemiesForWave(Wave wave)
        {
            List<GameObject> generatedEnemies = new List<GameObject>();
            int remainingValue = wave.WaveValue;

            while (remainingValue > 0)
            {
                // Найдём врагов, стоимость которых <= remainingValue
                List<int> validIndices = new List<int>();
                for (int i = 0; i < _enemies.Count; i++)
                {
                    if (_enemies[i].Cost <= remainingValue)
                    {
                        validIndices.Add(i);
                    }
                }

                if (validIndices.Count == 0)
                {
                    // Если не нашли подходящего врага, выходим
                    break;
                }

                // Выбираем случайного врага из подходящих
                int randIndex = validIndices[Random.Range(0, validIndices.Count)];
                int randEnemyCost = _enemies[randIndex].Cost;

                generatedEnemies.Add(_enemies[randIndex].EnemyPrefab);
                remainingValue -= randEnemyCost;
            }

            wave.EnemiesToSpawn = generatedEnemies;
        }

        // Публичный метод для получения волны по индексу (0-based)
        public Wave GetWave(int index)
        {
            if (index >= 0 && index < _waves.Count)
            {
                return _waves[index];
            }
            return null;
        }

        // Публичный метод для получения общего количества волн
        public int GetTotalWaves()
        {
            return _waves.Count;
        }
    }

    [System.Serializable]
    public class EnemyData
    {
        public GameObject EnemyPrefab;
        public int Cost;
    }
}