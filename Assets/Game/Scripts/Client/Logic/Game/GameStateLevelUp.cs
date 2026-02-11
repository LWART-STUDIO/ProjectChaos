using System;
using System.Collections.Generic;
using Game.Scripts.Client.Logic.Player;
using Game.Scripts.Client.Logic.Skills;
using Game.Scripts.Services.UI;
using PurrNet;
using PurrNet.StateMachine;
using Sisus.Init;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Game
{
    public class GameStateLevelUp : StateNode
    {
        [SerializeField] private StateNode _runningState;
        [SerializeField] private List<SkillData> _allSkill = new List<SkillData>();
        private int _currentLevel = 1;
        
        private List<PlayerID> _readyPlayers = new List<PlayerID>();
        private List<PlayerID> _skillSelected = new List<PlayerID>();

        public override void Enter(bool asServer)
        {
            base.Enter(asServer);
            if(asServer)
                return;
            _readyPlayers.Clear();
            _skillSelected.Clear();
            SetUpLevelOptions();
            UpdateLevel();
            Time.timeScale = 0;
            
        }
        [ObserversRpc(runLocally: true)]
        private void UpdateLevel()
        {
            _currentLevel++;
            Service<UIService>.Instance.GetPlayerInGameUI().UpdateCurrentLevel(_currentLevel); 
        }

        public override void Exit(bool asServer)
        {
            base.Exit(asServer);
            if(asServer)
                return;
            Service<UIService>.Instance.GetLevelUpPanel().CloseWindow();
            Time.timeScale = 1;
            
        }

        public void SetSkillSelected()
        {
            Service<UIService>.Instance.GetLevelUpPanel().CloseWindowImmediately();
            Service<UIService>.Instance.GetPlayerSkillTree().OpenWindow(1,this);
            SkillSelectedRpc();
        }
        public void SetReady()
        {
            Service<UIService>.Instance.GetLevelUpPanel().OpenWaitWindow();
            SetReadyRpc();
        }

        [ServerRpc(requireOwnership: false)]
        private void SetReadyRpc(RPCInfo info = default)
        {
            if(_readyPlayers.Contains(info.sender))
                return;
            _readyPlayers.Add(info.sender);
            if(_readyPlayers.Count <PlayerHealth.AllPlayers.Count)
                return;
            machine.SetState(_runningState);
        }
        [ServerRpc(requireOwnership: false)]
        private void SkillSelectedRpc(RPCInfo info = default)
        {
            if(_skillSelected.Contains(info.sender))
                return;
            _skillSelected.Add(info.sender);
        
        }

        private void SetUpLevelOptions()
        {
            
            List<SkillData> avaliableSkills = GetAvailableSkills();
            if (avaliableSkills == null || avaliableSkills.Count <= 0)
            {
                SetSkillSelected();
                return;
            }
                
            
            var randomSkills = new List<SkillData>();
            while (randomSkills.Count < 3 && avaliableSkills.Count > 0)
            {
                int rand = UnityEngine.Random.Range(0, avaliableSkills.Count);
                randomSkills.Add(avaliableSkills[rand]);
                avaliableSkills.RemoveAt(rand);
            }

            Service<UIService>.Instance.GetLevelUpPanel().OpenWindow(randomSkills,this);
            
            
        }

        private List<SkillData> GetAvailableSkills()
        {
            if(!InstanceHandler.TryGetInstance(out SkillsHandler skillHandler))
                return null;
            List<SkillData> availableSkills = new List<SkillData>();
            foreach (var skill in _allSkill)
            {
                if(skillHandler.GetSkillLevel(skill.skillId)<skill.maxLevel)
                    availableSkills.Add(skill);
            }
            return availableSkills;
        }
    }
}
