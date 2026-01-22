using Game.Scripts.Client.Logic.Player.Stats;
using Game.Scripts.Services.Pool;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Skills.Area
{
    public class FireAreaSkill : Skill
    {
        private FireAreaSkillData.LevelData _levelData;
        private float _damage => _levelData.damage * stats.GetStatValue(StatBonus.Damage);
        private float _duration => (_levelData.duration * stats.GetStatValue(StatBonus.SkillDuration));
        private float _speed => (_levelData.speed * stats.GetStatValue(StatBonus.SkillSpeed));
        [SerializeField] private SimpleAreaSkill _curentSkills;
        private float _lastCastTime;
        public override int GetDamage()
        {
            // 1. Урон за тик
            float damagePerTick = _damage;

            // 2. Частота тиков (эвристика)
            float tickRate = 1f; // 1 раз в секунду
            // если знаешь точнее — подставь

            // 3. Сколько тиков за жизнь зоны
            float totalTicks = _duration * tickRate;

            // 4. Ожидаемое число целей в зоне
            float expectedTargets = ExpectedTargetsInArea();
            float uptime = Mathf.Min(1f, _duration / _levelData.cooldown);
            // 5. Урон за один каст
            float damagePerCast =
                damagePerTick
                * totalTicks
                * expectedTargets;
            damagePerCast *= uptime;

            // 6. DPS
            return Mathf.RoundToInt(damagePerCast / _levelData.cooldown);
        }
        private float ExpectedTargetsInArea()
        {
            float areaRadius = _levelData.size * 0.5f;
            float area = Mathf.PI * areaRadius * areaRadius;

            // эмпирическое значение плотности врагов
            float enemyDensity = 0.05f;

            return Mathf.Clamp(area * enemyDensity, 1f, 6f);
        }

        protected override void OnInitialize()
        {
            var sparkData = data as FireAreaSkillData;
            _levelData = sparkData.GetLevelData(level);

        }

        public override void Tick()
        {
            if(_lastCastTime+_levelData.cooldown>Time.time)
                return;
            _lastCastTime = Time.time;
            var projectile = Instantiate(_curentSkills,transform.position,_curentSkills.transform.rotation,transform);
            projectile.Initialize(_damage, _levelData.distanceFromGround,
                _levelData.size, _speed,_levelData.onGround,_duration);
        }
    }
}
