using UnityEngine;

namespace Game.Scripts.Client.Logic.Enemy
{
    public class EnemyUpgrader : MonoBehaviour
    {
        [SerializeField] private EnemyHealth _enemyHealth;
        [SerializeField] private EnemyAttackMelee _enemyAttackMelee;
        private bool _upgraded;

        public void UpgradeEnemy(int value)
        {
            if (_upgraded)
                return;

            _upgraded = true;
            _enemyHealth.Upgrade(value);
            _enemyAttackMelee.Upgrade(value);
        }
    }
}
