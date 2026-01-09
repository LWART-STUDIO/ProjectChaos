using System;
using Game.Scripts.Services.Pool;
using PurrNet;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Skills.Projectiles
{
    public class SimpleProjectile : NetworkBehaviour,IPoolable<SimpleProjectile>
    {
        private float _damage;
        private float _speed;
        private float _duration;
        private float _lifetime;

        private Vector3 _direction;
        private float _radius;
        private float _distanceFromGround;

        private int _currentPierceCountLeft;
        private int _currentWallBounceCountLeft;

        private bool _setUp;

        private int _groundLayer;
        private int _enemyLayer;
        private int _wallLayer;
        private int _collisionMask;
        
        private Action<SimpleProjectile> _returnToPool;

        private RaycastHit[] _hits = new RaycastHit[1];

        private void Awake()
        {
            _groundLayer = 1 << 6;
            _enemyLayer = 1 << 7;
            _wallLayer   = 1 << 8;

            _collisionMask = _groundLayer | _enemyLayer | _wallLayer;
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();
            if (!isOwner)
                return;
            _direction = transform.forward.normalized;
            _setUp = true;
        }

        protected override void OnPoolReset()
        {
            base.OnPoolReset();
            _setUp = false;
        }

        public void Initialize(
            float damage,
            float speed,
            float distanceFromGround,
            float size,
            int pierceCount,
            int wallBounceCount,
            float duration = 4f)
        {
            _damage = damage;
            _speed = speed;
            _duration = duration;

            _radius = size * 0.5f;
            _distanceFromGround = distanceFromGround;

            _currentPierceCountLeft = pierceCount;
            _currentWallBounceCountLeft = wallBounceCount;

            _direction = transform.forward.normalized;

            transform.localScale = Vector3.one * size;

            _lifetime = 0;
            _setUp = true;
        }

        private void FixedUpdate()
        {
            if (!_setUp || !isOwner)
                return;

            float step = _speed * Time.fixedDeltaTime;
            Vector3 start = transform.position;

            int hitCount = Physics.SphereCastNonAlloc(
                start,
                _radius,
                _direction,
                _hits,
                step,
                _collisionMask,
                QueryTriggerInteraction.Ignore
            );

            if (hitCount > 0)
            {
                ProcessHit(_hits[0]);
                return;
            }

            Move(start + _direction * step);
        }

        private void ProcessHit(RaycastHit hit)
        {
            int hitLayer = hit.collider.gameObject.layer;

            // Enemy
            if (((1 << hitLayer) & _enemyLayer) != 0)
            {
                if (hit.collider.TryGetComponent(out EnemyHealth enemy) && enemy.isSpawned)
                {
                    enemy.ChangeHealth(-_damage);
                }

                if (_currentPierceCountLeft-- <= 0)
                {
                    DestroyObject();
                    return;
                }
            }
            // Wall
            else if (((1 << hitLayer) & _wallLayer) != 0||((1 << hitLayer) & _groundLayer)!=0)
            {
                if (_currentWallBounceCountLeft <= 0)
                {
                    DestroyObject();
                    return;
                }

                _currentWallBounceCountLeft--;
                _direction = Vector3.Reflect(_direction, hit.normal).normalized;
            }

            Vector3 newPos = hit.point + hit.normal * (_radius + 0.01f);
            Move(newPos);
        }

        private void Move(Vector3 position)
        {
            // Коррекция высоты
            if (Physics.Raycast(position, Vector3.down, out RaycastHit groundHit, 10f, _groundLayer))
            {
                position.y = groundHit.point.y + _distanceFromGround + _radius;
            }

            transform.position = position;
        }

        private void Update()
        {
            if (!isOwner||!_setUp)
                return;

            _lifetime += Time.deltaTime;
            if (_lifetime >= _duration)
                DestroyObject();
        }


        private void DestroyObject()
        {
            // заменить на Despawn, если используешь сетевой пул
            ReturnToPool();

        }

        public void Initialize(Action<SimpleProjectile> returnAction)
        {
            _returnToPool = returnAction;
        }
        /*private void OnDisable()
        {
            ReturnToPool();
        }*/

        public void ReturnToPool()
        {
            _setUp = false;
            _lifetime = 0f;
            _currentPierceCountLeft = 0;
            _currentWallBounceCountLeft = 0;

            _returnToPool?.Invoke(this);
        }
    }
}
