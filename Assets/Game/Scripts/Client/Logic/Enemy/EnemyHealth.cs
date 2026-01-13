using System;
using CompassNavigatorPro;
using Game.Scripts.Client.Logic.Game;
using Game.Scripts.Client.UI.Game.World;
using PurrNet;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyHealth : NetworkBehaviour
{
    [SerializeField] private int _expFOrKill = 1;
    [SerializeField] private float _health = 100;
    [SerializeField] private float _maxHealth = 100;
    [SerializeField] private float _maxHealthDefault = 100;
    [SerializeField] private float _maxHealthMultiplayer = 1f;
    [SerializeField] private float _maxHealthFlatModifier = 0;
    [SerializeField] private CompassProPOI _compassProPOI;
        

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
        _compassProPOI.id=Random.Range(100,10000000);
      
    }

    protected override void OnPoolReset()
    {
        base.OnPoolReset();
        _maxHealth=(_maxHealthDefault+_maxHealthFlatModifier)*_maxHealthMultiplayer;
        _health = _maxHealth;
        
    }
    [Client]
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
        _health = Mathf.Clamp(_health + amount, 0, _maxHealth);
        ShowDamageClientRpc(amount, transform.position);
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
    [ServerRpc(requireOwnership:false)]
    public void Upgrade(float amount)
    {
        _maxHealth+=amount;
        ChangeHealth(amount);
    }

    private void Die()
    {
        onEnemyKilled?.Invoke(this);
        if(InstanceHandler.TryGetInstance(out LevelManager levelManager))
            levelManager.AddExp(_expFOrKill);
        Destroy(gameObject);
    }
}
