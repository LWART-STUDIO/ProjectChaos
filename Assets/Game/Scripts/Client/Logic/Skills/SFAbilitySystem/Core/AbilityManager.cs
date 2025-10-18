using SFAbilitySystem.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SFAbilitySystem.Core
{
    [Serializable]
    public class AbilityManager
    {
        [SerializeField, ReadOnlyInspector]
        private List<CardEntry> _currentCards = new();
        [SerializeField]
        private HashSet<ICardsPoolUpdated> _cardsPoolUpdatedCallbacks = new HashSet<ICardsPoolUpdated>();

        /// <summary>
        /// Provides direct access to the list of all current cards (by reference)
        /// </summary>
        public ref List<CardEntry> AllCards => ref _currentCards;

        #region Card Retrieval Methods

        /// <summary>
        /// Attempts to find a card with a specific ability type, returning the card, ability, and level
        /// </summary>
        public bool TryGetCard<CD, T>(out CD card, out T ability, out int level)
            where T : AbilityBase where CD : CardData
        {
            card = default;
            ability = default;
            level = -1;

            var currentCard = FindFirstCardWithAbility<T>();
            if (currentCard == default) return false;

            level = currentCard.level;
            card = currentCard.card as CD;
            ability = card?.GetAbility<T>(currentCard.level);

            return card != null;
        }

        /// <summary>
        /// Attempts to find a card with a specific ability type, returning the card and ability
        /// </summary>
        public bool TryGetCard<CD, T>(out CD card, out T ability)
            where T : AbilityBase where CD : CardData
        {
            card = default;
            ability = default;

            var currentCard = FindFirstCardWithAbility<T>();
            if (currentCard == default) return false;

            card = currentCard.card as CD;
            ability = card?.GetAbility<T>(currentCard.level);

            return card != null;
        }

        /// <summary>
        /// Attempts to find all cards with a specific ability type, returning arrays of cards and abilities
        /// </summary>
        public bool TryGetCards<CD, Ability>(ref CD[] cards, ref Ability[] abilities)
            where Ability : AbilityBase where CD : CardData
        {
            cards = default;
            abilities = default;

            var matchingCards = _currentCards.Where(s => s.card.GetAbility<Ability>(s.level) != null).ToArray();
            if (!matchingCards.Any()) return false;

            cards = new CD[matchingCards.Length];
            abilities = new Ability[matchingCards.Length];

            for (int i = 0; i < matchingCards.Length; i++)
            {
                cards[i] = matchingCards[i].card as CD;
                abilities[i] = cards[i]?.GetAbility<Ability>(matchingCards[i].level);
            }

            return cards != null;
        }

        /// <summary>
        /// Attempts to find a card with a specific ability type, returning just the card
        /// </summary>
        public bool TryGetCard<T>(out CardData card) where T : AbilityBase
        {
            card = FindFirstCardWithAbility<T>()?.card;
            return card != null;
        }

        /// <summary>
        /// Attempts to find a card with a specific ability type, returning just the ability
        /// </summary>
        public bool TryGetCard<T>(out T ability) where T : AbilityBase
        {
            ability = default;
            var currentCard = FindFirstCardWithAbility<T>();

            if (currentCard == default) return false;

            ability = currentCard.card.GetAbility<T>(currentCard.level);
            return ability != null;
        }

        /// <summary>
        /// Helper method to find the first card with a specific ability type
        /// </summary>
        private CardEntry FindFirstCardWithAbility<T>() where T : AbilityBase
        {
            return _currentCards.FirstOrDefault(s => s.card.GetAbility<T>(s.level) != null);
        }

        #endregion

        #region Card Management

        /// <summary>
        /// Adds a card to the manager, either inserting new or updating existing
        /// </summary>
        /// <param name="card">The card to add</param>
        /// <param name="level">Specific level to set (-1 to increment current level)</param>
        public void AddCard(CardData card, int level = -1)
        {
            bool isNewCard = !_currentCards.Any(s => s.card == card);

            if (isNewCard)
            {
                // Add new card at the front with level 0 or specified level
                _currentCards.Insert(0, new CardEntry(card, Math.Max(level, 0)));
            }
            else
            {
                // Move existing card to front
                MoveToFront(ref _currentCards, a => a.card == card);

                // Update the card's level
                var updatedCard = _currentCards[0];
                updatedCard.level = level >= 0
                    ? level
                    : (int)Mathf.Clamp(updatedCard.level + 1, 0, updatedCard.card.abilityTiersCount - 1);

                _currentCards[0] = updatedCard;
            }

            NotifyCallbacks();
        }

        /// <summary>
        /// Removes a card from the manager
        /// </summary>
        public void RemoveCard(CardData card)
        {
            var cardToRemove = _currentCards.FirstOrDefault(s => s.card == card);
            if (cardToRemove != null)
            {
                _currentCards.Remove(cardToRemove);
                NotifyCallbacks();
            }
        }

        /// <summary>
        /// Gets the index of a card in the current cards list
        /// </summary>
        public int CardIndex(CardData card)
        {
            return _currentCards.FindIndex(s => s.card == card);
        }

        /// <summary>
        /// Updates the entire card list and optionally notifies callbacks
        /// </summary>
        public void UpdateCards(List<CardEntry> cardsData, bool notifyCallback = true)
        {
            _currentCards = cardsData;
            if (notifyCallback)
            {
                NotifyCallbacks();
            }
        }

        /// <summary>
        /// Moves an item matching the predicate to the front of the list
        /// </summary>
        public static bool MoveToFront<T>(ref List<T> list, Predicate<T> match)
        {
            int index = list.FindIndex(match);

            if (index == -1) return false;
            if (index == 0) return true; // Already at front

            T item = list[index];
            list.RemoveAt(index);
            list.Insert(0, item);

            return true;
        }

        #endregion

        #region Callback Management

        /// <summary>
        /// Notifies all registered callbacks about card pool changes
        /// </summary>
        private void NotifyCallbacks()
        {
            var callbacks = _cardsPoolUpdatedCallbacks;
            foreach (var callback in callbacks)
            {
                try
                {
                    callback?.OnCardsPoolUpdated(this);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error in callback notification: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Registers a callback to be notified when the card pool changes
        /// </summary>
        public void AddCardsPoolUpdatedCallback(ICardsPoolUpdated callback)
        {
            if (callback == null || _cardsPoolUpdatedCallbacks.Contains(callback))
                return;

            _cardsPoolUpdatedCallbacks.Add(callback);

            try
            {
                callback.OnCardsPoolUpdated(this);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in initial callback notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Unregisters a callback from card pool change notifications
        /// </summary>
        public void RemoveCardsPoolUpdatedCallback(ICardsPoolUpdated callback)
        {
            if (callback != null)
            {
                _cardsPoolUpdatedCallbacks.Remove(callback);
            }
        }

        #endregion
    }
}