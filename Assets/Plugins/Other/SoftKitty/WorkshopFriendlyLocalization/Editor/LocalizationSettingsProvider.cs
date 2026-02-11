using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SoftKitty.WSFL
{
    public class LocalizationSettingsProvider : AssetSettingsProvider
    {
        private string searchContext;
        private VisualElement rootElement;
        public static LocalizationSettings CurrentSettings
        {
            get
            {
                EditorBuildSettings.TryGetConfigObject(LocalizationSettings.CONFIG_NAME, out LocalizationSettings settings);
                return settings;
            }
            set
            {
                var remove = value == null;
                if (remove)
                {
                    EditorBuildSettings.RemoveConfigObject(LocalizationSettings.CONFIG_NAME);
                }
                else
                {
                    EditorBuildSettings.AddConfigObject(LocalizationSettings.CONFIG_NAME, value, overwrite: true);
                }
            }
        }

        public LocalizationSettingsProvider()
       : base("Project/Localization", () => CurrentSettings)
        {
            CurrentSettings = FindLocalizationSettings();
            keywords = GetSearchKeywordsFromGUIContentProperties<LocalizationSettings>();
        }

        private static LocalizationSettings FindLocalizationSettings()
        {
            var filter = $"t:{typeof(LocalizationSettings).Name}";
            var guids = AssetDatabase.FindAssets(filter);
            var hasGuids = guids.Length > 0;
            var path = hasGuids ? AssetDatabase.GUIDToAssetPath(guids[0]) : string.Empty;
            return AssetDatabase.LoadAssetAtPath<LocalizationSettings>(path);
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            this.rootElement = rootElement;
            this.searchContext = searchContext;
            base.OnActivate(searchContext, rootElement);
        }

        public override void OnGUI(string searchContext)
        {
            DrawCurrentSettingsGUI();
            EditorGUILayout.Space();

            var invalidSettings = CurrentSettings == null;
            if (invalidSettings) DisplaySettingsCreationGUI();
            else base.OnGUI(searchContext);
        }

        private void DrawCurrentSettingsGUI()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUI.indentLevel++;
            var settings = EditorGUILayout.ObjectField("Current Settings", CurrentSettings,
                typeof(LocalizationSettings), allowSceneObjects: false) as LocalizationSettings;
            if (settings) DrawCurrentSettingsMessage();
            EditorGUI.indentLevel--;

            var newSettings = EditorGUI.EndChangeCheck();
            if (newSettings)
            {
                CurrentSettings = settings;
                RefreshEditor();
            }
        }

        private void RefreshEditor() => base.OnActivate(searchContext, rootElement);

        private void DisplaySettingsCreationGUI()
        {
            const string message = "You have no Localization Settings. Would you like to create one?";
            EditorGUILayout.HelpBox(message, MessageType.Info, wide: true);
            var openCreationdialog = GUILayout.Button("Create");
            if (openCreationdialog)
            {
                CurrentSettings = SaveLocalizationAsset();
            }
        }

        private void DrawCurrentSettingsMessage()
        {
            const string message = "This is the current Localization Settings and " +
                "it will be automatically included into any builds.";
            EditorGUILayout.HelpBox(message, MessageType.Info, wide: true);
        }


        private static LocalizationSettings SaveLocalizationAsset()
        {
            var path = EditorUtility.SaveFilePanelInProject(
                title: "Save Localization Settings", defaultName: "DefaultLocalizationSettings", extension: "asset",
                message: "Please enter a filename to save the projects Localization settings.");
            var invalidPath = string.IsNullOrEmpty(path);
            if (invalidPath) return null;

            var settings = ScriptableObject.CreateInstance<LocalizationSettings>();
            AssetDatabase.CreateAsset(settings, path);
            AssetDatabase.SaveAssets();

            Selection.activeObject = settings;
            return settings;
        }

        [SettingsProvider]
        private static SettingsProvider CreateProjectSettingsMenu() => new LocalizationSettingsProvider();

    }
}
