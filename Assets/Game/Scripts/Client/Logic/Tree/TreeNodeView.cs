using System;
using System.Collections.Generic;
using Game.Scripts.Client.Logic.Player.Stats;
using GIGA.AutoRadialLayout;
using GIGA.AutoRadialLayout.QuerySystem;
using Michsky.MUIP;
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

        [SerializeField]private Image _icon;
        [SerializeField] private Image _background;
        [SerializeField] private Image _highlight;
        [SerializeField] private Image _outline;

        [SerializeField] private Color _normalFillBackground;
        [SerializeField] private Color _selectedFillBackground;
        [SerializeField] private Color _highlightColor;
        [SerializeField] private Color _normalFillIcon;
        [SerializeField] private Color _selectedFillIcon;
        [SerializeField] private Color _normalFillOutline;
        [SerializeField] private Color _selectedFillOutline;
        [SerializeField] private TooltipContent _tooltip;
        private bool isHovered;

        public void Init(PlayerSkillTree skillTree)
        {
            tree = skillTree;
            if(node!=null&&_tooltip!=null)
                _tooltip.description = node.description;
            Refresh(tree);
        }
#if UNITY_EDITOR
        [Button]
        private void OnValidate()
        {
            if (node == null || node.icon == null)
            {
                _icon.enabled = false;
                return;
            }
            _icon.enabled = true;
            _icon.sprite = node.icon;
                
        }
#endif

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
            if(_icon.sprite==null)
                _icon.enabled = false;
            bool selected = tree.IsSelected(this);
            bool hovered = isHovered; // выставляется OnPointerEnter/Exit

            _highlight.enabled = selected || hovered;
            _highlight.color = _highlightColor;
            _background.color = selected ? _selectedFillBackground : _normalFillBackground;
            _icon.color = selected ? _selectedFillIcon : _normalFillIcon;
            _outline.color = selected ? _selectedFillOutline : _normalFillOutline;
        }
    }
}