using SFAbilitySystem.Demo.Cards;
using UnityEngine;

namespace SFAbilitySystem.Demo.Environment
{
    /// <summary>
    /// Triggers card selection UI activation when player enters the trigger zone.
    /// Attach to GameObject with Collider set to trigger mode.
    /// </summary>
    public class ActivateCardsSelectionOnEnter : MonoBehaviour
    {
        /// <summary>
        /// Automatically activates card picker UI when player enters trigger zone
        /// </summary>
        /// <param name="other">The collider entering the trigger zone</param>
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out CardPickerActivation cardPickerActivation))
            {
                cardPickerActivation.SetActive(true);
            }
        }
    }
}