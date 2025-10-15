using Game.Scripts.Services.Lobby;
using Game.Scripts.Services.UI;
using Sisus.Init;
using UnityEngine;

namespace Game.Scripts.Services
{
    [Service(typeof(ServiceInitor),FindFromScene = true,LazyInit = true)]
    public class ServiceInitor : MonoBehaviour<SteamService,LocalLobbyService,UIService>
    {
        [SerializeField] private bool _useSteam;
        private ILobbyService _lobbyService;
        private UIService _uiService;
        public ILobbyService LobbyService=>_lobbyService;
        protected override void Init(SteamService steamService,LocalLobbyService localLobby,UIService uiService)
        {
            if(_useSteam)
                _lobbyService = steamService;
            else
                _lobbyService = localLobby;
            _uiService = uiService;
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            if (Service<ServiceInitor>.Instance == null)
                Service.SetInstance(this);
            
            _lobbyService.LocalAwake();
            _uiService.LocalAwake();
        }
        private void Start()
        {
            _lobbyService.LocalStart();
            _uiService.LocalStart();
        }

        private void Update()
        {
            _lobbyService.LocalUpdate(Time.deltaTime);
            _uiService.LocalUpdate(Time.deltaTime);
        }
       
    }
}
