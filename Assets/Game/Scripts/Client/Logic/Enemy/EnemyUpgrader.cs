using SaintsField.Playa;
using UnityEditor;
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
#if UNITY_EDITOR
        

        [Button]
        public void ApplyDamageCurve()
        {
            _enemyAttackMelee.ApplyDamageCurve();
            EditorUtility.SetDirty(gameObject);

        }

      

        [Button]
        public void ApplyHpCurve()
        {
            _enemyHealth.ApplyHpCurve();
            EditorUtility.SetDirty(gameObject);
            
        }
#endif
    }
}
