using System;
using System.Collections.Generic;
using PurrNet;
using Sisus.Init;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Skills
{


    public abstract class SkillData:ScriptableObject
    {
        public string skillId;
        public string skillName;
        [SerializeField] private List<string> levelDescriptions = new List<string>();
        public Sprite icon;
        public Skill prefab;
        public int maxLevel=>levelDescriptions.Count-1;
        public string GetLevelDescription(int level)=>levelDescriptions[Mathf.Clamp(level,0,levelDescriptions.Count-1)];

    }

 
}