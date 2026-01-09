using System.Collections.Generic;
using Game.Scripts.Client.Logic.Player.Stats;
using Game.Scripts.Services.Pool;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Skills.Projectiles.Spark
{
    public class SparkSkill:Skill
    {
        [SerializeField] private bool _shootBackwards;
        [SerializeField] private bool _shootLeft;
        [SerializeField] private bool _shootRight;
        
        [SerializeField] private SimpleProjectile _projectile;
        private SparkData.LevelData _levelData;
        private float _lastCastTime;
        private ObjectPool<SimpleProjectile> _projectilePool;
        private float _damage => _levelData.damage * stats.GetStatValue(StatBonus.Damage);
        protected override void OnInitialize()
        {
            var sparkData = (SparkData)data;
            _levelData = sparkData.GetLevelData(level);
            _projectilePool = new ObjectPool<SimpleProjectile>(_projectile.gameObject,10);
        }

        public override void Tick()
        {
            if(_lastCastTime+_levelData.cooldown>Time.time)
                return;
            _lastCastTime = Time.time;
            List<Vector3> directions = new List<Vector3> { transform.forward };
            if (_shootBackwards) directions.Add(-transform.forward);
            if (_shootLeft) directions.Add(-transform.right);
            if (_shootRight) directions.Add(transform.right);
            foreach (var baseDir in directions)
            {
                if (_levelData.projectileCount == 1)
                {
                    SpawnProjectile(baseDir);
                }
                else
                {
                    float totalAngle = _levelData.angleSpread;
                    float angleStep = (_levelData.projectileCount > 1) ? totalAngle / (_levelData.projectileCount - 1) : 0f;
                    float startAngle = -totalAngle / 2f;
                    for (int i = 0; i < _levelData.projectileCount; i++)
                    {
                        float currentAngle = startAngle + angleStep * i;
                        // Поворачиваем базовое направление вокруг оси "вверх" локального ShootPoint
                        Quaternion rotation = Quaternion.AngleAxis(currentAngle, transform.up);
                        Vector3 finalDir = rotation * baseDir;
                        SpawnProjectile(finalDir);
                    }
                }
              
            }
            

        }
        private void SpawnProjectile(Vector3 direction)
        {
            direction.Normalize();
            var projectile = _projectilePool.Pull(shootPoint.position, Quaternion.LookRotation(direction));
            projectile.Initialize(_damage, _levelData.speed, _levelData.distanceFromGround,
                _levelData.size, _levelData.pierceCount, _levelData.wallBounceCount, _levelData.duration);
        }
    }
}