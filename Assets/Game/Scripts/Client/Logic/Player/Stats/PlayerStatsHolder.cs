using System.Collections.Generic;
using PurrNet;
using SaintsField;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Player.Stats
{
    public class PlayerStatsHolder : NetworkBehaviour
    {

        [FieldLabelText("$" + nameof(StatsLabels))]
        [SerializeField] private List<RuntimeStat> _stats;
        private string StatsLabels(RuntimeStat _, int index) => $"<color=gray>[{_.bonus.ToString()}]";
        private Dictionary<StatBonus, RuntimeStat> _statMap;

        private void Awake()
        {
            _statMap = new Dictionary<StatBonus, RuntimeStat>();
            foreach (var stat in _stats)
            {
                _statMap.Add(stat.bonus, stat);
            }
        }

        public void ResetStats()
        {
            foreach (var stat in _stats)
            {
                stat.Reset();
            }
        }

        public void ApplyStat(SimpleStat stat)
        {
            if (!_statMap.TryGetValue(stat.statBonus, out var runtimeStat))
                return;

            switch (stat.statType)
            {
                case StatType.Flat:
                    runtimeStat.flat += stat.value;
                    break;

                case StatType.IncPercent:
                    runtimeStat.incPercent += stat.value;
                    break;

                case StatType.MultPercent:
                    runtimeStat.morePercent += stat.value;
                    break;

                case StatType.Custom:
                    ApplyCustom(stat);
                    break;
            }
        }

        private void ApplyCustom(SimpleStat stat)
        {
            // например:
            // crit chance scaling, chaining, проки и т.д.
        }

        public float GetStatValue(StatBonus bonus)
        {
            return _statMap.TryGetValue(bonus, out var stat)
                ? stat.FinalValue
                : 1f;
        }
    }
}