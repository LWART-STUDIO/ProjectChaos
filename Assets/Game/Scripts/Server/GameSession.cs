using System.Collections.Generic;
using Game.Scripts.Services.UI;
using Sisus.Init;
using Unity.Netcode;


namespace Game.Scripts.Server
{
    public class GameSession : NetworkBehaviour
    {
        public NetworkVariable<bool> IsGameStarted = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        public NetworkVariable<int> PlayerCount = new NetworkVariable<int>(0,NetworkVariableReadPermission.Everyone);
        public List<NetworkVariable<ulong>> RegistredPlayers=new List<NetworkVariable<ulong>>();


        public static GameSession Instance { get; private set; }

        public override void OnNetworkSpawn()
        {
            Instance = this;
            DontDestroyOnLoad(this);
            if (IsServer)
            {
                if(Instance != null&&Instance!= this)
                    Destroy(gameObject);
                IsGameStarted.Value = false;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (Instance == this)
            {
                Instance = null;
            } 
        }

        private void Update()
        {
            if (IsServer)
            {
                
                PlayerCount.Value = NetworkManager.Singleton.ConnectedClients.Count;
                if (RegistredPlayers.Count != PlayerCount.Value)
                {
                    RegistredPlayers.Clear();
                    foreach (var player in NetworkManager.Singleton.ConnectedClients)
                    {
                        RegistredPlayers.Add(new NetworkVariable<ulong>(player.Key,
                            NetworkVariableReadPermission.Everyone));
                    }
                    
                }
            }
        }

        // Хост вызывает это
        public void StartGame()
        {
            if (!IsServer) return;
            IsGameStarted.Value = true;
            LoadGameScene();
        }

        // Клиент вызывает это, чтобы присоединиться
        public void JoinGame()
        {
            if (IsServer)
                return;
            LoadGameScene();
        }

        private void LoadGameScene()
        {
            // Загружаем вручную — SceneManager, а не NetworkSceneManager
            Service<UIService>.Instance.GetGameCanvas().GetLobbyUI().gameObject.SetActive(false);
           // SceneManager.LoadScene("Game");
        }
    }
}