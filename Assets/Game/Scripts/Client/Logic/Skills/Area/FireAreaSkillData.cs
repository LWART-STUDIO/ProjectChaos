using System.Collections.Generic;
using SaintsField.Playa;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Skills.Area
{
    [CreateAssetMenu(fileName = "FireAreaData", menuName = "Skills/SkillData/FireArea", order = 0)]
    public class FireAreaSkillData : SkillData
    {
        [System.Serializable]
        public struct LevelData
        {
            public float damage;
            public float cooldown;
            public float size;
            public float speed;
            public float duration;
            public float distanceFromGround;
        }
        [SerializeField] private List<LevelData> _levels =  new List<LevelData>();
        [SerializeField] private AnimationCurve _damageCurve = AnimationCurve.Linear(0, 1, 1, 10);
        public LevelData GetLevelData(int level)=>_levels[Mathf.Clamp(level,0,_levels.Count-1)];
        [Button]
        public void ApplyDamageCurve()
        {
            int count = _levels.Count;
            if (count == 0) return;

            for (int i = 0; i < count; i++)
            {
                float t = i / (float)(count - 1); // 0..1 по всем уровням
                LevelData ld = _levels[i];
                ld.damage = _damageCurve.Evaluate(t);
                _levels[i] = ld;
            }
        }
        [Button]
        public void SetDescription()
        {
            int count = _levels.Count;
            if (count == 0) return;
            levelDescriptions.Clear();
            for (int i = 0; i < count; i++)
            {
                levelDescriptions.Add($"Уровень {i+1}");
            }
        }
    }
}
