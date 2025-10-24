using System;
using System.Collections;
using System.Collections.Generic;
using Game.Scripts.Client.Logic.Player;
using Game.Scripts.Server;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Scripts.Client.Logic
{
    public class PlayerSpawner : NetworkBehaviour
    {
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private List<NetPlayerData> _currentPlayers = new List<NetPlayerData>();
        public static PlayerSpawner instance;
        private bool _isStarted;
        public List<NetPlayerData> CurrentPlayers => _currentPlayers;

        private void Awake()
        {
  
            if (instance != null&&instance != this)
                Destroy(this.gameObject);
            instance = this;
            
 
        }

        [Rpc(SendTo.Server)]
        public void SpawnPlayersRpc(ulong id)
        {
            if(!IsServer)
                return;
            _currentPlayers = new List<NetPlayerData>();
            if (IsHost)
            {
                AttachScenePlayerToHost();
            }

            SceneLoaded(id);
        }
        private void AttachScenePlayerToHost()
        {
            // Найти объект игрока на сцене
            var existingPlayer = FindFirstObjectByType<PlayerTag>();
            if (existingPlayer == null)
                return;

            var networkObject = existingPlayer.GetComponent<NetworkObject>();
  
            if (!networkObject.IsSpawned)
            {
                // Делаем этого игрока локальным объектом хоста
                networkObject.SpawnAsPlayerObject(NetworkManager.Singleton.LocalClientId);
            }

            if (!_currentPlayers.Exists(p => p.PlayerId == networkObject.NetworkObjectId))
            {
                var playerData = new NetPlayerData();
                playerData.PlayerId = NetworkManager.Singleton.LocalClientId;
                playerData.Player = existingPlayer.gameObject;
                
                _currentPlayers.Add(playerData);
            }
        }

        private void SceneLoaded(ulong idPlayer)
        {
   
            
            if (IsHost)
            {
                foreach (var id in GameSession.Instance.RegistredPlayers)
                {
                    if (_currentPlayers.Exists(x => x.PlayerId == id.Value))
                        continue;
                    if(idPlayer!=id.Value)
                        return;
                    GameObject player = Instantiate(_playerPrefab, transform.position, Quaternion.identity);
                    player.GetComponent<NetworkObject>().
                        SpawnAsPlayerObject(id.Value, true);
                    NetPlayerData playerData = new NetPlayerData();
                    playerData.PlayerId = id.Value;
                    playerData.Player = player;
                    _currentPlayers.Add(playerData);
                }
            }
        }

        [Serializable]
        public class NetPlayerData
        {
            public GameObject Player;
            public ulong PlayerId;
        }

        public int TotalPlayerCount
        {
            get
            {
                if (NetworkManager.Singleton == null)
                    return 0;
                int count = NetworkManager.Singleton.ConnectedClientsIds.Count;
                if (NetworkManager.Singleton.IsHost)
                    count++;
                return count;
            }
        }

        public IEnumerable<ulong> AllPlayerIds
        {
            get
            {
                if (NetworkManager.Singleton == null)
                    yield break;

                foreach (var id in NetworkManager.Singleton.ConnectedClientsIds)
                    yield return id;

                if (NetworkManager.Singleton.IsHost)
                    yield return NetworkManager.Singleton.LocalClientId;
            }
        }

       
    }
}