using SFAbilitySystem.Core;
using SFAbilitySystem.Demo.UI;
using System.Collections.Generic;
using UnityEngine;

namespace SFAbilitySystem.Demo.Cards
{
    /// <summary>
    /// Manages the creation, display, and selection of card UI elements.
    /// Handles card spawning, selection events, and view updates.
    /// </summary>
    public class CardViewHandler : MonoBehaviour
    {
        [Header("Dependencies")]
        [Tooltip("Reference to the card database")]
        [SerializeField] private CardDatabase _cardDatabase;

        [Tooltip("Reference to the card system")]
        [SerializeField] private CardSystem _cardSystem;

        [Header("UI Components")]
        [Tooltip("Prefab for card view UI elements")]
        [SerializeField] private CardView _cardViewPrefab;

        [Tooltip("Parent transform for card instances")]
        [SerializeField] private Transform _cardViewParent;

        private List<CardView> _currentCards = new List<CardView>();

        private void Start()
        {
            SpawnCards();
        }

        /// <summary>
        /// Clears all current card views and unsubscribes from events
        /// </summary>
        public void Clear()
        {
            foreach (var cardView in _currentCards)
            {
                if (cardView != null)
                {
                    cardView.OnClicked -= OnCardSelected;
                    Destroy(cardView.gameObject);
                }
            }
            _currentCards.Clear();
        }

        /// <summary>
        /// Spawns new card views with random selections from available cards
        /// </summary>
        /// <param name="count">Number of cards to spawn (default: 3)</param>
        public void SpawnCards(int count = 3)
        {
            Clear();

            var availableCards = _cardSystem.abilityManager.AllCards.ToArray();
            var selectedCards = _cardDatabase.GetRandomCards(availableCards, count);

            foreach (var cardEntry in selectedCards)
            {
                if (cardEntry == null) continue;

                var cardView = Instantiate(_cardViewPrefab, _cardViewParent);
                cardView.Initialize(cardEntry);
                cardView.OnClicked += OnCardSelected;
                _currentCards.Add(cardView);
            }
        }

        /// <summary>
        /// Handles card selection events
        /// </summary>
        /// <param name="selectedCard">The card data that was selected</param>
        private void OnCardSelected(CardData selectedCard)
        {
            if (selectedCard == null) return;

            _cardSystem.abilityManager.AddCard(selectedCard);
            SpawnCards(); // Refresh with new card selection
        }

        /// <summary>
        /// Ensures proper cleanup when destroyed
        /// </summary>
        private void OnDestroy()
        {
            Clear();
        }
    }
}