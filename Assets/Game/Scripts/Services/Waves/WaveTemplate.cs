using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.Services.Waves
{
    [CreateAssetMenu(menuName = "Game/Waves/Wave Template")]
    public class WaveTemplate : ScriptableObject
    {
        public string Name;
        [Range(0f, 10f)] public float Weight = 1f;
        public List<EnemyWeight> EnemyWeights = new();
    }
}