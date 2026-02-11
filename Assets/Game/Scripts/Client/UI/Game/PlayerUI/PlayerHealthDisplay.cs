using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Client.UI.Game.PlayerUI
{
   public class PlayerHealthDisplay : MonoBehaviour
   {
      [SerializeField] private Image _healthBar;
      [SerializeField] private TMP_Text _healthTextCurrent;
      [SerializeField] private TMP_Text _healthTextMax;
      [SerializeField] private Image _energyBar;
      [SerializeField] private GameObject _energyBarHolder;
      [SerializeField] private TMP_Text _energyTextCurrent;
      [SerializeField] private TMP_Text _energyTextMax;
      
      public void UpdateHealth(int current, int max)
      {
         _healthBar.fillAmount = (float)current / max;
         _healthTextCurrent.text = current.ToString();
         _healthTextMax.text = max.ToString();
      }

      public void UpdateEnergyShield(int current, int max)
      {
         if (max <= 0)
         {
            _energyBarHolder.SetActive(false);
            return;
         }
         _energyBar.fillAmount = (float)current / max;
         _energyTextCurrent.text = current.ToString();
         _energyTextMax.text = max.ToString();
      }
   }
}
