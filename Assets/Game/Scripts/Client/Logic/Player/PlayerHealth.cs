using System;
using System.Collections;
using System.Collections.Generic;
using Game.Scripts.Services.UI;
using PurrNet;
using Sisus.Init;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Scripts.Client.Logic.Player
{
    public class PlayerHealth : NetworkBehaviour
    {
        private static readonly Dictionary<PlayerID, PlayerHealth> _allPlayers = new Dictionary<PlayerID, PlayerHealth>();
        public static Dictionary<PlayerID, PlayerHealth> AllPlayers => _allPlayers;
        [SerializeField] private int _selfLayer, _otherLayer;
        [SerializeField] private SyncVar<int> _health = new(100);
        [SerializeField] private SyncVar<float> _healthRegenerationRate = new(5f);
        [SerializeField] private SyncVar<int> _healthRegenerationCount = new(1);
        [SerializeField] private SyncVar<int> _maxHealth = new(100);
        [SerializeField] private SyncVar<int> _maxHealthDefault = new(100);
        [SerializeField] private SyncVar<float> _maxHealthMultiplayer = new(1f);
        [SerializeField] private SyncVar<int> _maxHealthFlatModifier = new(0);
        [SerializeField] private SkinnedMeshRenderer _renderer;
        
        
        [SerializeField] private SyncVar<int> _energyShield = new(100);
        [SerializeField] private SyncVar<float> _energyShieldRegenerationRate = new(5f);
        [SerializeField] private SyncVar<int> _energyShieldRegenerationCount = new(1);
        [SerializeField] private SyncVar<float> _energyShieldRechargeDelay = new(3);
        [SerializeField] private SyncVar<int> _maxEnergyShield = new(100);
        [SerializeField] private SyncVar<int> _maxEnergyShieldDefault = new(100);
        [SerializeField] private SyncVar<float> _maxEnergyShieldMultiplayer = new(1f);
        [SerializeField] private SyncVar<int> _maxEnergyShieldFlatModifier = new(0);
        
        private SyncVar<float> _currentRechargeBlock =new SyncVar<float>(5f);
        
        public int Health => _health;
        public int EnergyShield => _energyShield;
        public float HealthRegenerationRate => _healthRegenerationRate;
        public int HealthRegenerationCount => _healthRegenerationCount;
        public float EnergyShieldRegenerationRate => _energyShieldRegenerationRate;
        public int EnergyShieldRegenerationCount => _energyShieldRegenerationCount;
        public float EnergyShieldRechargeDelay => _energyShieldRechargeDelay;
        
        public int MaxEnergyShield => _maxEnergyShield;
        public int MaxEnergyShieldDefault => _maxEnergyShieldDefault;
        public float MaxEnergyShieldMultiplayer => _maxEnergyShieldMultiplayer;
        public int MaxEnergyShieldFlatModifier => _maxEnergyShieldFlatModifier;
        public int MaxHealth => _maxHealth;
        public int MaxHealthDefault => _maxHealthDefault;
        public float MaxHealthMultiplayer => _maxHealthMultiplayer;
        public int MaxHealthFlatModifier => _maxHealthFlatModifier;

        public static Action<PlayerID> onPlayerDie;
        public Action<int> onHealthChanged;
        public Action onPlayerDieLocal;
        
        
        [ServerRpc(requireOwnership:false)]
        public void RestoreFullHealth()
        {
   
            _maxHealth.value=(int)((_maxHealthDefault.value+_maxHealthFlatModifier.value)*_maxHealthMultiplayer.value);
            _maxEnergyShield.value=(int)((_maxEnergyShieldDefault.value+_maxEnergyShieldFlatModifier.value)*_maxEnergyShieldMultiplayer.value);
            _health.value = _maxHealth;
            _energyShield.value =_maxEnergyShield;
            UpdateVisual();

        }

        [ObserversRpc(runLocally:true)]
        private void UpdateVisual()
        {
            _renderer.material.SetColor("_OutlineColorVertex", _energyShield.value > 0 ? Color.cyan : Color.black);
        }

        private void OnDisable()
        {
            _health.onChanged -= OnHealthChanged;
            _energyShield.onChanged -= OnHealthChanged;
            StopAllCoroutines();
        }


        protected override void OnSpawned()
        {
            base.OnSpawned();
            int actualLayer = isOwner? _selfLayer : _otherLayer;
            SetLayerRecursively(gameObject, actualLayer);
            RestoreFullHealth();
            if (owner.HasValue)
                _allPlayers[owner.Value] = this;
            _health.onChanged += OnHealthChanged;
            _energyShield.onChanged += OnHealthChanged;
            if (isOwner)
            {
                Service<UIService>.Instance.GetPlayerInGameUI().UpdateHealth(_health.value,_maxHealth.value);
                Service<UIService>.Instance.GetPlayerInGameUI().UpdateEnergyShield(_energyShield.value,_maxEnergyShield.value);
                Service<UIService>.Instance.GetPlayerInGameUI().SetPlayerCompas(transform);
                StartRegen();
            }
        }
        [RuntimeInitializeOnLoadMethod]
        private static void Clear()
        {
            _allPlayers.Clear();
        }

        private void OnHealthChanged( int newValue)
        {
            onHealthChanged?.Invoke(newValue);
            if (isOwner)
            {
                _renderer.material.SetColor("_OutlineColorVertex", _energyShield.value > 0 ? Color.cyan : Color.black);
                
                Service<UIService>.Instance.GetPlayerInGameUI().UpdateEnergyShield(_energyShield.value,_maxEnergyShield.value);
                Service<UIService>.Instance.GetPlayerInGameUI().UpdateHealth(_health.value,_maxHealth.value);
            }

            if(_health.value<=0 )
                Die();
        }
        [ServerRpc(requireOwnership:false)]
        private void StartRegen()
        {
            StartCoroutine(RegenerateHealth());
            StartCoroutine(RegenerateEnergyShield());
        }

        private void Die()
        {
            if(owner.HasValue)
                onPlayerDie?.Invoke(owner.Value);
            if(!isOwner)
                return;
            onPlayerDieLocal?.Invoke();
            
      
        }

        private void SetLayerRecursively(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
                SetLayerRecursively(child.gameObject, layer);
        }

        [ServerRpc(requireOwnership:false)]
        public void ChangeHealth(int amount)
        {
           
            if (_energyShield.value > 0 && amount < 0)
            {
                
                int shieldDamage = Mathf.RoundToInt(-amount * 1.5f);

                if (shieldDamage >= _energyShield.value)
                {
                    int overflow = shieldDamage - _energyShield.value;
                    _energyShield.value = 0;

                    int healthDamage = Mathf.RoundToInt(overflow * 0.75f);
                    _health.value = Mathf.Clamp(_health.value - healthDamage, 0, _maxHealth.value);
                }
                else
                {
                    _energyShield.value -= shieldDamage;
                }
                _currentRechargeBlock.value = _energyShieldRechargeDelay.value;
                return;
            }
            _health.value = Mathf.Clamp(_health.value + amount, 0, _maxHealth.value);
        }
        [ServerRpc(requireOwnership:false)]
        public void AddES(int amount)
        {
            _energyShield.value = Mathf.Clamp(_energyShield.value + amount, 0, _maxEnergyShield.value);
        }
        [ServerRpc(requireOwnership:false)]
        public void ChangeMaxHealthFlat(int amount)
        {
            _maxHealthFlatModifier.value += amount;
            _maxHealth.value=(int)((_maxHealthDefault.value+_maxHealthFlatModifier.value)*_maxHealthMultiplayer.value);
        }
        [ServerRpc(requireOwnership:false)]
        public void ChangeMaxHealthMultiplayer(float amount)
        {
            _maxHealthMultiplayer.value +=  amount;
            _maxHealth.value=(int)((_maxHealthDefault.value+_maxHealthFlatModifier.value)*_maxHealthMultiplayer.value);
        }
        [ServerRpc(requireOwnership:false)]
        public void ChangeMaxEnergyShiedFlat(int amount)
        {
            _maxEnergyShieldFlatModifier.value += amount;
            _maxEnergyShield.value=(int)((_maxEnergyShieldDefault.value+_maxEnergyShieldFlatModifier.value)*_maxEnergyShieldMultiplayer.value);
        }
        [ServerRpc(requireOwnership:false)]
        public void ChangeMaxEnergyShiedMultiplayer(float amount)
        {
            _maxEnergyShieldMultiplayer.value +=  amount;
            _maxEnergyShield.value=(int)((_maxEnergyShieldDefault.value+_maxEnergyShieldFlatModifier.value)*_maxEnergyShieldMultiplayer.value);
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();
            if (owner.HasValue)
                _allPlayers.Remove(owner.Value);
        }

        private IEnumerator RegenerateHealth()
        {
            while (true)
            {
                if (_health.value <= 0)
                {
                    yield return null;
                    continue;
                }

                yield return new WaitForSeconds(_healthRegenerationRate.value);
                ChangeHealth(_healthRegenerationCount.value);
            }
        }
        private IEnumerator RegenerateEnergyShield()
        {
            while (true)
            {
                _currentRechargeBlock.value-=Time.deltaTime;
                if (_currentRechargeBlock.value > 0)
                {
                    yield return null;
                    continue;
                }

                yield return new WaitForSeconds(_energyShieldRegenerationRate.value);
                AddES(_energyShieldRegenerationCount.value);
            }
        }
    }
}
