using SFAbilitySystem.Core;
using SFAbilitySystem.Demo.Abilities;
using SFAbilitySystem.Demo.Cards;
using SFAbilitySystem.Demo.Interfaces;
using SFAbilitySystem.Demo.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SFAbilitySystem.Demo.Core
{
    /// <summary>
    /// Manages active abilities, handling their instantiation, input, cooldowns, and UI.
    /// Implements ICardsPoolUpdated to receive new ability cards from the ability system.
    /// </summary>
    public class ActiveAbilitiesController : MonoBehaviour, ICardsPoolUpdated
    {
        /// <summary>
        /// Container class for active ability instances and their associated data
        /// </summary>
        public class ActiveCardEntry
        {
            public CardData card;               // Reference to the card data
            public ActiveAbilityBase ability;   // The ability configuration
            public ActiveAbilityLogicBase logic; // Runtime logic instance
            public Image uiElement;              // Associated UI element
        }

        [SerializeField] private Transform _logicParent; // Parent transform for ability logic instances
        [SerializeField] private List<Object> _injectables = new List<Object>(); // Objects available for dependency injection
        [SerializeField] private CardSystem _cardSystem; // Reference to the card system

        [Header("UI Settings")]
        [SerializeField] private Image[] _abilityIcons; // UI icons for each ability slot

        // Hotkey mapping (1-3 keys by default)
        private Dictionary<KeyCode, ActiveCardEntry> _abilityHotkeys = new Dictionary<KeyCode, ActiveCardEntry>()
    {
        { KeyCode.Alpha1, null },
        { KeyCode.Alpha2, null },
        { KeyCode.Alpha3, null }
    };

        private void Start()
        {
            // Initialize all ability icons as inactive
            foreach (var abilityIcon in _abilityIcons)
            {
                abilityIcon.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            _cardSystem.abilityManager.AddCardsPoolUpdatedCallback(this);
        }

        private void OnDisable()
        {
            _cardSystem.abilityManager.RemoveCardsPoolUpdatedCallback(this);
        }

        private void Update()
        {
            HandleAbilityInput();
            UpdateCooldownVisuals();
        }

        /// <summary>
        /// Updates cooldown visuals for all active abilities
        /// </summary>
        private void UpdateCooldownVisuals()
        {
            foreach (var kvp in _abilityHotkeys)
            {
                if (kvp.Value == null || kvp.Value.uiElement == null) continue;

                var logic = kvp.Value.logic;
                var ability = kvp.Value.ability;

                // Update fill amount based on cooldown progress
                kvp.Value.uiElement.fillAmount = logic.CurrentCooldown > 0
                    ? 1f - (logic.CurrentCooldown / ability.cooldown)
                    : 1f;
            }
        }

        /// <summary>
        /// Handles player input for ability activation
        /// </summary>
        private void HandleAbilityInput()
        {
            foreach (var kvp in _abilityHotkeys)
            {
                if (Input.GetKeyDown(kvp.Key) && kvp.Value != null)
                {
                    kvp.Value.logic?.PerformAction();
                }
            }
        }

        /// <summary>
        /// Adds or updates an active ability in the controller
        /// </summary>
        /// <param name="abilityBase">The ability configuration</param>
        /// <param name="cardData">The source card data</param>
        public void AddAbility(ActiveAbilityBase abilityBase, CardData cardData)
        {
            // Validate card type
            if (!(cardData is ActiveCardData activeCardData))
            {
                Debug.LogError("CardData is not of type ActiveCardData");
                return;
            }

            // Validate logic prefab
            if (activeCardData.abilityLogicPrefab == null)
            {
                Debug.LogError("Ability logic prefab is not assigned");
                return;
            }

            // Check for existing ability
            var existingEntry = _abilityHotkeys.FirstOrDefault(kvp =>
                kvp.Value != null && kvp.Value.card == cardData).Value;

            if (existingEntry != null)
            {
                UpdateExistingAbility(existingEntry, abilityBase, cardData);
                return;
            }

            // Create new ability instance
            var logicInstance = Instantiate(activeCardData.abilityLogicPrefab, _logicParent);
            var logic = logicInstance.GetComponent<ActiveAbilityLogicBase>();

            if (logic == null)
            {
                Debug.LogError("Logic prefab doesn't implement ActiveAbilityLogicBase");
                Destroy(logicInstance);
                return;
            }

            // Initialize logic
            logic.Initialize(_cardSystem.abilityManager, abilityBase);

            // Find available slot
            var availableSlot = _abilityHotkeys.FirstOrDefault(x => x.Value == null);
            if (availableSlot.Equals(default(KeyValuePair<KeyCode, ActiveCardEntry>)))
            {
                Debug.LogWarning("No available ability slots");
                Destroy(logicInstance);
                return;
            }

            // Setup new entry
            int slotIndex = _abilityHotkeys.Keys.ToList().IndexOf(availableSlot.Key);
            Image uiElement = slotIndex < _abilityIcons.Length ? _abilityIcons[slotIndex] : null;

            var newEntry = new ActiveCardEntry
            {
                card = cardData,
                ability = abilityBase,
                logic = logic,
                uiElement = uiElement
            };

            _abilityHotkeys[availableSlot.Key] = newEntry;
            logic.Initialize(availableSlot.Key);

            // Update UI
            if (uiElement != null)
            {
                uiElement.sprite = cardData.abilityIcon;
                uiElement.gameObject.SetActive(true);
            }

            // Handle dependency injection
            if (logic is IAbilityInject injectable)
            {
                var dependency = _injectables.FirstOrDefault(injectable.GetDependencyType().IsInstanceOfType);
                if (dependency != null)
                {
                    injectable.Inject(dependency);
                }
            }
        }

        /// <summary>
        /// Updates an existing ability entry with new data
        /// </summary>
        private void UpdateExistingAbility(ActiveCardEntry entry, ActiveAbilityBase abilityBase, CardData cardData)
        {
            entry.ability = abilityBase;
            entry.logic.Initialize(_cardSystem.abilityManager, abilityBase);

            if (entry.uiElement != null)
            {
                entry.uiElement.sprite = cardData.abilityIcon;
                entry.uiElement.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Callback for when the available card pool is updated
        /// </summary>
        public void OnCardsPoolUpdated(AbilityManager abilityManager)
        {
            if (abilityManager.TryGetCard(out CardData cardData, out ActiveAbilityBase activeAbilityBase))
            {
                AddAbility(activeAbilityBase, cardData);
            }
        }
    }
}