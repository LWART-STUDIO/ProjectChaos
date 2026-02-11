using System;
using System.Collections;
using System.Collections.Generic;
using Game.Scripts.Client.Logic.Game;
using Game.Scripts.Services.ResourceLoader;
using Game.Scripts.Services.StaticService;
using PurrNet;
using Sisus.Init;
using Systems.Services.SceneService;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Scripts.Services.Scene
{
    [Service(typeof(SceneService),FindFromScene =  true,LazyInit =  true)]
    public class SceneService : MonoBehaviour
    {
        private LoadingScreen _loadingScreen;
        private LoadingScreen _loadingScreenInstance;
        private List<AsyncOperation> _loadingOperations = new List<AsyncOperation>();
        private float _loadingProgress = 0f;
        private bool _sceneLoaded;
        public bool SceneLoaded=>_sceneLoaded;
        public event Action<SceneMapper> OnSceneLoaded;
        [PurrScene,SerializeField] private List<string> _sceneNamesToLoad;
        private bool _isLoadingScene;
        public void LocalAwake()
        {
            DontDestroyOnLoad(gameObject);
            if (_loadingScreenInstance == null)
            {
                GameObject loadingScreenPrefab = Service<ResourceLoaderService>.
                    Instance.Load<GameObject>(StaticPath.LoadingScreenPath);
                if (loadingScreenPrefab != null) 
                     _loadingScreenInstance = Instantiate(loadingScreenPrefab,transform).GetComponent<LoadingScreen>();
                if (_loadingOperations.Count == 0)
                    _sceneLoaded = true;
            }
        }

        public SceneMapper GetActiveScene()
        {
            return (SceneMapper)SceneManager.GetActiveScene().buildIndex;
        }
        public int GetActiveSceneIndex()
        {
            return SceneManager.GetActiveScene().buildIndex;
        }


        public void LocalUpdate(float deltaTime)
        {
            //For switchSceneByKeyBoard
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha0))
                LoadScene(SceneMapper.LobbySample);
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))
                LoadScene(SceneMapper.Game);


        }
        public void LoadScene(SceneMapper scene)
        {
            LoadScene((int)scene);
        }
        public void ReloadScene(SceneMapper scene)
        {
            ReloadScene((int)scene);
        }
        public void ReloadScene()
        {
            ReloadScene(GetActiveSceneIndex());
        }
   
        private IEnumerator GetSceneLoadProgress(int sceneIndexInBuildSettings)
        {
            _sceneLoaded = false;
            for (int i = 0; i < _loadingOperations.Count; i++)
            {
                while (_loadingOperations[i]!=null && !_loadingOperations[i].isDone)
                {
                    _loadingProgress = 0;
                    _loadingScreenInstance.SetCurrentProgress(_loadingProgress);
                    foreach (AsyncOperation operation in _loadingOperations)
                    {
                        if(operation==null)
                            continue;
                        _loadingProgress+= operation.progress;
                    }
                    _loadingProgress = (_loadingProgress/_loadingOperations.Count)*100f;
                    _loadingScreenInstance.SetCurrentProgress(_loadingProgress);
                    yield return null;
                }
            }
            bool waitUntilFullLoad = true;
            GameStateWarmUp gameStateWarmUp;
            if(SceneMapper.Game ==(SceneMapper)sceneIndexInBuildSettings)
                while (waitUntilFullLoad)
                {
                    gameStateWarmUp = FindAnyObjectByType<GameStateWarmUp>();
                    if (gameStateWarmUp == null)
                    {
                        yield return null;
                        continue;
                    }
                    if (!gameStateWarmUp.WarmUpComplete)
                    {
                        yield return null;
                        continue;
                    }
                    break;
                }
                
            _sceneLoaded = true;
            _loadingScreenInstance.gameObject.SetActive(false);
            OnSceneLoaded?.Invoke(GetActiveScene());
        }

        public async Awaitable IsSceneLoaded()
        {
            while (!_sceneLoaded)
                await Awaitable.NextFrameAsync();
        }
        public void LoadScene(string sceneName)
        {
            _sceneLoaded = false;
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("Scene name is empty");
                return;
            }
   
            Enum.TryParse(sceneName, true, out SceneMapper scene);
            LoadScene(scene);
        }
   
        public void ReloadScene(string sceneName)
        {
            _sceneLoaded = false;
            Enum.TryParse(sceneName, true, out SceneMapper scene);
            ReloadScene(scene);
        }
   
        public void ReloadScene(int sceneIndexInBuildSettings)
        {
            _sceneLoaded = false;
            _loadingOperations = new List<AsyncOperation>();
           // DOTween.KillAll();
            _loadingScreenInstance.gameObject.SetActive(true);
            _loadingOperations.Add(SceneManager.LoadSceneAsync(sceneIndexInBuildSettings,LoadSceneMode.Single));
              
            StartCoroutine(GetSceneLoadProgress(sceneIndexInBuildSettings));
        }
   
        public void LoadScene(int sceneIndexInBuildSettings)
        {
            if (_isLoadingScene)
            {
                return;
            }

            _isLoadingScene = true;
            _sceneLoaded = false;

            _loadingOperations.Clear();
            StopAllAudio();

            _loadingScreenInstance.gameObject.SetActive(true);

            var asyncOp = SceneManager.LoadSceneAsync(
                _sceneNamesToLoad[sceneIndexInBuildSettings],
                LoadSceneMode.Single
            );

            asyncOp.allowSceneActivation = false;
            _loadingOperations.Add(asyncOp);

            StartCoroutine(TrackLoading(sceneIndexInBuildSettings, asyncOp));
        }

        private IEnumerator TrackLoading(int sceneIndex, AsyncOperation op)
        {
            // 1. Ждём загрузку в память
            while (op.progress < 0.9f)
            {
                float progress = Mathf.Clamp01(op.progress / 0.9f) * 100f;
                _loadingScreenInstance.SetCurrentProgress(progress);
                yield return null;
            }

            // 2. Разрешаем активацию сцены
            op.allowSceneActivation = true;

            // 3. Ждём, пока сцена станет активной
            while (!op.isDone)
                yield return null;

            // 4. Теперь объекты существуют
            if ((SceneMapper)sceneIndex == SceneMapper.Game)
                yield return WaitForGameWarmup();

            _loadingScreenInstance.gameObject.SetActive(false);
            _sceneLoaded = true;
            _isLoadingScene = false;

            OnSceneLoaded?.Invoke(GetActiveScene());
        }


        private IEnumerator WaitForGameWarmup()
        {
            GameStateWarmUp warmUp = null;

            // ждём появления объекта
            while (warmUp == null)
            {
                warmUp = FindAnyObjectByType<GameStateWarmUp>();
                yield return null;
            }

            // ждём окончания прогрева
            while (!warmUp.WarmUpComplete)
                yield return null;
        }

        private void StopAllAudio()
        {
            //No implemated
        }
            
    }
}
