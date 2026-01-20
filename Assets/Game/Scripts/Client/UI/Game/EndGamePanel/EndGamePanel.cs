
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
        public void CloseWindow()
        {
            _modalWindowManager.CloseWindow();
            Cursor.visible = false;
        }

        public void OpenWindow()
        {
            _modalWindowManager.OpenWindow();
            Cursor.visible = true;
        }
    }
}
