using System;
using Game.Prefabs.Skills.Active;
using SFAbilitySystem.Demo.Abilities;
using SFAbilitySystem.Demo.Interfaces;
using SFAbilitySystem.Demo.Logic;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Scripts.Client.Logic.Skills.Active
{
    public class SparkLogic : ActiveLogicBase<FireballAbility>,IAbilityInject
    {
                
        [SerializeField] private Projecrile _sparkPrefab;
        private Transform _spawnPoint;

        protected override void ExecuteAbility()
        {
            if (_sparkPrefab == null || _spawnPoint == null)
            {
                Debug.LogError("Spark prefab or spawn point not set!");
                return;
            }
            SpawnObjectRpc();
        }
        [Rpc(SendTo.Server)]
        private void SpawnObjectRpc()
        {
            Projecrile spark = Instantiate(
                _sparkPrefab,
                _spawnPoint.position,
                _spawnPoint.rotation
            );
            spark.GetComponent<NetworkObject>().Spawn(true);
        }

        public Type GetDependencyType()
        {
            return typeof(SkillsControl);
        }

        public void Inject(Object instance)
        {
            _spawnPoint = ((SkillsControl)instance).transform;
        }
    }
}
