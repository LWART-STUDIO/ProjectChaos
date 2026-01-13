using System.Collections;
using System.Collections.Generic;
using Game.Scripts.Client.Logic.Enemy;
using Game.Scripts.Client.Logic.Player;
using Game.Scripts.Services.Waves;
using PurrNet;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Game
{
    public class EnemySpawner : NetworkBehaviour
    {
        [Header("Waves")]
        [SerializeField] private WavesData _data;

        [Header("Spawn")]
        [SerializeField] private float _spawnRadius = 5f;
        [SerializeField] private int _maxAttempts = 10;
        [SerializeField] private float _groundRayHeight = 5f;
        [SerializeField] private float _ceilingCheckHeight = 2f;

        [Header("Layers")]
        [SerializeField] private LayerMask _groundLayer;

        private Coroutine _spawnCoroutine;

        private int _waveIndex;
        private int _difficultyLevel = 1;

        private int _playerIndex;

        protected override void OnSpawned()
        {
            base.OnSpawned();
            enabled = isServer;

            _waveIndex = 0;
            _difficultyLevel = 1;
            _playerIndex = 0;

            if (!isServer)
                return;
            _spawnCoroutine = StartCoroutine(SpawnLoop());
        }

        #region Spawn Loop

        private IEnumerator SpawnLoop()
        {
            while (true)
            {
                if (PlayerHealth.AllPlayers==null || PlayerHealth.AllPlayers.Count == 0)
                    yield return null;
                if(networkManager.players.Count<=0)
                    yield return null;
                Wave wave = GetCurrentWave();

                yield return SpawnWave(wave);

                _waveIndex++;
                _difficultyLevel++;
            }
        }

        private Wave GetCurrentWave()
        {
            if (_waveIndex < _data.Waves.Count)
                return _data.Waves[_waveIndex];

            // бесконечные волны — последняя волна
            return _data.Waves[_data.Waves.Count - 1];
        }

        private IEnumerator SpawnWave(Wave wave)
        {
            if (PlayerHealth.AllPlayers == null || PlayerHealth.AllPlayers.Count == 0)
                yield break;

            foreach (var enemyPrefab in wave.EnemiesToSpawn)
            {
                foreach (var player in PlayerHealth.AllPlayers.Values)
                {
                    Vector3? spawnPos = FindValidSpawnPosition(player.transform.position);

                    if (spawnPos.HasValue)
                    {
                        SpawnEnemy(enemyPrefab, spawnPos.Value);
                        yield return new WaitForSeconds(wave.SpawnInterval);
                    }
                    else
                    {
                        yield return null;
                    }
                }
            }
        }

        #endregion

        #region Spawn Helpers

        private void SpawnEnemy(GameObject prefab, Vector3 position)
        {
            GameObject enemy = Instantiate(prefab, position, Quaternion.identity);

            if (enemy.TryGetComponent(out EnemyUpgrader upgrader))
                upgrader.UpgradeEnemy(_difficultyLevel);
        }

        private Vector3? FindValidSpawnPosition(Vector3 playerPosition)
        {
            for (int i = 0; i < _maxAttempts; i++)
            {
                // Случайный угол
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

                // Точка на окружности радиуса _spawnRadius
                Vector3 candidate = playerPosition + new Vector3(
                    Mathf.Cos(angle) * _spawnRadius,
                    0f,
                    Mathf.Sin(angle) * _spawnRadius
                );

                // Проверяем землю
                if (!Physics.Raycast(
                        candidate + Vector3.up * _groundRayHeight,
                        Vector3.down,
                        out RaycastHit groundHit,
                        Mathf.Infinity,
                        _groundLayer))
                    continue;

                // Проверяем потолок
                if (Physics.Raycast(
                        groundHit.point,
                        Vector3.up,
                        _ceilingCheckHeight,
                        _groundLayer))
                    continue;

                return groundHit.point;
            }

            return null; // не нашли валидную точку
        }

        #endregion
    }
}
