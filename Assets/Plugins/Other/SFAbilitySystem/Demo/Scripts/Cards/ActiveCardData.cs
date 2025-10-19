using SFAbilitySystem.Core;
using SFAbilitySystem.Demo.Core;
using UnityEngine;

namespace SFAbilitySystem.Demo.UI
{
    /// <summary>
    /// ScriptableObject representing an active (player-activated) card in the ability system.
    /// Extends base CardData with specific functionality for active abilities.
    /// </summary>
    [CreateAssetMenu(fileName = "ActiveCard", menuName = "SFAbilitySystem/ActiveCard", order = 2)]
    public class ActiveCardData : CardData
    {
        /// <summary>
        /// The prefab containing the active ability's gameplay logic and behavior.
        /// Can be replaced by NetworkBehaviour
        /// </summary>
        public MonoBehaviour abilityLogicPrefab => _abilityLogicPrefab;

        [Tooltip("Prefab with the ActiveAbilityLogicBase component that implements this card's behavior")]
        [SerializeField] private ActiveAbilityLogicBase _abilityLogicPrefab;
    }
}