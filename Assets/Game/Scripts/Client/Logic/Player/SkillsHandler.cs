using System.Collections.Generic;
using Game.Scripts.Client.Logic.Player.Stats;
using Game.Scripts.Client.Logic.Skills;
using Game.Scripts.Services.UI;
using PurrNet;
using Sisus.Init;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Player
{
    public class SkillsHandler : NetworkBehaviour,ITick
    {
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private PlayerStatsHolder _playerStatsHolder;
        private Dictionary<string,Skill> _activeSkills = new Dictionary<string,Skill>();
        [SerializeField] private List<SkillData> _initialSkills = new List<SkillData>();

        protected override void OnSpawned()
        {
            base.OnSpawned();
            if(!isOwner)
                return;
            InstanceHandler.RegisterInstance(this);
            foreach (SkillData skillData in _initialSkills)
            {
                AddSkill(skillData);
            }
            _playerStatsHolder.ResetStats();

            var skillTree = Service<UIService>.Instance.GetPlayerSkillTree();
            skillTree.SetUp(_playerStatsHolder);
            skillTree.ResetTree();
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();
            if(!isOwner)
                return;
            InstanceHandler.UnregisterInstance<SkillsHandler>();
        }
        public void AddSkill(SkillData skillData)
        {
            if (_activeSkills.TryGetValue(skillData.skillId, out var skill))
            {
                int newLevel = skill.level + 1;
                skill.Initialize(_shootPoint,skillData,newLevel,_playerStatsHolder);
                return;
            }
            var attack = Instantiate(skillData.prefab,_shootPoint.position,Quaternion.identity,_shootPoint);
            attack.Initialize(_shootPoint,skillData,0,_playerStatsHolder);
            _activeSkills[skillData.skillId] = attack;
        }

        public int GetSkillLevel(string id)
        {
            return _activeSkills.TryGetValue(id, out var skill) ? skill.level : -1;
        }


        public void OnTick(float delta)
        {
            if(!isOwner)
                return;
            if(!enabled)
                return;
            foreach (var skill in _activeSkills) 
                skill.Value.Tick();
        }
    }

    
}