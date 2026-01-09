using System;
using System.Linq;
using GIGA.AutoRadialLayout;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Client.Logic.Tree
{
    public class TreeLinkView : MonoBehaviour
    {
        public int fromId;
        public int toId;

        [SerializeField] private Image line;
        [SerializeField] private Color inactiveColor;
        [SerializeField] private Color activeColor;

        [SaintsField.Playa.Button]
        public void UpdateIds()
        {
            RadialLayoutLink link =GetComponent<RadialLayoutLink>();
            fromId = link.from.GetComponent<TreeNodeView>().Id;
            toId = link.to.GetComponent<TreeNodeView>().Id;
        }

        public void Refresh(bool active)
        {
            line.color = active ? activeColor : inactiveColor;
        }
    }
}