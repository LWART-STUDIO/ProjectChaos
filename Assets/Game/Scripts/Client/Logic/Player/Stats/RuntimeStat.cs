using UnityEngine;

namespace Game.Scripts.Client.Logic.Player.Stats
{
    [System.Serializable]
    public class RuntimeStat
    {
        public StatBonus bonus;

        [Tooltip("Для мультипликативных статов обычно 1")]
        public float baseValue = 1f;

        public float flat;          // +5
        public float incPercent;    // +20%
        public float morePercent;   // +50% (мультипликативный)

        public float FinalValue
        {
            get
            {
                float value = baseValue;

                value += flat;
                value *= 1f + incPercent / 100f;
                value *= 1f + morePercent / 100f;

                return Mathf.Max(0, value);
            }
        }

        public void Reset()
        {
            flat = 0;
            incPercent = 0;
            morePercent = 0;
        }
    }
}