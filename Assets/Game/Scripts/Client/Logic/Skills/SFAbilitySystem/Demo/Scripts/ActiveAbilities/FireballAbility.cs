namespace SFAbilitySystem.Demo.Abilities
{
    /// <summary>
    /// Concrete implementation of a fireball projectile ability.
    /// Inherits timing parameters from ActiveAbilityBase and adds fireball-specific properties.
    /// </summary>
    public class FireballAbility : ActiveAbilityBase
    {
        /// <summary>
        /// Base damage dealt by the fireball on impact.
        /// This value can be modified by character stats or ability upgrades.
        /// </summary>
        public float damage = 50f;

        /// <summary>
        /// Travel speed of the fireball projectile in units per second.
        /// Affects how quickly the projectile reaches its target.
        /// </summary>
        public float projectileSpeed = 30f;
    }
}