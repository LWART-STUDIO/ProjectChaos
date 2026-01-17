using System;
using System.Collections.Generic;
using Game.Scripts.Client.Logic.Enemy;
using Game.Scripts.Services.Pool;
using PurrNet;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Skills.Area
{
    public class SimpleAreaSkill : NetworkBehaviour
    {
        private float _damage;
        private float _duration;
        private float _lifetime;
        private float _speed;
        private bool _setUp;
        private float _distanceFromGround;
        private float _lastAttackTime = 0;
        private readonly HashSet<EnemyHealth> _enemiesInArea = new();
        private readonly List<EnemyHealth> _damageBuffer = new();
        

        public void Initialize(float damage,
            float distanceFromGround,
            float size,
            float speed,
            float duration = 4f)
        {
            if(!isOwner)
                return;
            _damage = damage;
            _duration = duration;
            _lifetime = 0f;
            _distanceFromGround = distanceFromGround;
            transform.localScale = Vector3.one * size;
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.x+_distanceFromGround, transform.localPosition.z);
            _speed = speed;
            _lastAttackTime = 0;
            _setUp = true;
        }
        private void Update()
        {
            if (!_setUp || !isOwner)
                return;

            _lifetime += Time.deltaTime;
            _lastAttackTime += Time.deltaTime;

            if (_lastAttackTime >= _speed)
            {
                DealDamage();
                _lastAttackTime = 0f;
            }

            if (_lifetime >= _duration)
                DestroyObject();
        }
        private void OnEnable()
        {
            EnemyHealth.onEnemyKilled += OnEnemyKilled;
        }

        private void OnDisable()
        {
            EnemyHealth.onEnemyKilled -= OnEnemyKilled;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out EnemyHealth enemy))
            {
                _enemiesInArea.Add(enemy);
            }
        }
        private void OnEnemyKilled(EnemyHealth enemy)
        {
            _enemiesInArea.Remove(enemy);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out EnemyHealth enemy))
                _enemiesInArea.Remove(enemy);
        }
        private void DealDamage()
        {
            _damageBuffer.Clear();

            foreach (var enemy in _enemiesInArea)
            {
                if (enemy != null && enemy.isSpawned)
                    _damageBuffer.Add(enemy);
            }

            foreach (var enemy in _damageBuffer)
                enemy.ChangeHealth(-_damage);
        }
        

        private void DestroyObject()
        {
                ReturnToPool();
        }

        private void ReturnToPool_Local()
        {
            _setUp = false;
            _lifetime = 0f;
            _enemiesInArea.Clear();
            _lastAttackTime = 0;
            Destroy(gameObject);
        }
        public void ReturnToPool()
        {
            ReturnToPool_Local();
        }
    }
}
