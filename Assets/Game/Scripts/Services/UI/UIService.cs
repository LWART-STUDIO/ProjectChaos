using System;
using Game.Scripts.Client.UI;
using Game.Scripts.Client.UI.Game;
using Game.Scripts.Client.UI.Game.EndGamePanel;
using Game.Scripts.Client.UI.Game.PlayerUI;
using Game.Scripts.Services.ResourceLoader;
using Game.Scripts.Services.Scene;
using Game.Scripts.Services.StaticService;
using PurrNet;
using Sisus.Init;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Scripts.Services.UI
{
    [Service]
    public class UIService : MonoBehaviour, IService
    {
        private ResourceLoaderService _resourceLoaderService => Service<ResourceLoaderService>.Instance;

        private GameCanvas _gameCanvas;
        private EscMenu _escMenu;
        private LevelUpPanel _levelUpPanel;
        private PlayerInGameUI _playerInGameUI;
        private EndGamePanel _endGamePanel;
        private PlayerSkillTree  _playerSkillTree;
        public LevelUpPanel LevelUpPanel => GetLevelUpPanel();

        /*public   void LocalAwake()
        {
            if(_serviceInitor.LevelService!=null)
                _serviceInitor.LevelService.OnLevelFinished += LevelEnd;
            DestroyAll();
            GetGameCanvas();
        }*/
        public void LocalAwake()
        {
            GetGameCanvas();
            GetEscMenu();
        }

        public void LocalStart()
        {
            // throw new System.NotImplementedException();
        }

        public void LocalUpdate(float deltaTime)
        {
            if (SceneManager.GetActiveScene().name == "MainMenu")
                return;
            /*if (Input.GetKeyDown(KeyCode.Escape))
            {


            }*/
        }

        public void ExitToLobby()
        {

        }

        public void ExitToMenu()
        {
            Service<SceneService>.Instance.LoadScene(SceneMapper.LobbySample);
            Cursor.visible = true;
        }

        public GameCanvas GetGameCanvas()
        {
            _gameCanvas = FindFirstObjectByType<GameCanvas>();
            if (_gameCanvas != null)
                return _gameCanvas;
            GameObject gameCanvas = _resourceLoaderService.Load<GameObject>(StaticPath.GameCanvasPath);
            if (gameCanvas != null)
            {
                _gameCanvas = gameCanvas.GetComponent<GameCanvas>();
                _gameCanvas = Instantiate(_gameCanvas, null);
                return _gameCanvas;
            }

            return null;

        }

        public EscMenu GetEscMenu()
        {
            _escMenu = FindFirstObjectByType<EscMenu>();
            if (_escMenu != null)
                return _escMenu;
            GameObject escMenu = _resourceLoaderService.Load<GameObject>(StaticPath.EscMenu);
            if (escMenu != null)
            {
                _escMenu = escMenu.GetComponent<EscMenu>();
                _escMenu = Instantiate(_escMenu, _gameCanvas.transform);
                return _escMenu;
            }

            return null;

        }

        public LevelUpPanel GetLevelUpPanel()
        {
            _levelUpPanel = FindFirstObjectByType<LevelUpPanel>(FindObjectsInactive.Include);
            if (_levelUpPanel != null)
                return _levelUpPanel;
            GameObject levelUpPanel = _resourceLoaderService.Load<GameObject>(StaticPath.LevelUpMenu);
            if (levelUpPanel != null)
            {
                _levelUpPanel = levelUpPanel.GetComponent<LevelUpPanel>();
                _levelUpPanel = Instantiate(_levelUpPanel, _gameCanvas.transform);
                return _levelUpPanel;
            }

            return null;

        }
        public PlayerInGameUI GetPlayerInGameUI()
        {
            _playerInGameUI = FindFirstObjectByType<PlayerInGameUI>(FindObjectsInactive.Include);
            if (_playerInGameUI != null)
                return _playerInGameUI;
            GameObject playerInGameUI = _resourceLoaderService.Load<GameObject>(StaticPath.PlayerInGameUI);
            if (playerInGameUI != null)
            {
                _playerInGameUI = playerInGameUI.GetComponent<PlayerInGameUI>();
                _playerInGameUI = Instantiate(_playerInGameUI, _gameCanvas.transform);
                return _playerInGameUI;
            }

            return null;

        }
        public EndGamePanel GetEndGamePanel()
        {
            _endGamePanel = FindFirstObjectByType<EndGamePanel>(FindObjectsInactive.Include);
            if (_endGamePanel != null)
                return _endGamePanel;
            GameObject endGamePanel = _resourceLoaderService.Load<GameObject>(StaticPath.EndGamePanel);
            if (endGamePanel != null)
            {
                _endGamePanel = endGamePanel.GetComponent<EndGamePanel>();
                _endGamePanel = Instantiate(_endGamePanel, _gameCanvas.transform);
                return _endGamePanel;
            }

            return null;

        }
        public PlayerSkillTree GetPlayerSkillTree()
        {
            _playerSkillTree = FindFirstObjectByType<PlayerSkillTree>(FindObjectsInactive.Include);
            if (_playerSkillTree != null)
                return _playerSkillTree;
            GameObject playerSkillTree = _resourceLoaderService.Load<GameObject>(StaticPath.PlayerSkillTree);
            if (playerSkillTree != null)
            {
                _playerSkillTree = playerSkillTree.GetComponent<PlayerSkillTree>();
                _playerSkillTree = Instantiate(_playerSkillTree, _gameCanvas.transform);
                return _playerSkillTree;
            }

            return null;

        }
    }
}
