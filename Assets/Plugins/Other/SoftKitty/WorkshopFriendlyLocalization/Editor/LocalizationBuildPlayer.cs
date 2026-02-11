using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace SoftKitty.WSFL
{
    public class LocalizationBuildPlayer : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var settings = LocalizationSettingsProvider.CurrentSettings;
            var settingsType = settings.GetType();
            var preloadedAssets = PlayerSettings.GetPreloadedAssets().ToList();

            preloadedAssets.RemoveAll(settings => settings.GetType() == settingsType);

            preloadedAssets.Add(settings);

            PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
        }
    }
}
