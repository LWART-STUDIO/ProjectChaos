using System;
using System.Collections;
using System.Collections.Generic;
using AmazingAssets.DynamicRadialMasks;
using AtmosphericHeightFog;
using Game.Scripts.Client.Logic.Location;
using Game.Scripts.Client.Logic.Player;
using PurrNet;
using PurrNet.StateMachine;
using UnityEngine;
using LocationInfo = Game.Scripts.Client.Logic.Location.LocationInfo;

namespace Game.Scripts.Client.Logic.Game
{
    public class GameStateSpawning : StateNode<Dictionary<PlayerID, PlayerClassType>>
    {
        [SerializeField] private float _changeTime = 5f;
        [SerializeField] private float _startSpawnTime = 5f;
        [SerializeField] private float _maxRadius=10000f;
        [SerializeField] private DRMGameObject _radiusControl;
        [SerializeField] private DRMController _distanceController;
        [SerializeField] private DRMGameObjectsPool _distancePool;
        [SerializeField] private HeightFogGlobal _fog;
        [SerializeField] private Light _light;
        [SerializeField] private LocationSpawner _locationSpawner;
        [SerializeField] private List<Transform> _spawnPoints;
        [SerializeField] private List<PlayerHealth> _players = new List<PlayerHealth>();
        [SerializeField] private List<PlayerClassConfig> _classConfigs;
        [SerializeField] private EnemySpawner _enemySpawner;
        [SerializeField] private Camera _spectateCamera;
        private Dictionary<PlayerClassType, PlayerHealth> _prefabs;
        private Dictionary<PlayerID, PlayerClassType> _playerClases;
        private bool _spawned = false;
        

     
        private void Awake()
        {
            _prefabs = new Dictionary<PlayerClassType, PlayerHealth>();
            foreach (var config in _classConfigs)
                _prefabs[config.classType] = config.prefab;

        }

        public override void Enter(Dictionary<PlayerID, PlayerClassType> dictionary, bool asServer)
        {
            base.Enter(asServer);
          //  RenderSettings.skybox.SetFloat("_Exposure",0.63f);
            _light.intensity = 0.8f;
            RenderSettings.skybox.SetColor("_Tint",Color.white);
            RenderSettings.ambientSkyColor = Color.white;
            RenderSettings.ambientEquatorColor = Color.white;
            _fog.fogIntensity = 0f;
            _fog.skyboxFogIntensity = 0f;
            _playerClases = dictionary;
            _distancePool.UpdateController();
            _radiusControl.radius = 0f;
            _distanceController.UpdateShaderData();
            InstanceHandler.NetworkManager.Subscribe<LocationInfo>(OnLocationIDReceived);
            Cursor.visible = false;
            if (asServer)
            {
                _spawned = false;
                DespawnPlayers();
                SpawnPlayers();
                SpawnLocation();

            }
        
        }
        
        private void DespawnPlayers()
        {
            var allPlayers = FindObjectsByType<PlayerHealth>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var player in allPlayers)
                Destroy(player.gameObject);
        }

        private void SpawnPlayers()
        {
            _players.Clear();
            int spawnIndex = 0;

            foreach (var player in networkManager.players)
            {
                if (!_playerClases.TryGetValue(player, out var classType))
                {
                    Debug.LogError($"[SPAWN] Missing class for player {player}");
                    continue;
                }

                if (!_prefabs.TryGetValue(classType, out var prefab))
                {
                    Debug.LogError($"[SPAWN] No prefab for class {classType}");
                    continue;
                }

                var spawnPoint = _spawnPoints[spawnIndex];
                Debug.Log($"[SPAWN] Игрок {player} выбрал класс {classType}");
                var newPlayer = Instantiate(
                    prefab,
                    spawnPoint.position,
                    spawnPoint.rotation
                );
                newPlayer.GiveOwnership(player);
                newPlayer.GetComponent<PlayerController>().SetStartPosition(spawnPoint.position);
                _players.Add(newPlayer);

                spawnIndex = (spawnIndex + 1) % _spawnPoints.Count;
            }

            foreach (var player in _players)
            {
                player.GetComponent<PlayerController>().MoveToStartPosition();
            }
        }

        private void OnLocationIDReceived(PlayerID player,LocationInfo locationInfo,bool asServer)
        {
            StartCoroutine(ChangeLocation());
        }

        private IEnumerator ChangeLocation()
        {
            _locationSpawner.SpawnLocation();
            _spectateCamera.gameObject.SetActive(false);
            float startRadius = _radiusControl.radius;
            float t = 0f;
            float spawntT = 0f;
            float spawnTimer = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / _changeTime;
                spawnTimer += Time.deltaTime;
                spawntT+= Time.deltaTime/_startSpawnTime;
               // RenderSettings.skybox.SetFloat("_Exposure",Mathf.Lerp(0.63f, 0.1f, spawntT));
               // _light.intensity = Mathf.Lerp(0.8f, 0.3f, spawntT);
              //  RenderSettings.skybox.SetColor("_Tint",Color.Lerp(Color.white, Color.red, Mathf.Clamp01(spawntT)));
               // RenderSettings.ambientSkyColor = Color.Lerp(Color.white, Color.red, Mathf.Clamp01(spawntT));
                RenderSettings.ambientEquatorColor = Color.Lerp(Color.white, Color.gray, Mathf.Clamp01(spawntT));
                _fog.fogIntensity = Mathf.Lerp(0f, 1f, spawntT);
                _fog.skyboxFogIntensity = Mathf.Lerp(0f, 1f, spawntT);
                _distancePool.UpdateController();
                _distanceController.UpdateShaderData();
                _radiusControl.radius = Mathf.Lerp(startRadius, _maxRadius, t);
                if (spawnTimer >= _startSpawnTime)
                {
                    if(_spawned)
                        continue;
                    _spawned = true;
                    _locationSpawner.DisableStartLocation();
                    if(!isServer)
                        continue;
                    _enemySpawner.StartSpawning();
                    machine.Next();
  
                }
                yield return null;
            }
            _radiusControl.radius = _maxRadius;
            _locationSpawner.DisableStartLocation();
            if(!isServer)
                yield break;
            if(_spawned)
                yield break;
            _spawned = true;
            _enemySpawner.StartSpawning();
            machine.Next();
        }

        private void SpawnLocation()
        {
            int locationId = 42;
            InstanceHandler.NetworkManager.SendToAll(new LocationInfo(locationId));
        }
        


        public override void Exit(bool asServer)
        {
            base.Exit(asServer);
        }
    }
}
