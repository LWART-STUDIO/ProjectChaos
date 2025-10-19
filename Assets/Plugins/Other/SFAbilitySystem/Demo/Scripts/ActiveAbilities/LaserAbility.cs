namespace SFAbilitySystem.Demo.Abilities
{
    /// <summary>
    /// A continuous laser beam ability that deals damage over time to targets in its path.
    /// The laser persists for a duration and deals damage at regular intervals.
    /// </summary>
    public class LaserAbility : ActiveAbilityBase
    {
        /// <summary>
        /// Base damage dealt per damage tick (in health units).
        /// Total damage = (duration / damageTickInterval) * damage
        /// </summary>
        public float damage = 10f;

        /// <summary>
        /// Total duration the laser remains active (in seconds).
        /// The laser will automatically deactivate after this time.
        /// </summary>
        public float duration = 5f;

        /// <summary>
        /// Maximum distance the laser can reach (in world units).
        /// Targets beyond this range won't be affected.
        /// </summary>
        public float range = 50f;

        /// <summary>
        /// Interval between damage applications (in seconds).
        /// Lower values mean more frequent damage ticks.
        /// </summary>
        public float damageTickInterval = 0.2f;
    }
}