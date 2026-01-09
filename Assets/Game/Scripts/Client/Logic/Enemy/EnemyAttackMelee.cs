using Game.Scripts.Client.Logic.Player;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Enemy
{
    public class EnemyAttackMelee : MonoBehaviour
    {
        [SerializeField] private int _damage=10;

        private float _lastAttackTime;

        private void OnCollisionStay(Collision other)
        {
            if(_lastAttackTime+1>Time.time)
                return;
            if(!other.transform.TryGetComponent(out PlayerHealth playerHealth) || !playerHealth.isOwner)
                return;
            _lastAttackTime = Time.time;
            playerHealth.ChangeHealth(-_damage);
        }
    }
}
