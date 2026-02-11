using UnityEditor;
using UnityEngine;
using System.Collections;

namespace SoftKitty.WSFL
{
	public class LocalizationMenu : ScriptableWizard
	{

		[MenuItem("Tools/Localization Tool")]

		public static void CreateWizard()
		{
			EditorWindow _window = EditorWindow.GetWindowWithRect<LocalizationTool>(new Rect(Screen.width * 0.5F - 100F, Screen.height * 0.5F - 200F, 600F, 600F), false, "Localization Tool", true);
			_window.maxSize = new Vector2(700, 600);
			_window.minSize = new Vector2(500, 400);
			_window.Show();
		}

	}
}
