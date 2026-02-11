using Game.Scripts.Client.Logic.Player.Stats;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Skills.Passives
{
    public class MoveSpeedSkill:Skill
    {
        private MoveSpeedPassiveData.LevelData _levelData;
        public override int GetDamage()
        {
            return 0;
        }

        protected override void OnInitialize()
        {
            var moveData = data as MoveSpeedPassiveData;
            _levelData = moveData.GetLevelData(level);
            SimpleStat newStat = ScriptableObject.CreateInstance<SimpleStat>();
            newStat.statBonus = StatBonus.MoveSpeed;
            newStat.statType = StatType.IncPercent;
            newStat.value = _levelData.speedIncrease;
            stats.ApplyStat(newStat);
        }

        public override void Tick()
        {
            
        }
    }
}