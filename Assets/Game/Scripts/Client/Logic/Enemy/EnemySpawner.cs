using System;
using System.Collections;
using Game.Scripts.Services.Waves;
using ProjectDawn.Navigation.Hybrid;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Enemy
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private WavesData _data;
        [SerializeField] private float _spawnRadius = 5f;  
        private float _raycastDistance = float.MaxValue; 
        
        private PlayerSpawner _playerSpawner=>PlayerSpawner.Instance;
        private Coroutine _spawnCoroutine;
        private int _currentPlayerIndex;
        private int _maxAttempts = 10;    
        

        private void Update()
        {
            if(_playerSpawner==null)
                return;
            if (_playerSpawner.CurrentPlayers == null || _playerSpawner.CurrentPlayers.Count == 0)
                return;
            if(_spawnCoroutine != null)
                return;
            _spawnCoroutine = StartCoroutine(SpawnWaves());

        }

        private IEnumerator SpawnWaves()
        {
            foreach (var wave in _data.Waves)
            {
                foreach (var enemy in wave.EnemiesToSpawn)
                {
                    bool spawned = false;
                    while (!spawned)
                    {
                        Vector3 playerPos = _playerSpawner.CurrentPlayers[SelectPlayer()].Player.transform.position;
                        Vector3? spawnPos = FindValidSpawnPosition(playerPos);
                        if (spawnPos.HasValue)
                        {
                            GameObject newEnemy =Instantiate(enemy, spawnPos.Value, Quaternion.identity);
                            var crowed = _playerSpawner.CurrentPlayers[_currentPlayerIndex].Player
                                .GetComponent<CrowdGroupAuthoring>();
                            newEnemy.GetComponent<AgentSetDestination>()
                                .SetTarget(_playerSpawner.CurrentPlayers[_currentPlayerIndex].Player.transform,crowed);
                            spawned = true;
                        }
                        else
                        {
                            yield return null;
                            
                        }
                    }
                    yield return new WaitForSeconds(wave.SpawnInterval);
                }
            }
        }
        private Vector3? FindValidSpawnPosition(Vector3 playerPosition)
        {
            for (int attempt = 0; attempt < _maxAttempts; attempt++)
            {
                // Генерируем случайную точку в круге вокруг игрока (на плоскости XZ)
                Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * _spawnRadius;
                Vector3 candidate = playerPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);

                // Проверяем вниз: есть ли земля?
                if (Physics.Raycast(candidate + Vector3.up * 5f, Vector3.down, out RaycastHit hitDown, _raycastDistance, 1<<6))
                {
                    // Опционально: проверяем вверх — нет ли потолка слишком близко?
                    // (если нужно, чтобы враг не застревал)
                    if (!Physics.Raycast(hitDown.point, Vector3.up, 2f, 1<<6))
                    {
                        // Возвращаем точку **на поверхности земли**
                        return hitDown.point;
                    }
                }

                // Альтернатива: проверка "вниз и вверх" как "есть ли коллизия в радиусе"
                // Но raycast проще и дешевле.
            }

            // Не удалось найти валидную позицию
            return null;
        }

        private int SelectPlayer()
        {
            for (var index = 0; index < _playerSpawner.CurrentPlayers.Count; index++)
            {
                var player = _playerSpawner.CurrentPlayers[index];
                if (index > _currentPlayerIndex)
                {
                    _currentPlayerIndex = index;
                    return _currentPlayerIndex;
                }
            }

            _currentPlayerIndex = 0;
            return 0;
        }
    }
}
