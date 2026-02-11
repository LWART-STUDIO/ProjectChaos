using Game.Scripts.Server;
using Game.Scripts.Services.Audio;
using Game.Scripts.Services.Scene;
using Michsky.UI.Reach;
using PurrLobby;
using Sisus.Init;
using UnityEngine;

namespace Game.Scripts.Client.UI.Menu
{
    public class MenuManager : MonoBehaviour
    {

        [SerializeField] private LobbyManager _lobbyManager;
        [SerializeField] private LobbyMemberList _memberList;
        [SerializeField] private PanelManager _panelManager;
        [SerializeField] private ButtonManager _playButton;
        [SerializeField] private GameObject _playButtonObject;
        [SerializeField] private GameObject _createButtonObject;
        [SerializeField]private bool _allPlayersReady = false;
        [SerializeField]private bool _inLobby = false;
        private bool _isReady = false;

        private void Awake()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private void Start()
        {
            AudioService.instance.PlayMusicCrossfade("MenuMusic");
        }
        public void OpenLobbyScreen()
        {
            _inLobby = true;
            _allPlayersReady = false;
            _createButtonObject.SetActive(true);
            _playButtonObject.SetActive(false);
        }

        private void Update()
        {
            if(!_inLobby)
                return;
            if(!_lobbyManager.CurrentLobby.IsValid)
                return;
            _allPlayersReady = true;
            foreach (var member in _lobbyManager.CurrentLobby.Members)
            {
                if (!member.IsReady)
                    _allPlayersReady = false;
            }
            _playButton.Interactable(_allPlayersReady);
        }

        public void EnterPlaySolo()
        {

        }

        public void EnterPlayMultiplayer()
        {
            _allPlayersReady = true;
        }

        #region Events

        public void OnRoomJoined()
        {
            _panelManager.OpenPanel("Multiplayer");
            _allPlayersReady = false;
        }

        public void OnRoomLeft()
        {
            _panelManager.OpenFirstPanel();
            _allPlayersReady = false;
            _isReady = false;
            _inLobby = false;
            _memberList.DisableAllMembers();
        }

        public void OnRoomUpdate(PurrLobby.Lobby lobby)
        {
            _memberList.LobbyDataUpdate(lobby,_lobbyManager.CurrentProvider.GetLocalUserIdAsync().Result);
        }

        public void OnBrowseClicked()
        {
        }

        public void OnRoomCreateClicked()
        {
            _lobbyManager.CreateRoom();
            _createButtonObject.SetActive(false);
            _playButtonObject.SetActive(true);
            
        }

        public void OnJoiningRoom()
        {

        }

        public void OnLeaveBrowseClicked()
        {

        }

        public void LeaveLobby()
        {
            if(_lobbyManager.CurrentLobby.IsValid)
                _lobbyManager.LeaveLobby();
            else
                OnRoomLeft();
            LeaveLobbyHandler.LeaveAnyLobby();
        }

        public bool AllPlayersReady()
        {
            return _allPlayersReady;
        }

        public void LocalPlayerReady()
        {
            if(_lobbyManager.CurrentLobby.IsValid)
                _lobbyManager.LocalReady();
            _isReady = true;

        }
        public void LocalPlayerUnReady()
        {
            if(_lobbyManager.CurrentLobby.IsValid)
                _lobbyManager.LocalUnReady();
            _isReady = false;

        }

        public void PressStartButton()
        {
            _lobbyManager.StartGame();
        }


        public void StartGame()
        {
            if (_allPlayersReady == false)
                return;
            _lobbyManager.SetLobbyStarted();
            Service<SceneService>.Instance.LoadScene(SceneMapper.Game);
        }

    #endregion
    }
}
