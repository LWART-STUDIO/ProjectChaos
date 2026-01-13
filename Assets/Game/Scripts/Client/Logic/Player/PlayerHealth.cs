using System;
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
        [SerializeField] private SyncVar<int> _maxHealth = new(100);
        [SerializeField] private SyncVar<int> _maxHealthDefault = new(100);
        [SerializeField] private SyncVar<float> _maxHealthMultiplayer = new(1f);
        [SerializeField] private SyncVar<int> _maxHealthFlatModifier = new(0);
        

        public int Health => _health;
        public int MaxHealth => _maxHealth;
        public int MaxHealthDefault => _maxHealthDefault;
        public float MaxHealthMultiplayer => _maxHealthMultiplayer;
        public int MaxHealthFlatModifier => _maxHealthFlatModifier;

        public static Action<PlayerID> onPlayerDie;
        public Action<int> onHealthChanged;
        public Action onPlayerDieLocal;

        private void Awake()
        {
            
            
        }
        [ServerRpc(requireOwnership:false)]
        public void RestoreFullHealth()
        {
   
            _maxHealth.value=(int)((_maxHealthDefault.value+_maxHealthFlatModifier.value)*_maxHealthMultiplayer.value);
            _health.value = _maxHealth;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _health.onChanged -= OnHealthChanged;
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
            if (isOwner)
            {
                Service<UIService>.Instance.GetPlayerInGameUI().UpdateHealth(_health.value);
                Service<UIService>.Instance.GetPlayerInGameUI().SetPlayerCompas(transform.GetComponentInChildren<Camera>());
                
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
            if(isOwner)
                Service<UIService>.Instance.GetPlayerInGameUI().UpdateHealth(newValue);
            if(newValue<=0 )
                Die();
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
            _health.value = Mathf.Clamp(_health.value + amount, 0, _maxHealth);
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

        protected override void OnDespawned()
        {
            base.OnDespawned();
            if (owner.HasValue)
                _allPlayers.Remove(owner.Value);
        }
    }
}
