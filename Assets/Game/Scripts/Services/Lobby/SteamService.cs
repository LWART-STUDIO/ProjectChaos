using System;
using System.Collections.Generic;
using Game.Scripts.Server;
using Game.Scripts.Services.GameFlow;
using Game.Scripts.Services.ResourceLoader;
using Game.Scripts.Services.Steam;
using Netcode.Transports;
using SaintsField.Playa;
using Sisus.Init;
using Steamworks;
using Unity.Netcode;
using UnityEngine;

namespace Game.Scripts.Services.Lobby
{
    [Service(typeof(SteamService),FindFromScene = true, LazyInit = true)]
    public class SteamService : MonoBehaviour, IService, ILobbyService
    {
        [SerializeField] private GameSession _gameSessionPrefab;
        private Dictionary<ulong, LobbyPlayer> _players = new();
        public event Action<LobbyPlayer> OnPlayerJoined;
        public event Action<LobbyPlayer> OnPlayerLeft;
        public event Action OnLobbyEntered;

        private Callback<LobbyCreated_t> _onLobbyCreated;
        private Callback<LobbyEnter_t> _onLobbyEntered;
        private Callback<GameLobbyJoinRequested_t> _onLobbyJoinRequest;
        private Callback<LobbyChatUpdate_t> _onLobbyChatUpdate;
        
        private ResourceLoaderService _resourceLoader=>Service<ResourceLoaderService>.Instance;
        private bool _wasAwaked;
        private bool _wasStarted;
        private GameFlowService _gameFlowService=>Service<GameFlowService>.Instance;

  
        public ulong LobbyId { get; private set; }

        public void LocalAwake()
        {
            if(_wasAwaked)
                return;
            if (!SteamAPI.IsSteamRunning())
            {
                Debug.LogError("❌ Запусти стим и перезапусти игру");
                return;
            }
          
            if (!SteamAPI.Init())
            {
                Debug.LogError("❌ Steam API init failed!");
                return;
            }

            Debug.Log("✅ Steam initialized.");
            _onLobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            _onLobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEnteredFunc);
            _onLobbyJoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyJoinRequested);
            _onLobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
            _wasAwaked = true;
        }

        public void LocalStart()
        {
            if(_wasStarted)
                return;
            _wasStarted = true;
        }

        private void OnLobbyJoinRequested(GameLobbyJoinRequested_t param)
        {
            SteamMatchmaking.JoinLobby(param.m_steamIDLobby);
        }
        
        [Button]
        public void CreateLobby()
        {
            _players = new Dictionary<ulong, LobbyPlayer>();
            Debug.Log("🟡 Creating Steam lobby...");
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 4);
            SteamMatchmaking.SetLobbyData(new CSteamID(LobbyId), "HostAddress", SteamUser.GetSteamID().m_SteamID.ToString());
            NetworkManager.Singleton.StartHost();
   
        }

        private void OnLobbyCreated(LobbyCreated_t callback)
        {
            if (callback.m_eResult == EResult.k_EResultOK)
            {
                LobbyId = new CSteamID( callback.m_ulSteamIDLobby).m_SteamID;
                Debug.Log("🟢 Lobby was created: ");
                if (GameSession.Instance == null)
                {
                    var sessionObj = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(
                        _gameSessionPrefab.GetComponent<NetworkObject>());
                }
                    
                return;
            }
            Debug.LogError("❌ Failed to create Steam lobby: " + callback.m_eResult);
            return;
        }

        [Button]
        public void JoinLobby()
        {
            Debug.Log($"🟡 Joining Steam lobby {LobbyId}...");
            
            SteamMatchmaking.JoinLobby(new CSteamID(LobbyId));
        }

        private void OnLobbyEnteredFunc(LobbyEnter_t callback)
        {
            if (!NetworkManager.Singleton.IsHost)
            {
                SteamNetworkingSocketsTransport transport 
                    =NetworkManager.Singleton.gameObject.GetComponent<SteamNetworkingSocketsTransport>();
                CSteamID hostId = SteamMatchmaking.GetLobbyOwner(new CSteamID(callback.m_ulSteamIDLobby));
                transport.ConnectToSteamID = hostId.m_SteamID;
                NetworkManager.Singleton.StartClient();
            }
            
            LobbyId = callback.m_ulSteamIDLobby;
            Debug.Log($"✅ Entered lobby: {LobbyId}");
            int memberCount = SteamMatchmaking.GetNumLobbyMembers(new CSteamID(LobbyId));
            for (int i = 0; i < memberCount; i++)
            { 
                CSteamID memberId = SteamMatchmaking.GetLobbyMemberByIndex(new CSteamID(LobbyId), i);
                if (_players.ContainsKey(memberId.m_SteamID))
                    continue;
                string memberName = SteamFriends.GetFriendPersonaName(memberId);
                Texture2D avatar = MySteamUtils.LoadSteamImage(memberId);
                var player = new LobbyPlayer(memberId.m_SteamID, memberName, avatar);
                _players[memberId.m_SteamID] = player;
                OnLobbyEntered?.Invoke();
                OnPlayerJoined?.Invoke(player);
                
            }
            
        }

        private void OnLobbyChatUpdate(LobbyChatUpdate_t callback)
        {
            var change = (EChatMemberStateChange)callback.m_rgfChatMemberStateChange;
            var memberId = new CSteamID(callback.m_ulSteamIDUserChanged);

            // Игрок зашёл
            if (change.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeEntered))
            {
                Debug.Log($"🟢 Player joined Steam lobby: {memberId.m_SteamID}");

                if (!_players.ContainsKey(memberId.m_SteamID))
                {
                    string memberName = SteamFriends.GetFriendPersonaName(memberId);
                    Texture2D avatar = MySteamUtils.LoadSteamImage(memberId);
                    var player = new LobbyPlayer(memberId.m_SteamID, memberName, avatar);
                    _players[memberId.m_SteamID] = player;

                    // ✅ обновляем UI
                    OnPlayerJoined?.Invoke(player);
                }
            }

            // Игрок вышел
            if (change.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeLeft) ||
                change.HasFlag(EChatMemberStateChange.k_EChatMemberStateChangeDisconnected))
            {
                Debug.Log($"🔴 Player left Steam lobby: {memberId.m_SteamID}");
                if (_players.ContainsKey(memberId.m_SteamID))
                {
                    OnPlayerLeft?.Invoke(_players[memberId.m_SteamID]);
                    _players.Remove(memberId.m_SteamID);
                }
            }
        }


        
        private void OnDestroy()
        {
            _onLobbyCreated?.Unregister();
            _onLobbyEntered?.Unregister();
            _onLobbyJoinRequest?.Unregister();
            _onLobbyChatUpdate?.Unregister();
        }

        public void LocalUpdate(float deltaTime)
        {
            if(!SteamAPI.IsSteamRunning())
                return;
            SteamAPI.RunCallbacks();
        }

        public void LeaveLobby()
        {
           
            SteamMatchmaking.LeaveLobby(new CSteamID(LobbyId));
            _players = new Dictionary<ulong, LobbyPlayer>();
            NetworkManager.Singleton.Shutdown();
            
        }

        public void StartGameServer()
        {
            if (!NetworkManager.Singleton.IsHost) 
                return;
            _gameFlowService.StartGameServerRpc();
        }
    }
}
