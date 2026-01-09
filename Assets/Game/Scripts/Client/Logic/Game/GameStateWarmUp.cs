using System.Collections;
using System.Collections.Generic;
using Game.Scripts.Client.Logic.Player;
using Game.Scripts.Server;
using Game.Scripts.Services.Save;
using PurrNet;
using PurrNet.Modules;
using PurrNet.StateMachine;
using Sisus.Init;
using Steamworks;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Game
{
    public class GameStateWarmUp : StateNode
    {
        [SerializeField] private int minPlayers=1;
        private ConnectionStarter _connectionStarter;
        private Dictionary<PlayerID, PlayerClassType> _playerClases;

        private void Awake()
        {
            _playerClases = new Dictionary<PlayerID, PlayerClassType>();
            
        }
        public override void Enter(bool asServer)
        {
            base.Enter(asServer);

            if (asServer)
            {
                InstanceHandler.NetworkManager.Subscribe<PlayerClassType>(OnClassDataReceived);
                StartCoroutine(WaitForPlayer());
            }
            else
            {
                
                // клиент отправляет СВОЙ выбор
                InstanceHandler.NetworkManager.SendToServer(
                    Service<SaveService>.Instance.LoadPlayerClassType()
                );
            }
        }

        private void OnClassDataReceived(PlayerID player,PlayerClassType classType,bool asServer )
        {
            if (!asServer)
                return;
            Debug.Log($"{player} has been connected with {classType}");
            _playerClases[player] = classType;
        }

        public override void Exit(bool asServer)
        {
            base.Exit(asServer);
        }



        private IEnumerator WaitForPlayer()
        {
            _connectionStarter = FindAnyObjectByType<ConnectionStarter>();
            if (_connectionStarter == null)
            {
                Debug.LogError("No ConnectionStarter found!");
                yield break;
            }
            while (!_connectionStarter.Coneted)
                yield return null;
            if (_connectionStarter.IsFromLobby)
            {
                if (!ulong.TryParse(_connectionStarter.LobbyId, out ulong lobbyId))
                {
                    Debug.Log($"Failed to parse lobby ID",this);
                    yield break;;
                }
                minPlayers= SteamMatchmaking.GetNumLobbyMembers(new CSteamID(lobbyId));
            }
            while (networkManager.players.Count < minPlayers)
                yield return null;
            while (_playerClases.Count < networkManager.players.Count)
            {
                foreach (var player  in networkManager.players)
                {
                    if(networkManager.localPlayer==player)
                        _playerClases[player] =Service<SaveService>.Instance.LoadPlayerClassType();
                }
                yield return null;
            }
            machine.Next(_playerClases);
        }
        private void OnDisable()
        {
            if(InstanceHandler.NetworkManager)
                InstanceHandler.NetworkManager.Unsubscribe<PlayerClassType>(OnClassDataReceived);
        }

    }
}
