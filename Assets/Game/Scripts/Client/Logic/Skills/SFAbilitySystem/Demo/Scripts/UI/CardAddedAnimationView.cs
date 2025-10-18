using SFAbilitySystem.Core;
using SFAbilitySystem.Demo.Cards;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace SFAbilitySystem.Demo.UI
{
    /// <summary>
    /// Handles the visual presentation when new cards are added to the player's collection.
    /// Manages card animation sequences and background display timing.
    /// </summary>
    public class CardAddedAnimationView : MonoBehaviour, ICardsPoolUpdated
    {
        [Header("Animation Components")]
        [Tooltip("Prefab containing the animated UI card")]
        [SerializeField] private AnimatedUICard _animatedUICardPrefab;

        [Header("System References")]
        [Tooltip("Reference to the card system for ability updates")]
        [SerializeField] private CardSystem _cardSystem;

        [Header("UI Elements")]
        [Tooltip("Background container for the animated card")]
        [SerializeField] private RectTransform _background;

        private void OnEnable()
        {
            _cardSystem.abilityManager.AddCardsPoolUpdatedCallback(this);
        }

        private void OnDisable()
        {
            _cardSystem.abilityManager.RemoveCardsPoolUpdatedCallback(this);
        }

        /// <summary>
        /// Handles new card notifications and triggers the animation sequence
        /// </summary>
        /// <param name="abilityManager">Source of the card update</param>
        public void OnCardsPoolUpdated(AbilityManager abilityManager)
        {
            var newCard = abilityManager.AllCards.FirstOrDefault();
            if (newCard == default) return;

            ShowCardAnimation(newCard.card);
        }

        /// <summary>
        /// Displays and animates a new card
        /// </summary>
        /// <param name="cardData">The card to animate</param>
        private void ShowCardAnimation(CardData cardData)
        {
            _background.gameObject.SetActive(true);

            AnimatedUICard cardView = Instantiate(_animatedUICardPrefab, _background);
            cardView.Init(cardData);

            StartCoroutine(HideBackgroundAfterDelay(cardView.GetTotalTime()));
        }

        /// <summary>
        /// Hides the background after the animation completes
        /// </summary>
        /// <param name="delay">Time to wait before hiding (animation duration)</param>
        private IEnumerator HideBackgroundAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            _background.gameObject.SetActive(false);
        }
    }
}