using System.Collections.Generic;
using Game.Scripts.Services;
using Game.Scripts.Services.Lobby;
using Game.Scripts.Services.Steam;
using Game.Scripts.Services.UI;
using Sisus.Init;
using Steamworks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Client.UI
{
    public class LobbyUI : MonoBehaviour
    {
        public Transform playerListContainer;
        public GameObject playerIconPrefab;
        [SerializeField] private TMP_Text _lobbyIdText;
        [SerializeField] private Button _coppyButton;
        [SerializeField] private Button _lobbyExitButton;
        [SerializeField] private Chat _chatBox;
        [SerializeField] private Button _playButton;

        private CSteamID currentLobby;
        private GameCanvas _gameCanvas=>Service<UIService>.Instance.GetGameCanvas();
        private ILobbyService _lobbyService=>Service<ServiceInitor>.Instance.LobbyService;
        
        private Dictionary<ulong, GameObject> playerIcons = new Dictionary<ulong, GameObject>();

        private void Awake()
        {
            _lobbyService.OnPlayerJoined+= OnPlayerJoined;
            _lobbyService.OnPlayerLeft+= OnPlayerLeft;
            _coppyButton?.onClick.AddListener(CoppyID);
            _lobbyExitButton?.onClick.AddListener(LeaveLobby);
            _playButton?.onClick.AddListener(StartGame);
            currentLobby = _lobbyService.LobbyId;
            if(_lobbyIdText!=null) 
                _lobbyIdText.text = $"Lobby ID: {currentLobby.m_SteamID.ToString()}";
            _playButton?.gameObject.SetActive(NetworkManager.Singleton.IsHost);
        }
        private void OnPlayerJoined(LobbyPlayer player)
        {
            if (playerIcons.ContainsKey(player.Id))
                return;
            GameObject iconGO = Instantiate(playerIconPrefab, playerListContainer);
            playerIcons[player.Id] = iconGO;

            var iconScript = iconGO.GetComponent<PlayerIcon>();
            iconScript.Setup(player);
        }

        private void OnPlayerLeft(LobbyPlayer player)
        {
            if (!playerIcons.TryGetValue(player.Id, out var go)) return;
            Destroy(go);
            playerIcons.Remove(player.Id);
        }

        public void CoppyID()
        {
            TextEditor textEditor = new TextEditor();
            textEditor.text = currentLobby.m_SteamID.ToString();
            textEditor.SelectAll();
            textEditor.Copy();
            Debug.Log($"ID было скопированно в буфер обмена: {textEditor.text}");
        }

        public void LeaveLobby()
        {
            _lobbyService.LeaveLobby();
            _gameCanvas.HideLobbyUI();
            _gameCanvas.GetMainMenu();
        }

        private void OnDestroy()
        {
            _lobbyService.OnPlayerJoined-= OnPlayerJoined;
            _lobbyService.OnPlayerLeft-= OnPlayerLeft;
            _coppyButton?.onClick.RemoveListener(CoppyID);
            _lobbyExitButton?.onClick.RemoveListener(LeaveLobby);
        }

        private void StartGame()
        {
            _lobbyService.StartGameServer();
        }
        

    }
}