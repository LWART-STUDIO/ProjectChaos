using System.Collections.Generic;
using Game.Scripts.Client.Logic.Player.Stats;
using GIGA.AutoRadialLayout;
using GIGA.AutoRadialLayout.QuerySystem;
using SaintsField;
using SaintsField.Playa;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Scripts.Client.Logic.Tree
{
    public class TreeNodeView : MonoBehaviour
    {
        [Expandable] public TreeStat node;
        private int id;
        public int Id => id;
        private PlayerSkillTree tree;

        [SerializeField] private Image fill;
        [SerializeField] private Image outline;

        [SerializeField] private Color normalFill;
        [SerializeField] private Color selectedFill;
        [SerializeField] private Color outlineColor;
        private bool isHovered;

        public void Init(PlayerSkillTree skillTree)
        {
            tree = skillTree;
            Refresh(tree);
        }

        public void OnPointerEnter()
        {
            isHovered = true;
            Refresh(tree);
        }

        public void OnPointerExit()
        {
            isHovered = false;
            Refresh(tree);
        }

        public void OnPointerClick()
        {
            if (tree.TrySelectNode(this))
                Refresh(tree);
        }
        [Button]
        public void UpdateID()
        {
            id=GetComponentInParent<RadialLayoutQueryTarget>().UniqueId;
        }
        public void Refresh(PlayerSkillTree tree)
        {
            bool selected = tree.IsSelected(this);
            bool hovered = isHovered; // выставляется OnPointerEnter/Exit

            outline.enabled = selected || hovered;
            outline.color = outlineColor;
            fill.color = selected ? selectedFill : normalFill;
        }
    }
}