using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Scripts.Client.Logic
{
    public class PlayerSpawner : NetworkBehaviour
    {
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private List<NetPlayerData> _currentPlayers = new List<NetPlayerData>();
        private static PlayerSpawner instance;
        private bool _isStarted;
        public List<NetPlayerData> CurrentPlayers=>_currentPlayers;
        private void Awake()
        {
            if(instance!=null)
                Destroy(this.gameObject);
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        public override void OnNetworkSpawn()
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneLoaded;
            _currentPlayers=new List<NetPlayerData>();

        }

        private void SceneLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            if(_isStarted)
                return;
            if (IsHost && sceneName == "Game")
            {
                foreach (var id in clientsCompleted)
                {
                    GameObject player = Instantiate(_playerPrefab,transform.position,Quaternion.identity);
                    player.GetComponent<NetworkObject>().SpawnAsPlayerObject(id,true);
                    NetPlayerData playerData = player.GetComponent<NetPlayerData>();
                    playerData.PlayerId = id;
                    playerData.Player = player;
                    _currentPlayers.Add(playerData);
                }
               
            }

            _isStarted = true;
        }

        [System.Serializable]
        public class NetPlayerData
        {
            public GameObject Player;
            public ulong PlayerId;
        }
    }
}
