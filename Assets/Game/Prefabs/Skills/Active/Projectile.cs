using System.Collections;
using UnityEngine;

namespace Game.Prefabs.Skills.Active
{
    public class Projectile : MonoBehaviour
    {
        private bool _setUp = false;
        private float _lifetime;
        private float _speed;
        private int _damage;
        private float _distanceFromGround;
        private Vector3 _direction;
        private float _size;
        private int _pierceCount;
        private int _wallBounceCount;

        private int _currentPierceCountLeft;
        private int _currentWallBounceCountLeft;

        private Coroutine _disableCoroutine;

        private int _groundLayer;
        private int _enemyLayer;
        private int _wallLayer;
        
        public float Size=>_size;
        public float LifeTime=>_lifetime;

        private void Awake()
        {
            _groundLayer = 1 << 6;
            _enemyLayer = 1 << 7;
            _wallLayer = 1 << 8;
        }

        public void Setup(
            float lifetime,
            float speed,
            int damage,
            float distanceFromGround,
            Vector3 direction,
            float size,
            int pierceCount,
            int wallBounceCount
        )
        {
            _lifetime = lifetime;
            _speed = speed;
            _damage = damage;
            _distanceFromGround = distanceFromGround;
            _direction = direction.normalized;
            _size = size;

            _pierceCount = pierceCount;
            _wallBounceCount = wallBounceCount;

            _currentPierceCountLeft = _pierceCount;
            _currentWallBounceCountLeft = _wallBounceCount;

            transform.localScale = new Vector3(_size, _size, _size);

            if (_disableCoroutine != null)
                StopCoroutine(_disableCoroutine);
            _disableCoroutine = StartCoroutine(DisableAfterLife());

            _setUp = true;
        }

        private void FixedUpdate()
        {
            if (!_setUp)
                return;

            float moveDistance = _speed * Time.fixedDeltaTime;
            Vector3 origin = transform.position;
    
            // SphereCast для проверки коллизий по пути движения
            if (Physics.SphereCast(origin, _size / 2f, _direction, out RaycastHit hit, moveDistance, _enemyLayer | _wallLayer | _groundLayer))
            {
                if (((1 << hit.collider.gameObject.layer) & _enemyLayer) != 0)
                {
                    HandleEnemyHit(hit.collider);
                }
                else
                {
                    HandleWallHit(hit.collider);
                }
            }

            // Перемещаем снаряд на рассчитанное расстояние
            transform.position += _direction * moveDistance;

            // Корректировка по высоте над землёй (как раньше)
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit groundHit, Mathf.Infinity, _groundLayer))
            {
                Vector3 targetPos = groundHit.point + Vector3.up * (_distanceFromGround+_size/2);
                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * _speed);
            }
        }
        
    

        private void HandleEnemyHit(Collider enemy)
        {
            // Здесь можно вызвать метод нанесения урона
            // enemy.GetComponent<Health>().TakeDamage(_damage);

            if (_pierceCount > 0)
            {
                _currentPierceCountLeft--;
                if (_currentPierceCountLeft <= 0)
                {
                    DisableProjectile();
                }
            }
            else
            {
                // Если pierce = 0, уничтожаем сразу
                DisableProjectile();
            }
        }

        private void HandleWallHit(Collider wall)
        {
            if (_currentWallBounceCountLeft > 0)
            {
                _currentWallBounceCountLeft--;

                // Вычисляем нормаль к поверхности
                if (Physics.Raycast(transform.position, _direction, out RaycastHit hit, 2f, _wallLayer | _groundLayer))
                {
                    Vector3 normal = hit.normal;

                    // Отражаем направление
                    _direction = Vector3.Reflect(_direction, normal).normalized;

                    // Переносим позицию немного от поверхности, чтобы не залипать
                    transform.position = hit.point + normal * 0.05f;
                }
            }
            else
            {
                DisableProjectile();
            }
        }

        private IEnumerator DisableAfterLife()
        {
            yield return new WaitForSeconds(_lifetime);
            DisableProjectile();
        }

        private void DisableProjectile()
        {
            _setUp = false;
            if (_disableCoroutine != null)
            {
                StopCoroutine(_disableCoroutine);
                _disableCoroutine = null;
            }
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            // Сброс состояния при возвращении в пул
            _setUp = false;
            _currentPierceCountLeft = 0;
            _currentWallBounceCountLeft = 0;
            _disableCoroutine = null;
        }
    }
}
