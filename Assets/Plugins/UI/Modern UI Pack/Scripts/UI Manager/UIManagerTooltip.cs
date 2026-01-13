using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.MUIP
{
    [ExecuteInEditMode]
    public class UIManagerTooltip : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private UIManager UIManagerAsset;

        [Header("Resources")]
        [SerializeField] private Image background;
        [SerializeField] private TextMeshProUGUI text;
        private Coroutine _textCollector=null;

        void Awake()
        {
            if (UIManagerAsset == null) { UIManagerAsset = Resources.Load<UIManager>("MUIP Manager"); }

            this.enabled = true;

            if (UIManagerAsset.enableDynamicUpdate == false)
            {
                UpdateTooltip();
                this.enabled = false;
            }
            _textCollector=null;
        }

        void Update()
        {
            if (UIManagerAsset == null) { return; }
            if (UIManagerAsset.enableDynamicUpdate == true) { UpdateTooltip(); }
        }
        public void CollectDescriptionText(string text)
        {
            if (_textCollector == null)
            {
                this.text.text = text;
                _textCollector=StartCoroutine(CollectTextTimer());
                return;
            }
            //this.text.text += $"\n{text}";

        }

        private IEnumerator CollectTextTimer()
        {
            yield return new WaitForSecondsRealtime(0.1f);
            _textCollector = null;

        }

        void UpdateTooltip()
        {
            background.color = UIManagerAsset.tooltipBackgroundColor;
            text.color = UIManagerAsset.tooltipTextColor;
            text.font = UIManagerAsset.tooltipFont;
            text.fontSize = UIManagerAsset.tooltipFontSize;
        }
    }
}