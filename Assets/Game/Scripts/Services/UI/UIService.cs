using Game.Scripts.Services.ResourceLoader;
using Game.Scripts.Services.StaticService;
using Sisus.Init;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Scripts.Services.UI
{
    [Service]
    public class UIService : MonoBehaviour, IService
    {
        private ResourceLoaderService _resourceLoaderService => Service<ResourceLoaderService>.Instance;

        private GameCanvas _gameCanvas;

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
            //throw new System.NotImplementedException();
        }

        public void LocalStart()
        {
            // throw new System.NotImplementedException();
        }

        public void LocalUpdate(float deltaTime)
        {
            if(SceneManager.GetActiveScene().name == "MainMenu")
                return;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (NetworkManager.Singleton.IsHost)
                {
                    NetworkManager.Singleton.SceneManager.LoadScene("MainMenu",LoadSceneMode.Single);
                    Service<ServiceInitor>.Instance.LobbyService.LeaveLobby();
                    NetworkManager.Singleton.Shutdown();
                }
                else
                {
                    SceneManager.LoadScene("MainMenu",LoadSceneMode.Single);
                    Service<ServiceInitor>.Instance.LobbyService.LeaveLobby();
                    NetworkManager.Singleton.Shutdown();
                }
                    
            }
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
    }
}
