using System;
using System.Collections.Generic;
using Game.Scripts.Client.Logic.Game;
using Game.Scripts.Client.Logic.Player;
using Game.Scripts.Client.Logic.Skills;
using PurrNet;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Client.UI.Game
{
    public class SkillLevelUp : MonoBehaviour
    {
        [SerializeField] private List<ButtonData> _buttons;
        private SkillData _skill;
        private GameStateLevelUp _levelUp;

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
