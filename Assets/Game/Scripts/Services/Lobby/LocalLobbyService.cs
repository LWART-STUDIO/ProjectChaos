using System;
using System.Collections.Generic;
using Game.Scripts.Client.Logic;
using Game.Scripts.Server;
using Game.Scripts.Services.GameFlow;
using Game.Scripts.Services.ResourceLoader;
using Game.Scripts.Services.Steam;
using Sisus.Init;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Game.Scripts.Services.Lobby
{
    [Service(typeof(LocalLobbyService),FindFromScene = true,LazyInit = true)]
    public class LocalLobbyService : MonoBehaviour, IService, ILobbyService
    {
        [SerializeField] private GameSession _gameSessionPrefab;
        [SerializeField] private PlayerSpawner _playerSpawnerPrefab;
        private Dictionary<ulong, LobbyPlayer> _players = new();

        public event Action<LobbyPlayer> OnPlayerJoined;
        public event Action<LobbyPlayer> OnPlayerLeft;
        public event Action OnLobbyEntered;
        private ResourceLoaderService _resourceLoader=>Service<ResourceLoaderService>.Instance;
        public event Action OnLobbyCreated;

        private bool _wasAwaked;
        private bool _wasStarted;

        private GameFlowService _gameFlowService => Service<GameFlowService>.Instance;

        private ulong _hostId;
        public ulong LobbyId { get; private set; }
        private NetworkVariable<int> _playerCount = new NetworkVariable<int>(0,NetworkVariableReadPermission.Everyone);

        public void LocalAwake()
        {
            if (_wasAwaked)
                return;

            NetworkManager network = FindFirstObjectByType<NetworkManager>();
            if (network == null)
            {
                Debug.LogError("❌ NetworkManager not found in scene!");
                return;
            }

            network.NetworkConfig.NetworkTransport = network.GetComponent<UnityTransport>();
            _wasAwaked = true;
        }

        public void LocalStart()
        {
            if (_wasStarted)
                return;

            if (NetworkManager.Singleton == null)
                return;

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

            _wasStarted = true;
        }

        public void LocalUpdate(float deltaTime)
        {

        }

        public void CreateLobby()
        {
            _players.Clear();
            Debug.Log("🟡 Creating Local Lobby...");
            
            NetworkManager.Singleton.StartHost();
            _hostId = NetworkManager.Singleton.LocalClientId;
            LobbyId = _hostId; // используем hostId как LobbyId
            if (GameSession.Instance == null)
            {
                var sessionObj = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(
                    _gameSessionPrefab.GetComponent<NetworkObject>());
            }

            if (PlayerSpawner.Instance== null)
            {
                var sessionObj = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(
                    _playerSpawnerPrefab.GetComponent<NetworkObject>());
            }
            // Создаём игрока-хоста
            AddPlayer(_hostId);

            OnLobbyCreated?.Invoke();
            OnLobbyEntered?.Invoke();

            Debug.Log($"🟢 Local lobby created. HostID = {_hostId}");
        }

        public void JoinLobby()
        {
            Debug.Log("🟡 Joining Local Lobby...");

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.ConnectionData.Address = "127.0.0.1";
            transport.ConnectionData.Port = 7778;

            NetworkManager.Singleton.StartClient();

            // При подключении клиент вызовет OnClientConnected()
        }

        public void LeaveLobby()
        {
            Debug.Log("🔴 Leaving Local Lobby...");

            _players.Clear();
            NetworkManager.Singleton.Shutdown();
            /*if(PlayerSpawner.instance!=null)
                Destroy(PlayerSpawner.instance.gameObject);*/
        }

        public void StartGameServer()
        {
            if (!NetworkManager.Singleton.IsHost)
                return;

            Debug.Log("🚀 Starting local game...");
            _gameFlowService.StartGameServerRpc();
        }

        private void OnClientConnected(ulong clientId)
        {
            Debug.Log($"🟢 Player connected: {clientId}");
            AddPlayer(clientId);

            // Если это не хост — значит клиент успешно вошёл
            if (clientId == NetworkManager.Singleton.LocalClientId && !NetworkManager.Singleton.IsHost)
            {
                OnLobbyEntered?.Invoke();
            }
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (!_players.ContainsKey(clientId))
                return;

            var player = _players[clientId];
            _players.Remove(clientId);

            Debug.Log($"🔴 Player left lobby: {clientId}");
            OnPlayerLeft?.Invoke(player);
        }

        private void AddPlayer(ulong clientId)
        {
            if (_players.ContainsKey(clientId))
                return;

            string nick = $"Player_{clientId}";
            Texture2D avatar = GeneratePlaceholderAvatar(clientId);

            var player = new LobbyPlayer(clientId, nick, avatar);
            _players[clientId] = player;

            OnPlayerJoined?.Invoke(player);
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
    }
}
