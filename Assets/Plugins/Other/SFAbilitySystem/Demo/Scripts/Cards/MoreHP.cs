using SFAbilitySystem.Core;
using System;
using UnityEngine;

namespace SFAbilitySystem.Demo.Cards
{
    /// <summary>
    /// Passive ability that increases the character's maximum health.
    /// The health boost is applied as a percentage of base health.
    /// </summary>
    [Serializable]
    public class MoreHP : AbilityBase
    {
        /// <summary>
        /// Health boost multiplier (0.1 = +10% max HP).
        /// Applied additively with other HP modifiers.
        /// </summary>
        [Tooltip("Percentage boost to max health (0.1 = +10%)")]
        public float hpBoost = 0.1f;
    }
}