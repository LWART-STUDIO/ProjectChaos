using System;
using Game.Scripts.Client.Logic.Player;
using PurrNet;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class EnemyMovment : NetworkBehaviour,ITick
{
    [SerializeField] private float _speed;
    private Rigidbody _rigidbody;
    private PlayerHealth _targetPlayer;
    private float _lastPlayerCheck;

    private void Awake()
    {
        TryGetComponent(out _rigidbody);
    }
    protected override void OnSpawned()
    {
        base.OnSpawned();
        enabled =isServer;
    }

    protected override void OnPoolReset()
    {
        if(!isServer)
            return;
        _targetPlayer = null;
        _lastPlayerCheck = 0;

    }

    private void FixedUpdate()
    {
        if(!isServer)
            return;
        if(!_targetPlayer)
            return;
        var targetPosition = _targetPlayer.transform.position;
        var direction = (targetPosition - transform.position).normalized;
        direction.y = 0;
        _rigidbody.linearVelocity = direction * _speed *Time.fixedDeltaTime;
        var lookRotation = Quaternion.LookRotation(direction);
        _rigidbody.MoveRotation(Quaternion.Slerp(_rigidbody.rotation, lookRotation, Time.fixedDeltaTime*10f));
    }

    public void OnTick(float delta)
    {
        if(!isServer)
            return;
        if (Time.time < _lastPlayerCheck + 1f)
            return;
        _lastPlayerCheck = Time.time;
        var allPlayers = PlayerHealth.AllPlayers;
        if(allPlayers.Count <= 0)
            return;
        PlayerID closestPlayer = default;
        float closestDistance = float.MaxValue;
        foreach (var player in allPlayers)
        {
            if(player.Value.Health<=0)
                continue;
            var playerPos = player.Value.transform.position;
            var distance = Vector3.Distance(transform.position, playerPos);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player.Key;
            }
        }
        if(closestPlayer == default)
            return;
        PlayerHealth.AllPlayers.TryGetValue(closestPlayer, out _targetPlayer);
    }
}
