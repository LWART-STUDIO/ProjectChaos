using UnityEngine;
namespace SFAbilitySystem.Core
{
    [System.Serializable]  // Allows Unity serialization of this container
    public class AbilityContainer
    {
        [SerializeReference]  // Enables polymorphic serialization of AbilityBase derivatives
        public AbilityBase[] abilityTiers = new AbilityBase[0];  // Array of ability instances by tier

        public int Count => abilityTiers != null ? abilityTiers.Length : 0;  // Convenience property for tier count
    }
}