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
        [SerializeField] private TMP_Text _healthText;
        [SerializeField] private CompassPro _compassPro;
        private UIService _uiService => Service<UIService>.Instance;

        public void UpdateHealth(int health)
        {
            _healthText.text = health.ToString();
        }

        public void SetPlayerCompas(Transform playerTransform)
        {
            _compassPro.cameraMain = playerTransform.GetComponentInChildren<Camera>();
            _compassPro.follow = playerTransform;

        }

      
    }
}
