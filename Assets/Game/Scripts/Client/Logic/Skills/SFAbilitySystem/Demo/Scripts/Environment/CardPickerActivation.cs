using SFAbilitySystem.Demo.Player;
using UnityEngine;

namespace SFAbilitySystem.Demo.Cards
{
    /// <summary>
    /// Manages the activation state of the card selection UI and corresponding character control.
    /// Handles the toggle between gameplay and card selection modes.
    /// </summary>
    public class CardPickerActivation : MonoBehaviour
    {
        [Tooltip("Reference to the character controller to disable during card selection")]
        [SerializeField] private SimpleCharacterController _characterController;

        [Tooltip("The card selection UI GameObject to show/hide")]
        [SerializeField] private GameObject cardSelection;

        /// <summary>
        /// Toggles between card selection mode and normal gameplay mode
        /// </summary>
        /// <param name="active">
        /// True: Enables card selection UI and disables character control
        /// False: Disables card selection UI and enables character control
        /// </param>
        public void SetActive(bool active)
        {
            _characterController.SetCharacterControllerEnabled(!active);
            cardSelection.SetActive(active);
        }
    }
}