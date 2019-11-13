using UnityEngine;
using UnityEditor;

namespace AwesomeFolders
{
	/// <summary>
	/// Handle plugin preferences
	/// </summary>
	public class PreferencesUI : MonoBehaviour
	{
		private static bool preferencesLoaded = false;

		// Preferences
		public static bool folderIconEnabled = true;
		public static bool simpleClickEnabled = false;

		/// <summary>
		/// Called on unity preferences windows is showing
		/// </summary>
		[PreferenceItem("A. Folders")]
		public static void PreferencesGUI()
		{
			// Load the preferences
			if (!preferencesLoaded)
			{
				folderIconEnabled = EditorPrefs.GetBool("ext_" + ResourceUtil.ExtensionName + "_enabled", true);
				simpleClickEnabled = EditorPrefs.GetBool("ext_" + ResourceUtil.ExtensionName + "_simpleclick", false);
				preferencesLoaded = true;
			}

			// Preferences GUI
			folderIconEnabled = EditorGUILayout.Toggle("Folder icons enabled", folderIconEnabled);
			GUI.enabled = folderIconEnabled;
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			EditorGUILayout.LabelField("Preferences: ", EditorStyles.boldLabel);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Show style selector on folder select");
			simpleClickEnabled = EditorGUILayout.Toggle(simpleClickEnabled);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
			GUI.enabled = true;

			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			EditorGUILayout.LabelField("Danger zone: ", EditorStyles.boldLabel);
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Reset all folders"))
			{
				if (EditorUtility.DisplayDialog("Reset all folders?", "If you confirm all folders will use the default icon", "Yes", "No"))
				{
					ResourceUtil.ClearAllFolders();
				}
			}
			if (GUILayout.Button("Delete all styles"))
			{
				if (EditorUtility.DisplayDialog("Delete all styles?", "If you confirm all custom styles will be deleted", "Yes", "No"))
				{
					ResourceUtil.ClearAllFolders();
					ResourceUtil.DeleteAllStyles();
				}
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			// Save the preferences
			if (GUI.changed)
			{
				EditorPrefs.SetBool("ext_" + ResourceUtil.ExtensionName + "_enabled", folderIconEnabled);
				EditorPrefs.SetBool("ext_" + ResourceUtil.ExtensionName + "_simpleclick", simpleClickEnabled);
				CustomProjectView.RepaintProjectViews();
			}
		}

		public static void InitPreferences()
		{
			if (!EditorPrefs.HasKey("ext_" + ResourceUtil.ExtensionName + "_enabled"))
			{
				EditorPrefs.SetBool("ext_" + ResourceUtil.ExtensionName + "_enabled", true);
				EditorPrefs.SetBool("ext_" + ResourceUtil.ExtensionName + "_simpleclick", false);
			}
		}
	}
}
