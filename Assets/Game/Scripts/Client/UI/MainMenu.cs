using System;
using Game.Scripts.Services;
using Game.Scripts.Services.Lobby;
using Game.Scripts.Services.Steam;
using Game.Scripts.Services.UI;
using Sisus.Init;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Client.UI
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _connectButton;
        private GameCanvas _gameCanvas=>Service<UIService>.Instance.GetGameCanvas();
        private ILobbyService _lobbyService=>Service<ServiceInitor>.Instance.LobbyService;

        private void Start()
        {
            _newGameButton.onClick.AddListener(StartNewGame);
            _connectButton.onClick.AddListener(ConnectToLobby);
            if (_lobbyService is SteamService steam)
                steam.OnLobbyEntered += OnLobbyEntered;
        }

        private void StartNewGame()
        {
           // Service<UIService>.Instance.GetGameCanvas().GetLobbyUI();
           _lobbyService.CreateLobby();
            
            
        }
        private void ConnectToLobby()
        { 
            if (_lobbyService is SteamService steam)
                SteamFriends.ActivateGameOverlay("Friends");
            else
            {
                Service<UIService>.Instance.GetGameCanvas().GetLobbyUI();
                Service<ServiceInitor>.Instance.LobbyService.JoinLobby();
                _gameCanvas.HideMainMenu();
            }
            
        }
        private void OnLobbyEntered()
        {
            Debug.Log("Steam lobby entered, showing lobby UI...");
            _gameCanvas.HideMainMenu();
            _gameCanvas.GetLobbyUI();
        }

        private void OnDestroy()
        {
            _newGameButton.onClick.RemoveListener(StartNewGame);
            _connectButton.onClick.RemoveListener(ConnectToLobby);
            if (_lobbyService is SteamService steam)
                steam.OnLobbyEntered -= OnLobbyEntered;
        }
    }
}
