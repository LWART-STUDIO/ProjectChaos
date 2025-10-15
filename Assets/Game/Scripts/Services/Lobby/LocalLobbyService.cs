using System;
using Game.Scripts.Services.Steam;
using Sisus.Init;
using Steamworks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Game.Scripts.Services.Lobby
{
    [Service(typeof(LocalLobbyService),LazyInit = true)]
    public class LocalLobbyService : MonoBehaviour,IService, ILobbyService
    {
        [SerializeField] private CSteamID _lastCreatedLobbyId;
        public event Action<LobbyPlayer> OnPlayerJoined;
        public event Action<LobbyPlayer> OnPlayerLeft;
        public CSteamID LobbyId => _lastCreatedLobbyId;
        private bool _wasAwaked;
        private bool _wasStarted;
        public void LocalAwake()
        {
            if(_wasAwaked)
                return;
            NetworkManager network = FindFirstObjectByType<NetworkManager>();
            network.NetworkConfig.NetworkTransport =
                network.GetComponent<UnityTransport>();
            _wasAwaked = true;
        }

        public void LocalStart()
        {
            if (_wasStarted)
                return;
            if (NetworkManager.Singleton == null) return;

            // Подписка на подключения других клиентов (только Host будет их видеть)
            NetworkManager.Singleton.OnClientConnectedCallback += clientId =>
            {
                // Игрок присоединился
               OnClientConnected(clientId);
            };

            NetworkManager.Singleton.OnClientDisconnectCallback += clientId =>
            {
                // Игрок отключился
                OnLedtLobby(clientId);
            };
            _wasStarted = true;
            
            
        }

        public void LocalUpdate(float deltaTime)
        {

        }

        public void LeaveLobby()
        {
        }

        public void StartGameServer()
        {
            
        }

        public void CreateLobby()
        {
            _lastCreatedLobbyId = new CSteamID(12345);
            NetworkManager.Singleton.StartHost();
            
   
        }
        

        private void OnClientConnected(ulong clientId)
        {
            // Создаём фиктивный ник и аватар
            string nick = $"Player_{clientId}";
            Texture2D avatar = GeneratePlaceholderAvatar(clientId); // метод возвращает Texture2D
            OnPlayerJoined?.Invoke(new LobbyPlayer(clientId, nick, avatar));
        }

        private Texture2D GeneratePlaceholderAvatar(ulong id)
        {
            Texture2D tex = new Texture2D(64, 64);
            Color col = Color.HSVToRGB((id % 10) / 10f, 0.8f, 0.8f);
            for (int x = 0; x < 64; x++)
            for (int y = 0; y < 64; y++)
                tex.SetPixel(x, y, col);
            tex.Apply();
            return tex;
        }
        public void JoinLobby()
        {
            NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address = "127.0.0.1";
            NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Port = 7777;
            NetworkManager.Singleton.StartClient();
            OnClientConnected(NetworkManager.Singleton.LocalClientId);
        }
        public void OnLedtLobby(ulong clientId)
        {
            string nick = $"Player_{clientId}";
            Texture2D avatar = GeneratePlaceholderAvatar(clientId); // метод возвращает Texture2D
            OnPlayerLeft?.Invoke(new LobbyPlayer(clientId, nick, avatar));
        }

       
        private void OnApplicationQuit()
        {
            // Выключить NGO
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
                NetworkManager.Singleton.Shutdown();
            // Принудительно закрыть приложение
            Application.Quit();
        }
    }
}
