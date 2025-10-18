
namespace SFAbilitySystem.Core
{
    /// <summary>
    /// Interface for objects that need to react to card pool updates in the ability system.
    /// Implement this to receive notifications when available cards change.
    /// </summary>
    public interface ICardsPoolUpdated
    {
        /// <summary>
        /// Called when the available card pool is updated.
        /// </summary>
        /// <param name="abilityManager">The AbilityManager instance that triggered the update,
        /// providing access to the current card collection and game state.</param>
        void OnCardsPoolUpdated(AbilityManager abilityManager);
    }
}