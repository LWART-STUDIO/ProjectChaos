using UnityEngine;

namespace Game.Scripts.Client.Logic.Player.Stats
{
    [CreateAssetMenu(fileName = "SimpleStat", menuName = "Skills/Tree/SimpleStat", order = 0)]
    public class SimpleStat : ScriptableObject
    {
        public StatBonus statBonus;
        public StatType statType;
        public float value;
        public string description;
        public Sprite icon;
        
    }

    public enum StatType
    {
        Flat = 0,
        IncPercent=1,
        MultPercent=3,
        Custom=4
    }

    public enum StatBonus
    {
        Damage = 0,
        Pirce = 1,
        Projectile = 2,
        SkillSpeed = 3,
        SkillDuration = 4,
        WallBounce = 5,
       
    }
}