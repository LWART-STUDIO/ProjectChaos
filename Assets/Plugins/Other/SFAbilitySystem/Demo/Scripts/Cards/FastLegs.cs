using SFAbilitySystem.Core;
using UnityEngine;

namespace SFAbilitySystem.Demo.Cards
{
    /// <summary>
    /// Passive movement speed enhancement ability.
    /// Applies a multiplicative speed boost to the character's base movement speed.
    /// </summary>
    public class FastLegs : AbilityBase
    {
        /// <summary>
        /// Multiplier applied to the character's movement speed (e.g., 1.2 = 20% speed increase).
        /// Values below 1 will reduce movement speed.
        /// </summary>
        [Tooltip("Movement speed multiplier (1.2 = +20% speed)")]
        public float speedMultiplier = 1.2f;
    }
}