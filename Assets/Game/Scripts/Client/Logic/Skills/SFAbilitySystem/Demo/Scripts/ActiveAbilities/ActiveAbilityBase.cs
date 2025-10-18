using SFAbilitySystem.Core;

namespace SFAbilitySystem.Demo.Abilities
{
    /// <summary>
    /// Abstract base class for all active (player-triggered) abilities in the ability system.
    /// Provides core timing parameters common to all active abilities.
    /// </summary>
    public abstract class ActiveAbilityBase : AbilityBase
    {
        /// <summary>
        /// The cooldown time in seconds before this ability can be used again.
        /// Set to 0 for abilities with no cooldown.
        /// </summary>
        public float cooldown = 5f;

        /// <summary>
        /// The casting time in seconds required to activate this ability.
        /// During this time, the character is typically locked in the casting animation.
        /// Set to 0 for instant-cast abilities.
        /// </summary>
        public float castTime = 0.5f;
    }
}