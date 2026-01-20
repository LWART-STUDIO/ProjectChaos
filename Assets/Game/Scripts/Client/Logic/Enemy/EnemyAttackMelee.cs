using System;
using System.Collections;
using System.Collections.Generic;
using Game.Scripts.Client.Logic.Player;
using ProjectDawn.Navigation.Hybrid;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Enemy
{
    public class EnemyAttackMelee : MonoBehaviour
    {
        [SerializeField] private float _damage=10;
        [SerializeField] private EnemyHealth _enemyHealth;
        [SerializeField] private AnimationCurve _damageCurve = AnimationCurve.Linear(0, 1, 1, 10);
        [SerializeField] private List<float> _damageByLevel = new List<float>();
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private AgentAuthoring _agent;
        private float _attackDelay = 2f;
        [SerializeField] private CapsuleCollider _collider;

        private float _lastAttackTime;
        private readonly RaycastHit[] _hits = new RaycastHit[1];
        

        private void Update()
        {
            if(!_enemyHealth.Spawned)
                return;
            if(_enemyHealth.Health<=0)
                return;
            if(_lastAttackTime+_attackDelay>Time.time)
                return;
            int hitCount = Physics.SphereCastNonAlloc(
                transform.position,
                _collider.radius,
                transform.forward,
                _hits,
                _collider.radius,
                1<<10,
                QueryTriggerInteraction.Ignore
            );
            if (hitCount > 0)
            {
                ProcessHit(_hits[0]);
                return;
            }
          
        }

        private void ProcessHit(RaycastHit hit)
        {
            if (!hit.collider.TryGetComponent(out PlayerHealth playerHealth))
                return;
            _lastAttackTime = Time.time;
            playerHealth.ChangeHealth(-(int)_damage);
            StartCoroutine(AttackState(playerHealth.transform));
        }

        private IEnumerator AttackState(Transform player)
        {
            _agent.enabled = false;
            _agent.EntityBody.Stop();
            _rigidbody.isKinematic = false;
            _rigidbody.AddForce((-transform.forward*5f+transform.up*5f), ForceMode.Impulse);
            yield return new WaitForSeconds(_attackDelay/2f);
            _agent.enabled = true;
            _rigidbody.isKinematic = true;


        }
        public void Upgrade(int value)
        {
            _damage =_damageByLevel[value];
        }
#if UNITY_EDITOR
        

        [Button]
        public void ApplyDamageCurve()
        {
            int count = _damageByLevel.Count;
            if (count == 0) return;

            for (int i = 0; i < count; i++)
            {
                float t = i / (float)(count - 1); // 0..1 по всем уровням
                float damage = _damageByLevel[i];
                damage = _damageCurve.Evaluate(t);
                _damageByLevel[i] = damage;
            }
            EditorUtility.SetDirty(gameObject);
        }

        [Button]
        public void ApplyHpCurve()
        {
            _enemyHealth.ApplyHpCurve();
            EditorUtility.SetDirty(gameObject);
            
        }
#endif
        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}
