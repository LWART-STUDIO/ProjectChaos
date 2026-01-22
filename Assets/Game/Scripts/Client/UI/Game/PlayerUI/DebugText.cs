using System;
using Game.Scripts.Client.Logic.Game;
using TMPro;
using UnityEngine;

namespace Game.Scripts.Client.UI.Game.PlayerUI
{
    public class DebugText : MonoBehaviour
    {
        [SerializeField] private TMP_Text _debugText;

        private void Update()
        {
      
            _debugText.text = $"Урон: {GameStatisticCollector.PlayerTotalDamage}\n" +
                              $"Врагов убито: {GameStatisticCollector.EnemyWasKilled}\n" +
                              $"Реальное время {Mathf.RoundToInt(GameStatisticCollector.GameTime)}";
        }
    }
}
