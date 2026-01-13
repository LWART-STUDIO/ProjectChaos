using UnityEngine;

namespace Game.Scripts.Client.Logic.Enemy
{
    public class EnemyUpgrader : MonoBehaviour
    {
        [SerializeField] private EnemyHealth _enemyHealth;
        [SerializeField] private EnemyAttackMelee _enemyAttackMelee;

        public void UpgradeEnemy(int value)
        {
            _enemyHealth.Upgrade(value*2f);
            _enemyAttackMelee.Upgrade(value * 2f);
        }
    }
}
