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
