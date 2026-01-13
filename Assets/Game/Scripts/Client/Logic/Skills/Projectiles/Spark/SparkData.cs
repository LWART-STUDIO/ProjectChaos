using System.Collections.Generic;
using SaintsField.Playa;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Skills.Projectiles.Spark
{
    [CreateAssetMenu(fileName = "SparkData", menuName = "Skills/SkillData/Spark", order = 0)]
    public class SparkData:SkillData
    {
        [System.Serializable]
        public struct LevelData
        {
            public int projectileCount;
            public float damage;
            public float cooldown;
            public float speed;
            public float size;
            public int pierceCount;
            public int wallBounceCount;
            public float distanceFromGround;
            public float duration;
            public float angleSpread;
        }
        [SerializeField] private AnimationCurve _damageCurve = AnimationCurve.Linear(0, 1, 1, 10);
        [SerializeField] private AnimationCurve _projectileCurve = AnimationCurve.Linear(0, 1, 4, 16);

        [SerializeField] private List<LevelData> _levels =  new List<LevelData>();
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
        public void ApplyProjectileCurve()
        {
            int count = _levels.Count;
            if (count == 0) return;

            for (int i = 0; i < count; i++)
            {
                float t = i / (float)(count - 1); // 0..1 по всем уровням
                LevelData ld = _levels[i];
                ld.projectileCount = (int)_projectileCurve.Evaluate(t);
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