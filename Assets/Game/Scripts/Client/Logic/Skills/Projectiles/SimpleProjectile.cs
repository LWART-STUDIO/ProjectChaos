using System;
using Game.Scripts.Services.Pool;
using PurrNet;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Skills.Projectiles
{
    public class SimpleProjectile : NetworkBehaviour
    {
        [Header("Trajectory")]
        [SerializeField] private bool _useTrajectory;
        [SerializeField] private AnimationCurve _heightCurve;
        [SerializeField] private AnimationCurve _sideCurve;
        [SerializeField] private bool _trajectoryDependsOnDuration = true;
        [SerializeField] private float _trajectoryCycleDuration = 1f;
        [SerializeField] private float _heightAmplitude = 1f;
        [SerializeField] private float _sideAmplitude = 1f;
        [SerializeField] private bool _randomizeTrajectoryStart;

        private float _trajectoryTime;
        private float _trajectoryPhaseOffset;

        [Header("Stats")]
        private float _damage;
        private float _speed;
        private float _duration;
        private float _lifetime;

        [Header("Movement")]
        private Vector3 _direction;
        private float _radius;
        private float _distanceFromGround;

        private float _distanceTraveled;
        private Vector3 _startPositionXZ;

        [Header("State")]
        private int _currentPierceCountLeft;
        private int _currentWallBounceCountLeft;
        private bool _setUp;

        [Header("Layers")]
        private int _groundLayer;
        private int _enemyLayer;
        private int _wallLayer;
        private int _collisionMask;
        private readonly RaycastHit[] _hits = new RaycastHit[1];

        #region Unity

        private void Awake()
        {
            _groundLayer = 1 << 6;
            _enemyLayer  = 1 << 7;
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
        

        private void Update()
        {
            if (!_setUp || !isOwner)
                return;

            _trajectoryTime += Time.deltaTime;
            _lifetime += Time.deltaTime;

            if (_lifetime >= _duration)
                DestroyObject();
        }

        private void FixedUpdate()
        {
            if (!_setUp || !isOwner)
                return;

            float step = _speed * Time.fixedDeltaTime;

            Vector3 prevPos = transform.position;

            _distanceTraveled += step;
            Vector3 nextPos = GetPositionAlongTrajectory(_distanceTraveled);

            Vector3 move = nextPos - prevPos;
            float moveDist = move.magnitude;

            if (moveDist > 0f)
            {
                int hitCount = Physics.SphereCastNonAlloc(
                    prevPos,
                    _radius,
                    move.normalized,
                    _hits,
                    moveDist,
                    _collisionMask,
                    QueryTriggerInteraction.Ignore
                );

                if (hitCount > 0)
                {
                    ProcessHit(_hits[0]);
                    return;
                }
            }

            Move(nextPos);
        }

        #endregion

        #region Initialization

        public void Initialize(
            float damage,
            float speed,
            float distanceFromGround,
            float size,
            int pierceCount,
            int wallBounceCount,
            float duration = 4f)
        {
            if(!isOwner)
                return;
            Vector3 pos = transform.position;
            _startPositionXZ = new Vector3(pos.x, 0f, pos.z);

            _distanceTraveled = 0f;
            _trajectoryTime = 0f;

            _trajectoryPhaseOffset = _randomizeTrajectoryStart
                ? UnityEngine.Random.value
                : 0f;

            _damage = damage;
            _speed = speed;
            _duration = duration;

            _radius = size * 0.5f;
            _distanceFromGround = distanceFromGround;

            _currentPierceCountLeft = pierceCount;
            _currentWallBounceCountLeft = wallBounceCount;

            _direction = transform.forward.normalized;
            transform.localScale = Vector3.one * size;

            _lifetime = 0f;
            _setUp = true;
        }

        #endregion

        #region Trajectory

        private Vector3 GetPositionAlongTrajectory(float distance)
        {
            Vector3 flatDir = new Vector3(_direction.x, 0f, _direction.z).normalized;
            Vector3 flatPos = _startPositionXZ + flatDir * distance;

            float groundY = GetGroundHeight(flatPos);

            float t;
            if (_trajectoryDependsOnDuration)
            {
                t = Mathf.Clamp01((_trajectoryTime / _duration + _trajectoryPhaseOffset) % 1f);
            }
            else
            {
                t = ((_trajectoryTime / _trajectoryCycleDuration) + _trajectoryPhaseOffset) % 1f;
            }

            float heightOffset = _useTrajectory
                ? _heightCurve.Evaluate(t) * _heightAmplitude
                : 0f;

            float sideOffset = _useTrajectory
                ? _sideCurve.Evaluate(t) * _sideAmplitude
                : 0f;

            Vector3 right = Vector3.Cross(Vector3.up, flatDir).normalized;

            return new Vector3(
                flatPos.x + right.x * sideOffset,
                groundY + heightOffset,
                flatPos.z + right.z * sideOffset
            );
        }

        private float GetGroundHeight(Vector3 position)
        {
            if (Physics.Raycast(
                    position + Vector3.up * 10f,
                    Vector3.down,
                    out RaycastHit hit,
                    50f,
                    _groundLayer))
            {
                return hit.point.y + _distanceFromGround + _radius;
            }

            return transform.position.y;
        }

        #endregion

        #region Collision

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
            // Wall / Ground
            else if (((1 << hitLayer) & _wallLayer) != 0 ||
                     ((1 << hitLayer) & _groundLayer) != 0)
            {
                if (_currentWallBounceCountLeft <= 0)
                {
                    DestroyObject();
                    return;
                }

                _currentWallBounceCountLeft--;

                Vector3 hitPos = hit.point + hit.normal * (_radius + 0.01f);

                _startPositionXZ = new Vector3(hitPos.x, 0f, hitPos.z);
                _distanceTraveled = 0f;

                Vector3 reflected = Vector3.Reflect(_direction, hit.normal);
                _direction = new Vector3(reflected.x, 0f, reflected.z).normalized;

                Move(GetPositionAlongTrajectory(0f));
            }
        }

        #endregion

        #region Pool

        private void Move(Vector3 position)
        {
            transform.position = position;
        }

        private void DestroyObject()
        {
                ReturnToPool();
        }
        
        public void ReturnToPool()
        {
            ReturnToPool_Local();
        }
        private void ReturnToPool_Local()
        {
            _setUp = false;
            _lifetime = 0f;
            _currentPierceCountLeft = 0;
            _currentWallBounceCountLeft = 0;
            Destroy(gameObject);
        }

        #endregion
    }
}
