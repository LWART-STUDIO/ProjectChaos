using System.Linq;
using Game.Scripts.Client.Logic;
using Game.Scripts.Client.Logic.Game;
using Game.Scripts.Services.Audio;
using Game.Scripts.Services.Input;
using Game.Scripts.Services.ResourceLoader;
using Game.Scripts.Services.Scene;
using Game.Scripts.Services.StaticService;
using Game.Scripts.Services.UI;
using Michsky.UI.Reach;
using SaintsField.Playa;
using Sisus.Init;
using SoftKitty.WSFL;
using UnityEngine;

namespace Game.Scripts.Services
{
    [Service(typeof(ServiceInitor),FindFromScene = true,LazyInit = true)]
    public class ServiceInitor : MonoBehaviour<UIService,InputService>
    {

        private LevelManager _levelManager;
        private UIService _uiService;
        private ResourceLoaderService _resourceLoaderService;
        private SceneService _sceneService;
        private InputService _inputService;

        protected override void Init(
            UIService uiService, InputService inputService)
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
            if(_uiService == null)
                _uiService = Service<UIService>.Instance;
            _uiService.LocalAwake();
            if(_sceneService == null)
                _sceneService = Service<SceneService>.Instance;
            if(_inputService == null)
                _inputService = Service<InputService>.Instance;
            _inputService.LocalAwake();

            _sceneService?.LocalAwake();

        }

        public void UpdateLocalization(int _)
        {
            switch (LocalizationManager.instance.currentLanguage)
            {
                case "Russian (ru-RU)":
                    Localization.SelectedLanguage=0;
                    break;
                case "English (en-US)":
                    Localization.SelectedLanguage=1;
                    break;
            }
            Debug.Log(LocalizationManager.instance.currentLanguage);
            
        }
        private void Start()
        {
            _uiService.LocalStart();

            if(_inputService != null)
                _inputService.LocalStart();

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
            _uiService.LocalUpdate(Time.deltaTime);
            if (_inputService != null)
            {
                _inputService.LocalUpdate(Time.deltaTime);
                _inputService.LocalUnscaledUpdate(Time.unscaledDeltaTime);
            }
                
        }
        

       
    }
}
