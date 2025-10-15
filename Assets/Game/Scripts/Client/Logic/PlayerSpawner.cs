using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Scripts.Client.Logic
{
    public class PlayerSpawner : NetworkBehaviour
    {
        [SerializeField] private GameObject _playerPrefab;
        private static PlayerSpawner instance;
        private bool _isStarted;
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
                }
               
            }

            _isStarted = true;
        }
    }
}
