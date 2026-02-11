using System;
using System.Collections.Generic;
using Game.Scripts.Client.Logic.Game;
using Game.Scripts.Client.Logic.Player;
using Game.Scripts.Client.Logic.Skills;
using Michsky.MUIP;
using PurrNet;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Client.UI.Game.LevelUpUI
{
    public class SkillLevelUp : MonoBehaviour
    {
        [SerializeField] private List<ButtonData> _buttons;
        [SerializeField] private ButtonManager _buttonManager;
        private SkillData _skill;
        private GameStateLevelUp _levelUp;
        private GameStatePassiveSkillGrant _levelPassive;

        public void Init(SkillData skill, GameStateLevelUp levelUp)
        {
            if(!InstanceHandler.TryGetInstance(out SkillsHandler skillHandler))
                return;
            
            _skill = skill;
            _levelUp = levelUp;
            foreach (var button in _buttons)
            {
                button.Icon.sprite = _skill.icon;
                button.Title.text = _skill.skillName;
                button.Description.text = _skill.GetLevelDescription(skillHandler.GetSkillLevel(_skill.skillId) + 1);
                
            }
            _buttonManager.onClick.RemoveAllListeners();
            _buttonManager.onClick.AddListener(() => PickUpUpgrade());
        }
        public void Init(SkillData skill, GameStatePassiveSkillGrant levelUp)
        {
            if(!InstanceHandler.TryGetInstance(out SkillsHandler skillHandler))
                return;
            
            _skill = skill;
            _levelPassive = levelUp;
            foreach (var button in _buttons)
            {
                button.Icon.sprite = _skill.icon;
                button.Title.text = _skill.skillName;
                button.Description.text = _skill.GetLevelDescription(skillHandler.GetSkillLevel(_skill.skillId) + 1);
            }
            _buttonManager.onClick.RemoveAllListeners();
            _buttonManager.onClick.AddListener(() => PickUpPassiveUpgrade());
        }

        public void PickUpPassiveUpgrade()
        {
            if(!InstanceHandler.TryGetInstance(out SkillsHandler skillHandler))
                return;
            skillHandler.AddPassiveSkill(_skill);
            _levelPassive.SetSkillSelected();
        }
        public void PickUpUpgrade()
        {
            if(!InstanceHandler.TryGetInstance(out SkillsHandler skillHandler))
                return;
            skillHandler.AddSkill(_skill);
            _levelUp.SetSkillSelected();
        }
        [Serializable]
        private struct ButtonData
        {
            public Image Icon;
            public TMP_Text Title;
            public TMP_Text Description;
        }
    }
}
