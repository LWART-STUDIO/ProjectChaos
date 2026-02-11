
using Game.Scripts.Services.Input;
using Game.Scripts.Services.UI;
using Michsky.MUIP;
using Sisus.Init;
using UnityEngine;

namespace Game.Scripts.Client.UI.Game.EndGamePanel
{
    public class EndGamePanel : MonoBehaviour
    {
        [SerializeField] private ModalWindowManager _modalWindowManager;
        private UIService _uiService => Service<UIService>.Instance;
        private bool _opened => _modalWindowManager.isOn;
        private InputService _input => Service<InputService>.Instance;
        public void CloseWindow()
        {
            _input.UnblockInput();
            _modalWindowManager.CloseWindow();
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void OpenWindow()
        {
            _input.BlockInput();
            _modalWindowManager.OpenWindow();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
