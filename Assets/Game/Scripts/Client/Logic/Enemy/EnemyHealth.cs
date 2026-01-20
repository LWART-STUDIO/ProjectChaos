using System;
using System.Collections;
using System.Collections.Generic;
using CompassNavigatorPro;
using Game.Scripts.Client.Logic.Colectables;
using Game.Scripts.Client.UI.Game.World;
using Game.Scripts.Services;
using PurrNet;
using SaintsField.Playa;
using Sisus.Init;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Scripts.Client.Logic.Enemy
{
    public class EnemyHealth : NetworkBehaviour
    {
        [SerializeField] private int _expFOrKill = 1;
        [SerializeField] private SyncVar< float> _health = new SyncVar<float>(100);
        [SerializeField] private SyncVar<float> _maxHealth = new SyncVar<float>(100);
        [SerializeField] private float _maxHealthDefault = 100;
        [SerializeField] private float _maxHealthMultiplayer = 1f;
        [SerializeField] private float _maxHealthFlatModifier = 0;
        [SerializeField] private Collider _collider;
        [SerializeField] private GameObject _ui;
        [SerializeField] private CompassProPOI _compassProPOI;
        [SerializeField] private GameObject _renderer;
        [SerializeField] private GameObject _deathEffect;
        [SerializeField] private GameObject _spawnEffect;
        [SerializeField] private AnimationCurve _hpCurve = AnimationCurve.Linear(0, 1, 1, 10);
        [SerializeField] private List<float> _hpByLevel = new List<float>();
        [SerializeField] private ExpOrb _expOrbPrefab;
        private bool _spawned=false;
        public Collider Collider=>_collider;
        

        public float Health => _health.value;
        public float MaxHealth => _maxHealth.value;
        public float MaxHealthDefault => _maxHealthDefault;
        public float MaxHealthMultiplayer => _maxHealthMultiplayer;
        public float MaxHealthFlatModifier => _maxHealthFlatModifier;
        public bool Spawned => _spawned;

        public static Action<EnemyHealth> onEnemyKilled;

        protected override void OnSpawned( bool asServer)
        {
            base.OnSpawned();
            if (!asServer)
                return;
            _maxHealth.value=(_maxHealthDefault+_maxHealthFlatModifier)*_maxHealthMultiplayer;
            _health.value = _maxHealth.value;
            SetNewPoiID();
            StartCoroutine(SpawnEffect());
      
        }
        [ObserversRpc(runLocally:true)]
        private void SetNewPoiID()
        {
            _compassProPOI.id=Random.Range(100,10000000);
           
        }

        protected override void OnPoolReset()
        {
            base.OnPoolReset();
            _maxHealth.value=(_maxHealthDefault+_maxHealthFlatModifier)*_maxHealthMultiplayer;
            _health.value = _maxHealth.value;
        
        }
        [ObserversRpc(runLocally:true)]
        private void ShowDamageClientRpc(float amount, Vector3 hitPosition)
        {
            if (DamagePopupSpawner.Instance == null)
                return;
            DamagePopupSpawner.Instance.Spawn(amount, hitPosition);
        }

        private void SetLayerRecursively(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
                SetLayerRecursively(child.gameObject, layer);
        }

        [ServerRpc(requireOwnership:false)]
        public void ChangeHealth(float amount)
        {
            _health.value = Mathf.Clamp(_health.value + amount, 0, _maxHealth.value);
            ShowDamageClientRpc(amount, transform.position);
            if(_health.value==0)
                Die();
        }
        [ServerRpc(requireOwnership:false)]
        public void ChangeMaxHealthFlat(float amount)
        {
            _maxHealthFlatModifier += amount;
            _maxHealth.value=(_maxHealthDefault+_maxHealthFlatModifier)*_maxHealthMultiplayer;
        }
        [ServerRpc(requireOwnership:false)]
        public void ChangeMaxHealthMultiplayer(float amount)
        {
            _maxHealthMultiplayer +=  amount;
            _maxHealth.value=(_maxHealthDefault+_maxHealthFlatModifier)*_maxHealthMultiplayer;
        }
        [ServerRpc(requireOwnership:false)]
        public void Upgrade(int amount)
        {
            float missingHealth = _maxHealth.value - _health.value;

            ChangeMaxHealthFlat(_hpByLevel[amount]);

            _health.value = _maxHealth.value - missingHealth;
        }
        private void Die()
        {
            onEnemyKilled?.Invoke(this);
            Service<ServiceInitor>.Instance.AudioService.PlaySoundInPlaceObserver("EnemyDeath",transform.position);
            DieFx();
            Destroy(gameObject,2f);
            SpawnExpOrb();
        }
        [ObserversRpc(runLocally:true)]
        private void SpawnExpOrb()
        {
            ExpOrb exp = NetworkManager.Instantiate(_expOrbPrefab, transform.position, Quaternion.identity);
            exp.SetUpExp(_expFOrKill);
        }


        private IEnumerator SpawnEffect()
        {
            
            float time = 0;
            while (time < 1f)
            {
                time += Time.deltaTime;
                UpdatePortalFx(time);
                yield return null;
            }
            SpawnFx();

        }

        [ObserversRpc(runLocally:true)]
        private void UpdatePortalFx(float time)
        {
            _spawnEffect.SetActive(true);
            _spawnEffect.transform.localScale = Vector3.Lerp(Vector3.zero, new Vector3(2, 2, 2), time);
        }
        
        [ObserversRpc(runLocally:true)]
        private void SpawnFx()
        {
            _spawnEffect.SetActive(false);
            _collider.enabled=true;
            _renderer?.SetActive(true);
            _ui.SetActive(true);
            _spawned = true;
        }
        [ObserversRpc(runLocally:true)]
        private void DieFx()
        {
            _collider.enabled=false;
            _renderer?.SetActive(false);
            _ui.SetActive(false);
            _deathEffect?.SetActive(true);
        }
        [Button]
        public void ApplyHpCurve()
        {
            int count = _hpByLevel.Count;
            if (count == 0) return;

            for (int i = 0; i < count; i++)
            {
                float t = i / (float)(count - 1); // 0..1 по всем уровням
                float hp = _hpByLevel[i];
                hp = _hpCurve.Evaluate(t);
                _hpByLevel[i] = hp;
            }
        }
    }
}
