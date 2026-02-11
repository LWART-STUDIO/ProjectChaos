using System.Collections;
using CompassNavigatorPro;
using Game.Scripts.Services.UI;
using Sisus.Init;
using TMPro;
using UnityEngine;

namespace Game.Scripts.Client.UI.Game.PlayerUI
{
    public class PlayerInGameUI : MonoBehaviour
    {

        [SerializeField] private PlayerHealthDisplay _playerHealthDisplay;
        [SerializeField] private CompassPro _compassPro;
        [SerializeField] private TimerUI _timer;
        [SerializeField] private TMP_Text _currentLevelText;
        private UIService _uiService => Service<UIService>.Instance;

        public void UpdateTimer(float time)
        {
            _timer.UpdateTimer(time);
        }
        public void UpdateHealth(int current, int max)
        {
            _playerHealthDisplay.UpdateHealth(current, max);
        }
        public void UpdateEnergyShield(int current, int max)
        {
            _playerHealthDisplay.UpdateEnergyShield(current, max);
        }

        public void UpdateCurrentLevel(int current)
        {
            _currentLevelText.text = current.ToString();
        }

        public void SetPlayerCompas(Transform playerTransform)
        {
            _compassPro.cameraMain = playerTransform.GetComponentInChildren<Camera>();
            _compassPro.follow = playerTransform;

        }

      
    }
}
