using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace SoftKitty.WSFL
{
    public class LocalizationTMP : MonoBehaviour
    {
        #region Variables
        private TMP_Text mText;
        private string _oriText = "";
        private string _translatedText = "";
        private int _language = -1;
        #endregion

        #region MonoBehaviour
        void Start()
        {
            if (GetComponent<TMP_Text>())
            {
                mText = GetComponent<TMP_Text>();
                _oriText = mText.text;
                _translatedText = mText.text;
            }
        }


        void Update()
        {
            if (mText == null) return;
            if ((mText.text != _oriText && mText.text != _translatedText) || _language != Localization.SelectedLanguage)
            {
                if(_language == Localization.SelectedLanguage) _oriText = mText.text;
                _language = Localization.SelectedLanguage;
                _translatedText = Localization.GetString(_oriText);
                mText.text = _translatedText;
            }
        }
        #endregion
    }
}
