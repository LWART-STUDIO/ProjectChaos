using System;
using Game.Scripts.Client.Logic.Player;
using Game.Scripts.Server;
using Game.Scripts.Services.Input;
using Game.Scripts.Services.UI;
using Michsky.MUIP;
using Sisus.Init;
using UnityEngine;

namespace Game.Scripts.Client.UI
{
    public class EscMenu : MonoBehaviour
    {
        [SerializeField] private ModalWindowManager _modalWindowManager;
        private UIService _uiService => Service<UIService>.Instance;
        private PlayerInputActions _inputActions;
        private bool _opened => _modalWindowManager.isOn;
        private InputService _input => Service<InputService>.Instance;

        private void Start()
        {
            _inputActions = new PlayerInputActions();
            _inputActions.Enable();
            _modalWindowManager.gameObject.SetActive(true);
        }

        private void Update()
        {
            if(_input.InputBlocked)
                return;
            if (_inputActions.Menu.EscMenu.WasPressedThisFrame())
            {
                if (!_opened)
                    OpenWindow();
                else
                    CloseWindow();
            }
        }
        public void ExitToMenu()
        {
            CloseWindow();
            ConnectionStarter connectionStarter = FindAnyObjectByType<ConnectionStarter>();
            if (connectionStarter != null&& connectionStarter.IsFromLobby)
            {
                connectionStarter.LeaveLobby();
            }
            _uiService.ExitToMenu();
            LeaveLobbyHandler.LeaveAnyLobby();
        }
        public void ExitToLobby()
        {
            CloseWindow();
            _uiService.ExitToLobby();
        }
        public void CloseWindow()
        {
            if(PlayerHealth.AllPlayers.Count<2)
                Time.timeScale = 1;
            _modalWindowManager.CloseWindow();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
        }

        public void OpenWindow()
        {
            if(PlayerHealth.AllPlayers.Count<2)
                Time.timeScale = 0;
            _modalWindowManager.OpenWindow();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnDestroy()
        {
            _inputActions.Disable();
            _inputActions.Dispose();
            
        }
    }
}
