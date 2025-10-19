using SFAbilitySystem.Core;
using SFAbilitySystem.Demo.Cards;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SFAbilitySystem.Demo.Player
{
    /// <summary>
    /// Manages player health points (HP) including:
    /// - Current and maximum HP tracking
    /// - HP UI display (text and slider)
    /// - HP modifications from ability cards
    /// </summary>
    public class PlayerHP : MonoBehaviour, ICardsPoolUpdated
    {
        [Header("UI References")]
        [Tooltip("Text element displaying current HP")]
        [SerializeField] private TextMeshProUGUI _hpText;

        [Tooltip("Slider visualizing current HP percentage")]
        [SerializeField] private Slider _hpSlider;

        [Header("Health Settings")]
        [Tooltip("Initial maximum HP value")]
        [SerializeField] private float _startingHP = 100;

        [Tooltip("Current HP value")]
        [SerializeField] private float _currentHP = 100;

        [Header("Dependencies")]
        [Tooltip("Reference to the card system for ability updates")]
        [SerializeField] private CardSystem _cardSystem;

        private MoreHP _currentMoreHp;

        /// <summary>
        /// Registers for card updates when enabled
        /// </summary>
        private void OnEnable()
        {
            _cardSystem.abilityManager.AddCardsPoolUpdatedCallback(this);
        }

        /// <summary>
        /// Unregisters from card updates when disabled
        /// </summary>
        private void OnDisable()
        {
            _cardSystem.abilityManager.RemoveCardsPoolUpdatedCallback(this);
        }

        /// <summary>
        /// Handles health modifications when new cards are acquired
        /// </summary>
        /// <param name="abilityManager">Source of the card update</param>
        public void OnCardsPoolUpdated(AbilityManager abilityManager)
        {
            if (abilityManager.TryGetCard(out MoreHP moreHP) && _currentMoreHp != moreHP)
            {
                _currentMoreHp = moreHP;

                // Maintain health percentage when max HP changes
                float healthRatio = (_startingHP > 0) ? _currentHP / _startingHP : 1f;
                _startingHP += moreHP.hpBoost;
                _currentHP = Mathf.Clamp(_startingHP * healthRatio, 0, _startingHP);

                UpdateUI();
            }
        }

        /// <summary>
        /// Initializes health values and UI
        /// </summary>
        private void Awake()
        {
            _currentHP = _startingHP;
            UpdateUI();
        }

        /// <summary>
        /// Updates all health UI elements with current values
        /// </summary>
        private void UpdateUI()
        {
            _hpSlider.maxValue = _startingHP;
            _hpSlider.value = _currentHP;
            _hpText.text = Mathf.FloorToInt(_currentHP).ToString();
        }
    }
}