using Game.Scripts.Client.Logic.Player.Stats;
using PurrNet;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Skills
{
    public abstract class Skill:NetworkBehaviour
    {
        public SkillData data {get; private set;}
        public PlayerStatsHolder stats {get; private set;}
        public int level {get; private set;}
        public Transform shootPoint;
        
        public void Initialize(Transform shootPoint,SkillData data, int level,PlayerStatsHolder stats)
        {
            this.data = data;
            this.level = level;
            this.shootPoint = shootPoint;
            this.stats = stats;
            OnInitialize();
        }

        protected abstract void OnInitialize();
        public abstract void Tick();

    }
}