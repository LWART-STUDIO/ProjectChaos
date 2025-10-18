using Game.Scripts.Services.Input;
using Game.Scripts.Services.Lobby;
using Game.Scripts.Services.UI;
using Sisus.Init;
using UnityEngine;

namespace Game.Scripts.Services
{
    [Service(typeof(ServiceInitor),FindFromScene = true,LazyInit = true)]
    public class ServiceInitor : MonoBehaviour<SteamService,LocalLobbyService,UIService,InputService>
    {
        [SerializeField] private bool _useSteam;
        private ILobbyService _lobbyService;
        private UIService _uiService;
        private InputService _inputService;
        public ILobbyService LobbyService=>_lobbyService;
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
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            if (Service<ServiceInitor>.Instance == null)
                Service.SetInstance(this);
            _inputService.LocalAwake();
            _lobbyService.LocalAwake();
            _uiService.LocalAwake();
        }
        private void Start()
        {
            _inputService.LocalStart();
            _lobbyService.LocalStart();
            _uiService.LocalStart();
        }

        private void Update()
        {
            _inputService.LocalUpdate(Time.deltaTime);
            _lobbyService.LocalUpdate(Time.deltaTime);
            _uiService.LocalUpdate(Time.deltaTime);
        }
       
    }
}
