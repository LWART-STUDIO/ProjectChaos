using System.Collections.Generic;
using System.Linq;
using Game.Scripts.Services.Pool;
using SFAbilitySystem.Core;
using SFAbilitySystem.Demo.Abilities;
using SFAbilitySystem.Demo.Cards;
using SFAbilitySystem.Demo.Core;
using SFAbilitySystem.Demo.Interfaces;
using SFAbilitySystem.Demo.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Client.Logic.Skills
{
    public class SkillsControl : MonoBehaviour, ICardsPoolUpdated
    {
        [SerializeField] private CardSystem _cardSystem;
        [SerializeField] private CardDatabase _activeSkillsDatabase;
        [SerializeField] private List<Object> _injectables = new List<Object>(); 
        [SerializeField] private Transform _logicParent;
        [SerializeField] private Transform _shootPoint;
        private Dictionary<string, ActiveCardEntry> _currentSkills = new Dictionary<string, ActiveCardEntry>();
        public Transform ShootPoint => _shootPoint;
        public class ActiveCardEntry
        {
            public CardData card;               // Reference to the card data
            public ActiveAbilityBase ability;   // The ability configuration
            public ActiveAbilityLogicBase logic; // Runtime logic instance
            public ObjectPool<PoolObject> pool;
            public Image uiElement;              // Associated UI element
        }

        private void Awake()
        {
            _cardSystem.abilityManager.AddCardsPoolUpdatedCallback(this);
            _cardSystem.abilityManager.AddCard(_activeSkillsDatabase.CardAt(0));
        }

      
        public void Update()
        {
            SpawnSkills();
        }

        public void OnCardsPoolUpdated(AbilityManager abilityManager)
        {
            if (abilityManager.TryGetCard(out CardData cardData, out ActiveAbilityBase activeAbilityBase))
            {
                AddAbility(activeAbilityBase, cardData);
                Debug.Log(activeAbilityBase.Name);
            }
        }
        public void SpawnSkills()
        {
            foreach (var kvp in _currentSkills)
            {
                if(kvp.Value.logic==null)
                    continue;
                if(kvp.Value.logic.CurrentCooldown>0)
                    return;
                kvp.Value.logic.PerformAction();
            }

        }

        public ObjectPool<PoolObject> GetOrCreatePool(string name, GameObject prefab)
        {
            if (!_currentSkills.TryGetValue(name, out var entry))
            {
                // Создаём новую запись, если её нет
                entry = new ActiveCardEntry();
                _currentSkills[name] = entry;
            }

            if (entry.pool == null)
            {
                entry.pool = new ObjectPool<PoolObject>(prefab);
            }

            return entry.pool;
        }
        

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
            var existingEntry = _currentSkills.FirstOrDefault(kvp =>
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
         //   var availableSlot = _abilityHotkeys.FirstOrDefault(x => x.Value == null);
            /*if (availableSlot.Equals(default(KeyValuePair<KeyCode, ActiveCardEntry>)))
            {
                Debug.LogWarning("No available ability slots");
                Destroy(logicInstance);
                return;
            }*/

            // Setup new entry
            //int slotIndex = _abilityHotkeys.Keys.ToList().IndexOf(availableSlot.Key);
           // Image uiElement = slotIndex < _abilityIcons.Length ? _abilityIcons[slotIndex] : null;

            var newEntry = new ActiveCardEntry
            {
                card = cardData,
                ability = abilityBase,
                logic = logic,
                uiElement = null,
                pool = null
            };

            _currentSkills[newEntry.card.abilityName] = newEntry;
            // logic.Initialize(availableSlot.Key);

            // Update UI
            /*if (uiElement != null)
            {
                uiElement.sprite = cardData.abilityIcon;
                uiElement.gameObject.SetActive(true);
            }*/

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

        private void OnDisable()
        {
            _cardSystem.abilityManager.RemoveCardsPoolUpdatedCallback(this);
        }
    }
}
