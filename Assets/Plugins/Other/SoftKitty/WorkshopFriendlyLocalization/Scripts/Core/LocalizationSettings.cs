using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SoftKitty.WSFL
{
    public class LocalizationSettings : ScriptableObject
    {
        #region Variables
        public string DefaultLanguageName = "English";
        public string LocalizationRelativePath = "";
        public string BuildLocalizationRelativePath = "/LocalizationFiles";
        public List<string> LocalizationTextList = new List<string>();

        public const string CONFIG_NAME = "com.SoftKitty.Localization.settings";

        private static LocalizationSettings instance;
        #endregion

        #region Methods
        private void OnEnable()
        {
            if (instance == null) instance = this;
        }

        public static string GetLocalizationPath()
        {
            if (instance == null) Load();
#if UNITY_EDITOR
            return Application.dataPath + instance.LocalizationRelativePath;
#else
            return Application.dataPath + instance.BuildLocalizationRelativePath;
#endif
        }

        public static LocalizationSettings Load()
        {
            if (instance) return instance;
#if UNITY_EDITOR
            UnityEditor.EditorBuildSettings.TryGetConfigObject(CONFIG_NAME, out instance);
#else
        // Loads from the memory.
        instance = FindObjectOfType<LocalizationSettings>();
#endif
            return instance;
        }

        public void ApplyData()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
        #endregion
    }
}
