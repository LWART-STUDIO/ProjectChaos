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
        [SerializeField] private Image _deselectedBackGround;
        
        [SerializeField] private Color _normalFillIcon;
        [SerializeField] private Color _selectedFillIcon;
        [SerializeField] private Color _highlightFillIcon;

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
            if (selected)
            {
                _background.gameObject.SetActive(true);
                _deselectedBackGround.gameObject.SetActive(false);
                _icon.color =  _selectedFillIcon; 
                _outline.gameObject.SetActive(true);
            }
            else
            {
                _background.gameObject.SetActive(false);
                _deselectedBackGround.gameObject.SetActive(true);
                _icon.color = !hovered ? _normalFillIcon : _highlightFillIcon;
                _outline.gameObject.SetActive(false);
            }
            
            
        }
    }
}