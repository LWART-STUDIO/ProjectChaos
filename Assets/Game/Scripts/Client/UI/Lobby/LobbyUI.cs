using System.Collections.Generic;
using Game.Scripts.Client.Logic;
using Game.Scripts.Server;
using Game.Scripts.Services;
using Game.Scripts.Services.Lobby;
using Game.Scripts.Services.Scene;
using Game.Scripts.Services.Steam;
using Game.Scripts.Services.UI;
using Sisus.Init;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Client.UI.Lobby
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

        private ulong currentLobby;
        private GameCanvas _gameCanvas=>Service<UIService>.Instance.GetGameCanvas();
        private ILobbyService _lobbyService=>Service<ServiceInitor>.Instance.LobbyService;
        
        private Dictionary<ulong, GameObject> playerIcons = new Dictionary<ulong, GameObject>();

        private void Awake()
        {
            _lobbyService.OnPlayerJoined+= OnPlayerJoined;
            _lobbyService.OnPlayerLeft+= OnPlayerLeft;
            _coppyButton?.onClick.AddListener(CoppyID);
            _lobbyExitButton?.onClick.AddListener(LeaveLobby);
            _playButton?.onClick.AddListener(OnPlayClicked);
            currentLobby = _lobbyService.LobbyId;
            if(_lobbyIdText!=null) 
                _lobbyIdText.text = $"Lobby ID: {currentLobby.ToString()}";
            UpdatePlayButton();
            if (GameSession.Instance != null)
            {
                GameSession.Instance.IsGameStarted.OnValueChanged += (_, _) => UpdatePlayButton();
            }
            DontDestroyOnLoad(gameObject);
        }
        private void UpdatePlayButton()
        {
            if (_playButton == null) return;

            bool isGameStarted = GameSession.Instance?.IsGameStarted.Value == true;
            if (NetworkManager.Singleton.IsHost)
            {
                // Хост: кнопка "Start Game", видна всегда
                _playButton.gameObject.SetActive(true);
                // Можно поменять текст: "Start Game" / "Game Started"
            }
            else
            {
                // Клиент: кнопка "Join Game", только если игра идёт
                _playButton.gameObject.SetActive(isGameStarted);
            }
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

        private void Update()
        {
            if(NetworkManager.Singleton==null)
                return;
            if (NetworkManager.Singleton.IsHost)
            {
                _playButton.gameObject.SetActive(true);
                return; 
            }
            if(GameSession.Instance?.IsGameStarted.Value == true)
                _playButton.gameObject.SetActive(true);
            else
                _playButton.gameObject.SetActive(false);
        }

        public void CoppyID()
        {
            TextEditor textEditor = new TextEditor();
            textEditor.text = currentLobby.ToString();
            textEditor.SelectAll();
            textEditor.Copy();
            Debug.Log($"ID было скопированно в буфер обмена: {textEditor.text}");
        }

        public void LeaveLobby()
        {
            _lobbyService.LeaveLobby();
            Service<SceneService>.Instance.LoadScene(SceneMapper.MainMenu);
            _gameCanvas.HideLobbyUI();

        }

        private void OnDestroy()
        {
            _lobbyService.OnPlayerJoined-= OnPlayerJoined;
            _lobbyService.OnPlayerLeft-= OnPlayerLeft;
            _coppyButton?.onClick.RemoveListener(CoppyID);
            _lobbyExitButton?.onClick.RemoveListener(LeaveLobby);
        }

        private void OnPlayClicked()
        {
            if (NetworkManager.Singleton.IsHost)
            {
                // Хост запускает игру
                GameSession.Instance?.StartGame();
            }
            else
            {
                // Клиент присоединяется к уже идущей игре
                GameSession.Instance?.JoinGame();
            }
            Service<UIService>.Instance.GetGameCanvas().HideLobbyUI();
            PlayerSpawner.instance.SpawnPlayersRpc(NetworkManager.Singleton.LocalClientId);
            Cursor.visible = false;
        }
        

    }
}