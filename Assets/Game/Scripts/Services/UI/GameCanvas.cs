using Game.Scripts.Client.UI;
using Game.Scripts.Services.ResourceLoader;
using Game.Scripts.Services.StaticService;
using Sisus.Init;
using UnityEngine;

namespace Game.Scripts.Services.UI
{
    public class GameCanvas : MonoBehaviour
    {
        private ResourceLoaderService _resourceLoaderService => Service<ResourceLoaderService>.Instance;
        private LobbyUI _lobbyUI;
        private MainMenu _mainMenu;

        public LobbyUI GetLobbyUI()
        {
            _lobbyUI = FindFirstObjectByType<LobbyUI>();
            if (_lobbyUI != null)
                return _lobbyUI;
            GameObject lobbyUI = _resourceLoaderService.Load<GameObject>(StaticPath.LobbyUI);
            if (lobbyUI != null)
            {
                _lobbyUI = lobbyUI.GetComponent<LobbyUI>();
                _lobbyUI = Instantiate(_lobbyUI, transform);
                return _lobbyUI;
            }
            return null;
        }

        public void HideLobbyUI()
        {
            if (_lobbyUI == null)
                GetLobbyUI();
            Destroy(_lobbyUI.gameObject);
            _lobbyUI = null;
        }
        public MainMenu GetMainMenu()
        {
            _mainMenu = FindFirstObjectByType<MainMenu>();
            if (_mainMenu != null)
                return _mainMenu;
            GameObject mainMenu = _resourceLoaderService.Load<GameObject>(StaticPath.MainMenu);
            if (mainMenu != null)
            {
                _mainMenu = mainMenu.GetComponent<MainMenu>();
                _mainMenu = Instantiate(_mainMenu, transform);
                return _mainMenu;
            }
            return null;
        }
        public void HideMainMenu()
        {
            if (_mainMenu == null)
                GetMainMenu();
            Destroy(_mainMenu.gameObject);
            _mainMenu = null;
        }
        
    }
}
