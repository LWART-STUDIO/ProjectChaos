using System.Collections.Generic;
using SaintsField.Playa;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Skills.Passives
{
    [CreateAssetMenu(fileName = "MoveSpeedPassiveData", menuName = "Skills/SkillData/Passives/MoveSpeedPassiveData", order = 0)]
    public class MoveSpeedPassiveData:SkillData
    {
        [System.Serializable]
        public struct LevelData
        {
          public float speedIncrease;
          
        }
        [SerializeField] private AnimationCurve _speedCurve = AnimationCurve.Linear(0, 1, 1, 10);
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
                ld.speedIncrease = _speedCurve.Evaluate(t);
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
                levelDescriptions.Add($"{_levels[i].speedIncrease}% увеличения скорости передвижения");
            }
        }
    }
}