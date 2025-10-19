using SFAbilitySystem.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SFAbilitySystem.Demo.UI
{
    /// <summary>
    /// Represents a single card in the UI, handling display and click interactions.
    /// Manages visual presentation and user input for card elements.
    /// </summary>
    public class CardView : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI Components")]
        [Tooltip("Image component displaying the card's icon")]
        [SerializeField] private Image _icon;

        [Tooltip("Text component displaying the card's description")]
        [SerializeField] private TextMeshProUGUI _descriptionText;

        [Header("Events")]
        [Tooltip("Event triggered when card is clicked")]
        public UnityAction<CardData> OnClicked;

        private CardEntry _currentCard;

        /// <summary>
        /// Initializes the card view with specific card data
        /// </summary>
        /// <param name="cardEntry">Card data to display</param>
        public void Initialize(CardEntry cardEntry)
        {
            _currentCard = cardEntry;
            UpdateVisuals();
        }

        /// <summary>
        /// Updates all visual elements to reflect current card data
        /// </summary>
        private void UpdateVisuals()
        {
            if (_currentCard == null || _currentCard.card == null) return;

            _icon.sprite = _currentCard.card.abilityIcon;
            _descriptionText.text = _currentCard.card.GetDescription(_currentCard.level + 1);
        }

        /// <summary>
        /// Handles click/tap interactions with the card
        /// </summary>
        /// <param name="eventData">Pointer event data</param>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (_currentCard?.card != null)
            {
                OnClicked?.Invoke(_currentCard.card);
            }
        }

        /// <summary>
        /// Clears the current card display
        /// </summary>
        public void Clear()
        {
            _currentCard = null;
            _icon.sprite = null;
            _descriptionText.text = string.Empty;
        }
    }
}