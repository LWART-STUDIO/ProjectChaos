using System.Collections.Generic;
using Game.Scripts.Client.Logic.Player.Stats;
using SaintsField;
using SaintsField.Playa;
using SoftKitty.WSFL;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Tree
{
    [CreateAssetMenu(fileName = "TreeStat", menuName = "Skills/Tree/TreeStat")]
    public class TreeStat : ScriptableObject
    {
        public Sprite icon;
        [TextArea] public string description;
        [Expandable]
        public List<SimpleStat> bonuses; // можно несколько бонусов
        

        [Button]
        public void UpdateDescription()
        {
            description = "";
            foreach (var bonus in bonuses)
            {
                description += $"{Localization.GetString(bonus.description)}  \n";
            }
           
        }
    }

}
