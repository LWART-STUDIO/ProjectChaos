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
        private InputService _inputService => Service<InputService>.Instance;
        private bool _opened => _modalWindowManager.isOn;

        private void Update()
        {
            if (_inputService.WasActionPressed(InputAction.Escape))
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
            _uiService.ExitToMenu();
        }
        public void ExitToLobby()
        {
            CloseWindow();
            _uiService.ExitToLobby();
        }
        public void CloseWindow()
        {
            _inputService.UnlockPlayerMovementInput();
            _modalWindowManager.CloseWindow();
            Cursor.visible = false;
        }

        public void OpenWindow()
        {
            _inputService.BlockPlayerMovementInput();
            _modalWindowManager.OpenWindow();
            Cursor.visible = true;
        }
    }
}
