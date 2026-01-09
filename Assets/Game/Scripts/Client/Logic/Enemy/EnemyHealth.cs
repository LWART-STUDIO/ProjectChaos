using System;
using Game.Scripts.Client.Logic.Game;
using PurrNet;
using UnityEngine;

public class EnemyHealth : NetworkBehaviour
{
    [SerializeField] private int _expFOrKill = 1;
    [SerializeField] private float _health = 100;
    [SerializeField] private float _maxHealth = 100;
    [SerializeField] private float _maxHealthDefault = 100;
    [SerializeField] private float _maxHealthMultiplayer = 1f;
    [SerializeField] private float _maxHealthFlatModifier = 0;
        

    public float Health => _health;
    public float MaxHealth => _maxHealth;
    public float MaxHealthDefault => _maxHealthDefault;
    public float MaxHealthMultiplayer => _maxHealthMultiplayer;
    public float MaxHealthFlatModifier => _maxHealthFlatModifier;

    public static Action<EnemyHealth> onEnemyKilled;

    protected override void OnSpawned()
    {
        base.OnSpawned();
        _maxHealth=(_maxHealthDefault+_maxHealthFlatModifier)*_maxHealthMultiplayer;
        _health = _maxHealth;
      
    }

    protected override void OnPoolReset()
    {
        base.OnPoolReset();
        _maxHealth=(_maxHealthDefault+_maxHealthFlatModifier)*_maxHealthMultiplayer;
        _health = _maxHealth;
        
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
        _health = Mathf.Clamp(_health + amount, 0, _maxHealth);
        if(_health==0)
            Die();
    }
    [ServerRpc(requireOwnership:false)]
    public void ChangeMaxHealthFlat(int amount)
    {
        _maxHealthFlatModifier += amount;
        _maxHealth=(_maxHealthDefault+_maxHealthFlatModifier)*_maxHealthMultiplayer;
    }
    [ServerRpc(requireOwnership:false)]
    public void ChangeMaxHealthMultiplayer(float amount)
    {
        _maxHealthMultiplayer +=  amount;
        _maxHealth=(_maxHealthDefault+_maxHealthFlatModifier)*_maxHealthMultiplayer;
    }

    private void Die()
    {
        onEnemyKilled?.Invoke(this);
        if(InstanceHandler.TryGetInstance(out LevelManager levelManager))
            levelManager.AddExp(_expFOrKill);
        Destroy(gameObject);
    }
}
