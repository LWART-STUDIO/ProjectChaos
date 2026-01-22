using TMPro;
using UnityEngine;

namespace Game.Scripts.Client.UI.Game.PlayerUI
{
    public class TimerUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _timerText;
        private float _startMinutes = 30f;

        private void Start()
        {
            UpdateTimer(0);
        }
        public void UpdateTimer(float time)
        {
            float startSeconds = _startMinutes * 60f;
            float uiTime = startSeconds - time;
            _timerText.text = FormatTime(uiTime);
        }
        private string FormatTime(float seconds)
        {
            bool isNegative = seconds < 0;
            seconds = Mathf.Abs(seconds);

            int min = Mathf.FloorToInt(seconds / 60f);
            int sec = Mathf.FloorToInt(seconds % 60f);

            return $"{(isNegative ? "-" : "")}{min:00}:{sec:00}";
        }
    }
}
