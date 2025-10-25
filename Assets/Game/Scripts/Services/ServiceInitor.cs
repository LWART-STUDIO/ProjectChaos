using System.Linq;
using Game.Scripts.Client.Logic;
using Game.Scripts.Services.Input;
using Game.Scripts.Services.Lobby;
using Game.Scripts.Services.ResourceLoader;
using Game.Scripts.Services.Scene;
using Game.Scripts.Services.StaticService;
using Game.Scripts.Services.UI;
using Sisus.Init;
using Unity.Multiplayer.Playmode;
using Unity.Netcode;
using UnityEngine;

namespace Game.Scripts.Services
{
    [Service(typeof(ServiceInitor),FindFromScene = true,LazyInit = true)]
    public class ServiceInitor : MonoBehaviour<SteamService,LocalLobbyService,UIService,InputService>
    {
        [SerializeField] private bool _useSteam;
        [SerializeField] private bool _debugHost;
        private ILobbyService _lobbyService;
        private UIService _uiService;
        private InputService _inputService;
        public ILobbyService LobbyService=>_lobbyService;
        private ResourceLoaderService _resourceLoaderService;
        private SceneService _sceneService;
        protected override void Init(SteamService steamService,
            LocalLobbyService localLobby,
            UIService uiService,
            InputService inputService)
        {
            if(_useSteam)
                _lobbyService = steamService;
            else
                _lobbyService = localLobby;
            _uiService = uiService;
            _inputService = inputService;
            _resourceLoaderService = Service<ResourceLoaderService>.Instance;
#if UNITY_EDITOR
            NetworkManager networkManager=FindFirstObjectByType<NetworkManager>(FindObjectsInactive.Include);
            if (networkManager == null)
                networkManager =Instantiate(_resourceLoaderService.Load<GameObject>(StaticPath.NetworkService)).GetComponent<NetworkManager>();
           
#endif
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            if (Service<ServiceInitor>.Instance == null)
                Service.SetInstance(this);
            if(_inputService == null)
                _inputService = Service<InputService>.Instance;
            _inputService.LocalAwake();
            if (_lobbyService == null)
            {
                if(_useSteam)
                    _lobbyService = Service<SteamService>.Instance;
                else
                    _lobbyService = Service<LocalLobbyService>.Instance;;
            }
            _lobbyService.LocalAwake();
            if(_uiService == null)
                _uiService = Service<UIService>.Instance;
            _uiService.LocalAwake();
            if(_sceneService == null)
                _sceneService = Service<SceneService>.Instance;
            _sceneService.LocalAwake();

        }
        private void Start()
        {
            _inputService.LocalStart();
            _lobbyService.LocalStart();
            _uiService.LocalStart();
#if UNITY_EDITOR
            if (_debugHost)
            {
                if (_sceneService.GetActiveScene() == SceneMapper.Game)
                {
                    if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer ||
                        NetworkManager.Singleton.IsClient)
                        return;
                   
                    _uiService.GetGameCanvas().GetLobbyUI();
                    if(CurrentPlayer.IsMainEditor)
                        _lobbyService.CreateLobby();
                    else
                        _lobbyService.JoinLobby();
                }

            }
               
#endif
           

        }

        private void Update()
        {
            _sceneService.LocalUpdate(Time.deltaTime);
            _inputService.LocalUpdate(Time.deltaTime);
            _lobbyService.LocalUpdate(Time.deltaTime);
            _uiService.LocalUpdate(Time.deltaTime);
        }

       
    }
}
