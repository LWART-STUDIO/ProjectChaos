using System;
using Game.Prefabs.Skills.Active;
using Game.Scripts.Services.Pool;
using SFAbilitySystem.Demo.Abilities;
using SFAbilitySystem.Demo.Interfaces;
using SFAbilitySystem.Demo.Logic;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Scripts.Client.Logic.Skills.Active
{
    public class SparkLogic : ActiveLogicBase<SparkSkill>,IAbilityInject
    {
        [SerializeField] private GameObject _prefab;
        private SkillsControl _skillsControl;

        protected override void ExecuteAbility()
        {
            if ( _skillsControl == null)
            {
                Debug.LogError("Spark prefab or spawn point not set!");
                return;
            }
            SpawnObjectRpc();
        }
        [Rpc(SendTo.Server)]
        private void SpawnObjectRpc()
        {
            ObjectPool<PoolObject> pool= _skillsControl.GetOrCreatePool(_activeAbilityBase.Name,_prefab);
            Projectile spark = pool.PullGameObject(_skillsControl.ShootPoint.position).GetComponent<Projectile>();
            spark.Setup(_activeAbilityBase.lifetime,_activeAbilityBase.speed,
                _activeAbilityBase.damage,_activeAbilityBase.distanceFromGround,_skillsControl.ShootPoint.forward);
          //  spark.GetComponent<NetworkObject>().Spawn(true);
        }

        public Type GetDependencyType()
        {
            return typeof(SkillsControl);
        }

        public void Inject(Object instance)
        {
            _skillsControl = instance as SkillsControl;
        }
    }
}
