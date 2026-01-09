using System.Collections.Generic;
using Game.Scripts.Client.Logic;
using Game.Scripts.Services;
using Game.Scripts.Services.Scene;
using Game.Scripts.Services.UI;
using Sisus.Init;
using TMPro;

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

        
        private Dictionary<ulong, GameObject> playerIcons = new Dictionary<ulong, GameObject>();

        private void Awake()
        {

            _coppyButton?.onClick.AddListener(CoppyID);
            _lobbyExitButton?.onClick.AddListener(LeaveLobby);
            _playButton?.onClick.AddListener(OnPlayClicked);
            if(_lobbyIdText!=null) 
                _lobbyIdText.text = $"Lobby ID: {currentLobby.ToString()}";
            UpdatePlayButton();
            DontDestroyOnLoad(gameObject);
        }
        private void UpdatePlayButton()
        {
            if (_playButton == null) return;

  
        }
        private void OnPlayerJoined()
        {
            
        }

        private void OnPlayerLeft()
        {

        }

        private void Update()
        {
            
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
            Service<SceneService>.Instance.LoadScene(SceneMapper.LobbySample);
            _gameCanvas.HideLobbyUI();

        }

        private void OnDestroy()
        {
            _coppyButton?.onClick.RemoveListener(CoppyID);
            _lobbyExitButton?.onClick.RemoveListener(LeaveLobby);

        }

        private void OnPlayClicked()
        {
           
            Cursor.visible = false;
        }
        

    }
}