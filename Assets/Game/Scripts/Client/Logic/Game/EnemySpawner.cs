using System.Collections;
using System.Linq;
using Game.Scripts.Client.Logic.Player;
using Game.Scripts.Services.Waves;
using PurrNet;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Game
{
       public class EnemySpawner : NetworkBehaviour
    {
        [SerializeField] private WavesData _data;
        [SerializeField] private float _spawnRadius = 5f;  
        private float _raycastDistance = float.MaxValue; 
        
        private Coroutine _spawnCoroutine;
        private PlayerID _currentPlayerId;
        private int _maxAttempts = 10;
        private bool _firstLaunch=true;


        protected override void OnSpawned()
        {
            base.OnSpawned();
            enabled =isServer;
        }
        
        private void Update()
        {
            if (!isServer)
                return;
            if (PlayerHealth.AllPlayers==null || PlayerHealth.AllPlayers.Count == 0)
                return;
            if(_spawnCoroutine != null)
                return;
            if(networkManager.players.Count<=0)
                return;
            _spawnCoroutine = StartCoroutine(SpawnWaves());

        }

        private IEnumerator SpawnWaves()
        {
            foreach (var wave in _data.Waves)
            {
                foreach (var enemy in wave.EnemiesToSpawn)
                {
                    for (int attempt = 0; attempt < 100; attempt++) // защита от бесконечности
                    {
                        Vector3 playerPos = PlayerHealth.AllPlayers[SelectPlayer()].transform.position;
                        Vector3? spawnPos = FindValidSpawnPosition(playerPos);
                        if (spawnPos.HasValue)
                        {
                            GameObject newEnemy = Instantiate(enemy, spawnPos.Value, Quaternion.identity);
                            yield return new WaitForSeconds(wave.SpawnInterval);
                            break; // ✅ выходим из цикла
                        }
                        yield return null;
                    }
                }
            }

            _spawnCoroutine = null;
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
            }

            // Не удалось найти валидную позицию
            return null;
        }

        private PlayerID SelectPlayer()
        {
            if (_firstLaunch)
            {
                _currentPlayerId = networkManager.players[0];
                _firstLaunch = false;  
                return _currentPlayerId;
            }
            var playerIds = PlayerHealth.AllPlayers.Keys.ToList();
            int currentIndex = playerIds.IndexOf(_currentPlayerId);
            int nextIndex = (currentIndex + 1) % playerIds.Count;
            _currentPlayerId = playerIds[nextIndex];
            return _currentPlayerId;
        }
    }
}

