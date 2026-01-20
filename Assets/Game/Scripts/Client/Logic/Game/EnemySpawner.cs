using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Scripts.Client.Logic.Enemy;
using Game.Scripts.Client.Logic.Player;
using Game.Scripts.Services.Waves;
using PurrNet;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Scripts.Client.Logic.Game
{
    public class EnemySpawner : NetworkBehaviour
    {
        [Header("Waves")]
        [SerializeField] private WavesData _data;

        [Header("Spawn Area")]
        [SerializeField] private float _spawnRadius = 6f;
        [SerializeField] private int _maxAttempts = 10;

        [Header("Pressure Control")]
        [SerializeField] private int _maxAliveEnemies = 50;

        [Header("Batch Spawn")]
        [SerializeField] private int _minBatchSize = 2;
        [SerializeField] private int _maxBatchSize = 5;
        [SerializeField] private float _batchInterval = 0.5f;
        [Header("Wave Flow")]
        [SerializeField] private int _earlyNextWaveThreshold = 3;

        private int _waveIndex;
        private int _difficultyLevel=0;
        private int _roundRobinIndex;
        private int _aliveEnemies;
        private float _timeFromLastEnemySpawn;

        private Coroutine _spawnRoutine;

        // =========================
        // Lifecycle
        // =========================

        protected override void OnSpawned()
        {
            base.OnSpawned();
            enabled = isServer;

            _waveIndex = 0;
            _difficultyLevel = 0;
            _roundRobinIndex = 0;
            _aliveEnemies = 0;

            EnemyHealth.onEnemyKilled += OnEnemyKilled;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EnemyHealth.onEnemyKilled -= OnEnemyKilled;
        }

        public void StartSpawning()
        {
            if (!isServer || _spawnRoutine != null)
                return;

            _spawnRoutine = StartCoroutine(SpawnLoop());
        }

        // =========================
        // Main Loop
        // =========================

        private IEnumerator SpawnLoop()
        {
            while (true)
            {
                if (!HasAlivePlayers())
                {
                    yield return null;
                    continue;
                }

                Wave wave = GetCurrentWave();
                Debug.Log($"Уровень сложности:{_difficultyLevel}");
                yield return SpawnWave(wave);

                // ожидание завершения волны
                while (_aliveEnemies > _earlyNextWaveThreshold)
                {
                    if (_timeFromLastEnemySpawn+10f < Time.time)
                    {
                        break;
                    }
                    yield return null;
                    
                }
                yield return null;
                _waveIndex++;
                _difficultyLevel++;
                
            }
        }

        private Wave GetCurrentWave()
        {
            if (_waveIndex < _data.Waves.Count)
                return _data.Waves[_waveIndex];

            // бесконечные волны
            return _data.Waves[^1];
        }

        // =========================
        // Wave Spawn
        // =========================

        private IEnumerator SpawnWave(Wave wave)
        {
            Queue<GameObject> spawnQueue = BuildSpawnQueue(wave);

            while (spawnQueue.Count > 0)
            {
                // ждём свободные слоты
                while (_aliveEnemies >= _maxAliveEnemies)
                {
                    if (_timeFromLastEnemySpawn+10f < Time.time)
                        break;
                    yield return null;
                }
                    

                int desiredBatch = Random.Range(_minBatchSize, _maxBatchSize + 1);
                int availableSlots = _maxAliveEnemies - _aliveEnemies;
                int actualBatch = Mathf.Min(desiredBatch, availableSlots);

                // если нет места — ждём дальше
                if (actualBatch <= 0)
                {
                    yield return null;
                    continue;
                }

                for (int i = 0; i < actualBatch && spawnQueue.Count > 0; i++)
                {
                    var enemyPrefab = spawnQueue.Dequeue();

                    bool enemyWasSpawned = SpawnNextEnemy(enemyPrefab);
                    if (!enemyWasSpawned)
                    {
                        i--;
                        spawnQueue.Enqueue(enemyPrefab);
                        yield return null;
                    }
                }

                yield return new WaitForSeconds(_batchInterval);
            }
        }

        // =========================
        // Spawn helpers
        // =========================

        private Queue<GameObject> BuildSpawnQueue(Wave wave)
        {
            int playerCount = PlayerHealth.AllPlayers.Count;
            int multiplier = Mathf.Max(1, playerCount);

            Queue<GameObject> queue = new();

            foreach (var enemy in wave.EnemiesToSpawn)
            {
                for (int i = 0; i < multiplier; i++)
                    queue.Enqueue(enemy);
            }

            return queue;
        }

        private bool SpawnNextEnemy(GameObject prefab)
        {
            var players = PlayerHealth.AllPlayers.Values.ToList();
            if (players.Count == 0)
                return false;

            PlayerHealth target = players[_roundRobinIndex];
            _roundRobinIndex = (_roundRobinIndex + 1) % players.Count;

            Vector3? pos = FindValidSpawnPosition(target.transform.position);
            if (!pos.HasValue)
                return false;
            Debug.Log("SpawnEnemy");
            InstantiateEnemy(prefab, pos.Value);
            return true;
        }

        private void InstantiateEnemy(GameObject prefab, Vector3 position)
        {
            GameObject enemy = Instantiate(prefab, position, Quaternion.identity);
            _aliveEnemies++;
            _timeFromLastEnemySpawn=Time.time;
            if (enemy.TryGetComponent(out EnemyUpgrader upgrader))
                upgrader.UpgradeEnemy(_difficultyLevel);
        }

        // =========================
        // Event handlers
        // =========================

        private void OnEnemyKilled(EnemyHealth enemy)
        {
            _aliveEnemies = Mathf.Max(0, _aliveEnemies - 1);
        }

        // =========================
        // Utils
        // =========================

        private bool HasAlivePlayers()
        {
            return PlayerHealth.AllPlayers != null &&
                   PlayerHealth.AllPlayers.Count > 0 &&
                   networkManager.players.Count > 0;
        }

        private Vector3? FindValidSpawnPosition(Vector3 playerPosition)
        {
            for (int i = 0; i < _maxAttempts; i++)
            {
                float angle = Random.Range(0f, Mathf.PI * 2f);
                Vector3 candidate =
                    playerPosition +
                    new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * _spawnRadius;

                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                    return hit.position;
            }

            return null;
        }
    }
}
