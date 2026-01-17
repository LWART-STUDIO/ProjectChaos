using System.Collections.Generic;
using Game.Scripts.Client.Logic.Player;
using SaintsField.Playa;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Enemy
{
    public class EnemyAttackMelee : MonoBehaviour
    {
        [SerializeField] private float _damage=10;
        [SerializeField] private EnemyHealth _enemyHealth;
        [SerializeField] private AnimationCurve _damageCurve = AnimationCurve.Linear(0, 1, 1, 10);
        [SerializeField] private List<float> _damageByLevel = new List<float>();

        private float _lastAttackTime;

        private void OnCollisionStay(Collision other)
        {
            if(!_enemyHealth.Spawned)
                return;
            if(_enemyHealth.Health<=0)
                return;
            if(_lastAttackTime+1>Time.time)
                return;
            if(!other.transform.TryGetComponent(out PlayerHealth playerHealth) || !playerHealth.isOwner)
                return;
            _lastAttackTime = Time.time;
            playerHealth.ChangeHealth(-(int)_damage);
        }

        public void Upgrade(int value)
        {
            _damage =_damageByLevel[value];
        }
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
        }
        [Button]
        public void ApplyHpCurve()
        {
            _enemyHealth.ApplyHpCurve();
            
        }
    }
}
