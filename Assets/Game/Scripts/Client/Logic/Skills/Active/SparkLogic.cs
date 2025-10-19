using System;
using System.Collections.Generic;
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
    public class SparkLogic : ActiveLogicBase<SparkSkill>, IAbilityInject
    {
        [SerializeField] private GameObject _prefab;
        [SerializeField] private ProjectileVfxManager _projectileVfxManager;
        private SkillsControl _skillsControl;


        protected override void ExecuteAbility()
        {
            if (_skillsControl == null)
            {
                Debug.LogError("Spark prefab or spawn point not set!");
                return;
            }

            SpawnObjectRpc();
        }

        [Rpc(SendTo.Server)]
        private void SpawnObjectRpc()
        {
            // Базовые данные
            int projectileCount = _activeAbilityBase.count;
            float angleSpread = _activeAbilityBase.angle;
            Vector3 shootOrigin = _skillsControl.ShootPoint.position;

            // Список направлений в зависимости от флагов
            List<Vector3> directions = new List<Vector3> { _skillsControl.ShootPoint.forward };

            if (_activeAbilityBase.shootBackwards)
                directions.Add(-_skillsControl.ShootPoint.forward);
            if (_activeAbilityBase.shootLeft)
                directions.Add(-_skillsControl.ShootPoint.right);
            if (_activeAbilityBase.shootRight)
                directions.Add(_skillsControl.ShootPoint.right);

            foreach (var baseDir in directions)
            {
                // Если всего один снаряд — просто стреляем прямо по baseDir
                if (projectileCount == 1)
                {
                    SpawnProjectile(shootOrigin, baseDir);
                }
                else
                {
                    // Распределяем снаряды по углу angleSpread
                    float totalAngle = angleSpread;
                    float angleStep = (projectileCount > 1) ? totalAngle / (projectileCount - 1) : 0f;
                    float startAngle = -totalAngle / 2f;

                    for (int i = 0; i < projectileCount; i++)
                    {
                        float currentAngle = startAngle + angleStep * i;

                        // Поворачиваем базовое направление вокруг оси "вверх" локального ShootPoint
                        Quaternion rotation = Quaternion.AngleAxis(currentAngle, _skillsControl.ShootPoint.up);
                        Vector3 finalDir = rotation * baseDir;

                        SpawnProjectile(shootOrigin, finalDir);
                    }
                }
            }
        }

        private void SpawnProjectile(Vector3 position, Vector3 direction)
        {
            ObjectPool<PoolObject> pool = _skillsControl.GetOrCreatePool(_activeAbilityBase.Name, _prefab);
            Projectile spark = pool.PullGameObject(position).GetComponent<Projectile>();

            spark.Setup(
                _activeAbilityBase.lifetime,
                _activeAbilityBase.speed,
                _activeAbilityBase.damage,
                _activeAbilityBase.distanceFromGround,
                direction,
                _activeAbilityBase.size,
                _activeAbilityBase.pierceCount,
                _activeAbilityBase.wallBounceCount
            );

            var netObj = spark.GetComponent<NetworkObject>();
            if (!netObj.IsSpawned)
                netObj.Spawn(true);
            if (_projectileVfxManager != null)
                _projectileVfxManager.TrackProjectile(spark);
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