using SFAbilitySystem.Core;
using UnityEngine;

namespace SFAbilitySystem.Demo.Cards
{
    /// <summary>
    /// Core system for managing card-based abilities in the game.
    /// Serves as the main access point for card-related functionality.
    /// </summary>
    public class CardSystem : MonoBehaviour
    {
        /// <summary>
        /// Reference to the AbilityManager that handles ability logic and progression.
        /// This provides access to the player's current abilities and upgrades.
        /// </summary>
        [Tooltip("Reference to the AbilityManager that handles ability logic and progression")]
        public AbilityManager abilityManager;
    }
}