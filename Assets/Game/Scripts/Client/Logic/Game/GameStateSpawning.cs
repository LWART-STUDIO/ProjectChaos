using System.Collections.Generic;
using Game.Scripts.Client.Logic.Player;
using PurrNet;
using PurrNet.StateMachine;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Game
{
    public class GameStateSpawning : StateNode<Dictionary<PlayerID, PlayerClassType>>
    {
        [SerializeField] private List<Transform> _spawnPoints;
        [SerializeField] private List<PlayerHealth> _players = new List<PlayerHealth>();
        [SerializeField] private List<PlayerClassConfig> _classConfigs;
        private Dictionary<PlayerClassType, PlayerHealth> _prefabs;
        private Dictionary<PlayerID, PlayerClassType> _playerClases;
        

     
        private void Awake()
        {
            _prefabs = new Dictionary<PlayerClassType, PlayerHealth>();
            foreach (var config in _classConfigs)
                _prefabs[config.classType] = config.prefab;

        }

        public override void Enter(Dictionary<PlayerID, PlayerClassType> dictionary, bool asServer)
        {
            base.Enter(asServer);

            _playerClases = dictionary;
            if (asServer)
            {
                DespawnPlayers();
                SpawnPlayers();
                machine.Next();
            }
        }


        private void DespawnPlayers()
        {
            var allPlayers = FindObjectsByType<PlayerHealth>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var player in allPlayers)
                Destroy(player.gameObject);
        }

        private void SpawnPlayers()
        {
            _players.Clear();
            int spawnIndex = 0;

            foreach (var player in networkManager.players)
            {
                if (!_playerClases.TryGetValue(player, out var classType))
                {
                    Debug.LogError($"[SPAWN] Missing class for player {player}");
                    continue;
                }

                if (!_prefabs.TryGetValue(classType, out var prefab))
                {
                    Debug.LogError($"[SPAWN] No prefab for class {classType}");
                    continue;
                }

                var spawnPoint = _spawnPoints[spawnIndex];
                Debug.Log($"[SPAWN] Игрок {player} выбрал класс {classType}");
                var newPlayer = Instantiate(
                    prefab,
                    spawnPoint.position,
                    spawnPoint.rotation
                );

                newPlayer.GiveOwnership(player);
                _players.Add(newPlayer);

                spawnIndex = (spawnIndex + 1) % _spawnPoints.Count;
            }
        }


        public override void Exit(bool asServer)
        {
            base.Exit(asServer);
        }
    }
}
