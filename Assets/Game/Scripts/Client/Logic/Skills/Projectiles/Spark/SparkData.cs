using System.Collections.Generic;
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
            public int damage;
            public float cooldown;
            public float speed;
            public float size;
            public int pierceCount;
            public int wallBounceCount;
            public float distanceFromGround;
            public float duration;
            public float angleSpread;
        }

        [SerializeField] private List<LevelData> _levels =  new List<LevelData>();
        public LevelData GetLevelData(int level)=>_levels[Mathf.Clamp(level,0,_levels.Count-1)];
    }
}