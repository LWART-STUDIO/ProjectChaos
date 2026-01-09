using System.Collections;
using PurrLobby;
using PurrNet;
using PurrNet.Logging;
using PurrNet.Steam;
using PurrNet.Transports;
using Steamworks;
//#if UNITY_EDITOR
using Unity.Multiplayer.PlayMode;
//#endif
using UnityEngine;

namespace Game.Scripts.Server
{
    public class ConnectionStarter : MonoBehaviour
    {
        private NetworkManager _networkManager;
        private UDPTransport _udpTransport;
        private SteamTransport _steamTransport;
        private LobbyDataHolder _lobbyDataHolder;
        private bool _isFromLobby;
        private string _lobbyId;
        public string LobbyId=>_lobbyId;
        public bool IsFromLobby=>_isFromLobby;
        private bool _coneted;
        public bool Coneted => _coneted;

        private void Awake()
        {
            if (!TryGetComponent(out _networkManager))
            {
                PurrLogger.LogError($"Failed to get {nameof(NetworkManager)} component.", this);
            }

            _lobbyDataHolder = FindFirstObjectByType<LobbyDataHolder>();
            if (_lobbyDataHolder)
                _isFromLobby = true;
        }

        private void Start()
        {
            if (!_networkManager)
            {
                PurrLogger.LogError($"Failed to start connection. {nameof(NetworkManager)} is null!", this);
                return;
            }
            if(_isFromLobby)
                StartFromLobby();
            else
                StartNormal();

        }

        private IEnumerator StartClient()
        {
            yield return new WaitForSeconds(1f);
            _networkManager.StartClient();
            _coneted = true;
        }

        private void StartNormal()
        {
            if (!TryGetComponent(out _udpTransport))
            {
                PurrLogger.LogError($"Failed to get {nameof(UDPTransport)} component.", this);
            }
            _networkManager.transport = _udpTransport;

            if (Application.isEditor)
            {
                var tags = CurrentPlayer.ReadOnlyTags();

                foreach (var tag in tags)
                {
                    if (tag == "Server")
                    {
                      _networkManager.StartServer();
                    }
                    StartCoroutine(StartClient());
                }
            }
        }

        public void LeaveLobby()
        {
            if (!_lobbyDataHolder)
            {
                PurrLogger.LogError($"Failed to start connection. {nameof(LobbyDataHolder)} is null!", this);
                return;
            }

            if (!_lobbyDataHolder.CurrentLobby.IsValid)
            {
                PurrLogger.LogError($"Failed to start connection. Lobby is invalid!", this);
                return;
            }
            if (!ulong.TryParse(_lobbyDataHolder.CurrentLobby.LobbyId, out ulong lobbyId))
            {
                Debug.Log($"Failed to parse lobby ID",this);
                return;
            }
            SteamMatchmaking.LeaveLobby(new CSteamID(lobbyId));
        }

        private void StartFromLobby()
        {
            if (!_lobbyDataHolder)
            {
                PurrLogger.LogError($"Failed to start connection. {nameof(LobbyDataHolder)} is null!", this);
                return;
            }

            if (!_lobbyDataHolder.CurrentLobby.IsValid)
            {
                PurrLogger.LogError($"Failed to start connection. Lobby is invalid!", this);
                return;
            }
            if (!TryGetComponent(out _steamTransport))
            {
                PurrLogger.LogError($"Failed to get {nameof(SteamTransport)} component.", this);
            }
            _networkManager.transport = _steamTransport;
            if (!ulong.TryParse(_lobbyDataHolder.CurrentLobby.LobbyId, out ulong lobbyId))
            {
                Debug.Log($"Failed to parse lobby ID",this);
                return;
            }
            _lobbyId = _lobbyDataHolder.CurrentLobby.LobbyId;   
            var lobbyOwner = SteamMatchmaking.GetLobbyOwner(new CSteamID(lobbyId));
            if (!lobbyOwner.IsValid())
            {
                Debug.Log("Failed To Get Lobby Owner", this);
                return;
            }

            _steamTransport.address = lobbyOwner.ToString();

#if UTP_LOBBYRELAY
            else if(_networkManager.transport is UTPTransport) {
                if(_lobbyDataHolder.CurrentLobby.IsOwner) {
                    (_networkManager.transport as UTPTransport).InitializeRelayServer((Allocation)_lobbyDataHolder.CurrentLobby.ServerObject);
                }
                (_networkManager.transport as UTPTransport).InitializeRelayClient(_lobbyDataHolder.CurrentLobby.Properties["JoinCode"]);
            }
#else
            //P2P Connection, receive IP/Port from server
#endif

            if (_lobbyDataHolder.CurrentLobby.IsOwner)
                _networkManager.StartServer();
            StartCoroutine(StartClient());
        }
    }
}