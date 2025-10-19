
using SFAbilitySystem.Core;
using SFAbilitySystem.Demo.Abilities;
using UnityEngine;

namespace SFAbilitySystem.Demo.Core
{
    /// <summary>
    /// Base class for all active ability logic implementations.
    /// Handles the runtime behavior and execution of active abilities.
    /// </summary>
    public abstract class ActiveAbilityLogicBase : MonoBehaviour
    {
        /// <summary>
        /// Indicates whether the ability is ready to be used (cooldown complete, resources available, etc.)
        /// </summary>
        protected abstract bool IsReady { get; }

        /// <summary>
        /// Current remaining cooldown time in seconds.
        /// </summary>
        public float CurrentCooldown { get; protected set; }

        /// <summary>
        /// Initializes the ability with required systems and configuration.
        /// </summary>
        /// <param name="abilityManager">Reference to the ability manager system</param>
        /// <param name="abilityBase">Configuration data for this ability</param>
        public abstract void Initialize(AbilityManager abilityManager, ActiveAbilityBase abilityBase);

        /// <summary>
        /// Performs hotkey-specific initialization.
        /// </summary>
        /// <param name="hotkey">The key bound to this ability</param>
        public abstract void Initialize(KeyCode hotkey);

        /// <summary>
        /// Executes the ability's primary action.
        /// Should contain the main gameplay logic for the ability.
        /// </summary>
        public abstract void PerformAction();
    }
}