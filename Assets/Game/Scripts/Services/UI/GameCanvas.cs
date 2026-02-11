using System;
using System.Collections;
using System.Runtime.InteropServices.ComTypes;
using Game.Scripts.Client.UI;
using Game.Scripts.Client.UI.Lobby;
using Game.Scripts.Client.UI.Menu;
using Game.Scripts.Services.ResourceLoader;
using Game.Scripts.Services.StaticService;
using Sisus.Init;
using UnityEngine;

namespace Game.Scripts.Services.UI
{
    public class GameCanvas : MonoBehaviour
    {
        [SerializeField] private MenuManager _menuManager;
        private ResourceLoaderService _resourceLoaderService => Service<ResourceLoaderService>.Instance;
        private LobbyUI _lobbyUI;
        private MainMenu _mainMenu;

        public MenuManager GetMenuManager()
        {
            return _menuManager;
        }

        public LobbyUI GetLobbyUI()
        {
            _lobbyUI = FindFirstObjectByType<LobbyUI>(FindObjectsInactive.Include);
            if (_lobbyUI != null)
            {
                _lobbyUI.gameObject.SetActive(true);
                return _lobbyUI;
            }
               
            GameObject lobbyUI = _resourceLoaderService.Load<GameObject>(StaticPath.LobbyUI);
            if (lobbyUI != null)
            {
                _lobbyUI = lobbyUI.GetComponent<LobbyUI>();
                _lobbyUI = Instantiate(_lobbyUI, null);
                _lobbyUI.gameObject.SetActive(true);
                return _lobbyUI;
            }
            return null;
        }

        public void HideLobbyUI()
        {
            if (_lobbyUI == null)
                GetLobbyUI();
            _lobbyUI.gameObject.SetActive(false);
        }
        

     
        
    }
}
