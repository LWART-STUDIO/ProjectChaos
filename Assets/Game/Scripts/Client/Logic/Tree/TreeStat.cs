using System.Collections.Generic;
using SaintsField;
using SaintsField.Playa;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Player.Stats
{
    [CreateAssetMenu(fileName = "TreeStat", menuName = "Skills/Tree/TreeStat")]
    public class TreeStat : ScriptableObject
    {
        public Sprite icon;
        [TextArea] public string description;
        [Expandable]
        public List<SimpleStat> bonuses; // можно несколько бонусов
        

        [Button]
        private void UpdateDescription()
        {
            description = "";
            foreach (var bonuse in bonuses)
            {
                description += $"{bonuse.description}  \n";
            }
           
        }
    }

}
