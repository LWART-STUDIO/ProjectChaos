namespace SFAbilitySystem.Demo.Interfaces
{
    /// <summary>
    /// Interface for ability logic components that require external dependencies.
    /// Enables dependency injection for ability implementations.
    /// </summary>
    public interface IAbilityInject
    {
        /// <summary>
        /// Gets the System.Type of the required dependency
        /// </summary>
        /// <returns>
        /// The Type of dependency this ability requires (e.g., typeof(InventorySystem))
        /// </returns>
        System.Type GetDependencyType();

        /// <summary>
        /// Injects the required dependency instance into the ability
        /// </summary>
        /// <param name="instance">
        /// The dependency instance to inject. Must match the type returned by GetDependencyType().
        /// </param>
        void Inject(UnityEngine.Object instance);
    }
}