using System.Linq;
using Game.Scripts.Client.Logic;
using Game.Scripts.Client.Logic.Game;
using Game.Scripts.Services.Audio;
using Game.Scripts.Services.Input;
using Game.Scripts.Services.ResourceLoader;
using Game.Scripts.Services.Scene;
using Game.Scripts.Services.StaticService;
using Game.Scripts.Services.UI;
using SaintsField.Playa;
using Sisus.Init;

using UnityEngine;

namespace Game.Scripts.Services
{
    [Service(typeof(ServiceInitor),FindFromScene = true,LazyInit = true)]
    public class ServiceInitor : MonoBehaviour<UIService,InputService>
    {
        [SerializeField] private AudioService _audioService;
        private LevelManager _levelManager;
        private UIService _uiService;
        private InputService _inputService;
        private ResourceLoaderService _resourceLoaderService;
        private SceneService _sceneService;
        public AudioService AudioService => _audioService;
        protected override void Init(
            UIService uiService,
            InputService inputService)
        {
            _uiService = uiService;
            _inputService = inputService;
            _resourceLoaderService = Service<ResourceLoaderService>.Instance;
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            if (Service<ServiceInitor>.Instance == null)
                Service.SetInstance(this);
            if(_inputService == null)
                _inputService = Service<InputService>.Instance;
            _inputService.LocalAwake();
            if(_uiService == null)
                _uiService = Service<UIService>.Instance;
            _uiService.LocalAwake();
            if(_sceneService == null)
                _sceneService = Service<SceneService>.Instance;
            if(_audioService != null)
                _audioService.LocalAwake();
            _sceneService.LocalAwake();

        }
        private void Start()
        {
            _inputService.LocalStart();
            _uiService.LocalStart();
            if(_audioService != null)
                _audioService.LocalStart();

        }

        [Button]
        public void UpdateExp()
        {
            if(_levelManager == null)
                _levelManager = FindAnyObjectByType<LevelManager>();
            if(_levelManager == null)
                return;
            _levelManager.ApplyExpCurve();
        }

        private void Update()
        {
            _sceneService.LocalUpdate(Time.deltaTime);
            _inputService.LocalUpdate(Time.deltaTime);
            _uiService.LocalUpdate(Time.deltaTime);
            if(_audioService != null)
                _audioService.LocalUpdate(Time.deltaTime);
        }

       
    }
}
