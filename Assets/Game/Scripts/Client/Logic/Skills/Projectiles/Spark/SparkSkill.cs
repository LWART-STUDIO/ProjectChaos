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
        private float _damage => _levelData.damage * stats.GetStatValue(StatBonus.Damage);
        private int _pirce => (int)(_levelData.pierceCount + stats.GetStatValue(StatBonus.Pirce));
        private int _wallBounce => (int)(_levelData.wallBounceCount + stats.GetStatValue(StatBonus.WallBounce));
        private float _duration => (_levelData.duration * stats.GetStatValue(StatBonus.SkillDuration));
        private float _speed => (_levelData.speed * stats.GetStatValue(StatBonus.SkillSpeed));
        private int _projectileCount => (int)(_levelData.projectileCount + stats.GetStatValue(StatBonus.Projectile));
        public override int GetDamage()
        {
            float hitsPerProjectile =
                1f                                     
                + ExpectedPierceHits()
                + ExpectedBounceHits();

            float totalHits =
                _projectileCount
                * hitsPerProjectile
                * HitChanceBySpread();

            float damagePerCast = totalHits * _damage;

            return Mathf.RoundToInt(damagePerCast / _levelData.cooldown);
        }
        float ExpectedPierceHits()
        {
            return Mathf.Min(_pirce, 3) * 0.4f;
        }
        float ExpectedBounceHits()
        {
            return Mathf.Min(_wallBounce, 3) * 0.3f;
        }
        float HitChanceBySpread()
        {
            if (_projectileCount == 1)
                return 1f;

            float spreadPenalty = Mathf.Clamp01(
                1f - _levelData.angleSpread / 90f
            );

            return spreadPenalty;
        }

        protected override void OnInitialize()
        {
            var sparkData = data as SparkData;
            _levelData = sparkData.GetLevelData(level);
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
                if (_projectileCount == 1)
                {
                    SpawnProjectile(baseDir);
                }
                else
                {
                    float totalAngle = _levelData.angleSpread;
                    float angleStep = (_projectileCount > 1) ? totalAngle / (_projectileCount- 1) : 0f;
                    float startAngle = -totalAngle / 2f;
                    for (int i = 0; i < _projectileCount; i++)
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
            var projectile = Instantiate(_projectile,shootPoint.position, Quaternion.LookRotation(direction));
            projectile.Initialize(_damage, _speed, _levelData.distanceFromGround,
                _levelData.size, _pirce, _wallBounce, _duration);
        }
    }
}