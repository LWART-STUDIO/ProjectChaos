using System.Collections;
using System.Collections.Generic;
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
        public void Setup(float lifetime, float speed, int damage,
            float distanceFromGround,Vector3 direction)
        {
            _lifetime = lifetime;
            _speed = speed;
            _damage = damage;
            _distanceFromGround = distanceFromGround;
            _direction = direction;
            StartCoroutine(DisableAfterLife());
            _setUp = true;
        }

        private void Update()
        {
            if(!_setUp)
                return;
            int groundLayer = 1 << 6;
            transform.position+=_direction*_speed*Time.deltaTime;
            RaycastHit hit;
            Vector3 rayOrigin = transform.position;
            Vector3 rayDirection = Vector3.down;
            if (Physics.Raycast(rayOrigin, rayDirection, out hit, Mathf.Infinity, groundLayer))
            {
                // Точка, где луч коснулся земли
                Vector3 targetPosition = hit.point + Vector3.up * _distanceFromGround;
            
                // Плавно перемещаем (или мгновенно устанавливаем)
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * _speed);

            }
        }

        private IEnumerator DisableAfterLife()
        {
            yield return new WaitForSeconds(_lifetime);
            gameObject.SetActive(false);
        }
    }
}
