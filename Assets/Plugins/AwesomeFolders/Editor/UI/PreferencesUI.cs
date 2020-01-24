using UnityEngine;
using UnityEditor;
using System.IO;

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
		public static Settings settings;

		// Projects
		public class Settings
		{
			public bool converted = false;
			public bool useNewUI = false;

			public static Settings Read()
			{
				string path = ResourceUtil.ExtensionPath + "/settings.json";
				if (!File.Exists(path))
				{
					return new Settings();
				}

				StreamReader reader = new StreamReader(path);
				string json = reader.ReadToEnd();
				reader.Close();

				return JsonUtility.FromJson<Settings>(json);
			}

			public void Write()
			{
				string path = ResourceUtil.ExtensionPath + "/settings.json";
				StreamWriter writer = new StreamWriter(path, false);
				writer.Write(JsonUtility.ToJson(this));
				writer.Close();
			}
		}

#pragma warning disable 0618
		[PreferenceItem("A. Folders")]
#pragma warning restore 0618
		public static void PreferencesGUI()
		{
			// Load the preferences
			if (!preferencesLoaded)
			{
				UpdatePreferences();
				preferencesLoaded = true;
			}

			// Preferences GUI
			folderIconEnabled = EditorGUILayout.Toggle("Folder icons enabled", folderIconEnabled);
			GUI.enabled = folderIconEnabled;
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			EditorGUILayout.LabelField("User Preferences: ", EditorStyles.boldLabel);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Show style selector on folder select");
			simpleClickEnabled = EditorGUILayout.Toggle(simpleClickEnabled);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			EditorGUILayout.LabelField("Project Settings: ", EditorStyles.boldLabel);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Use Unity new icons");
			bool newUILocal = EditorGUILayout.Toggle(settings.useNewUI);
			if (settings.useNewUI != newUILocal)
			{
				settings.useNewUI = newUILocal;
				StyleConverter.ConvertStyles();
				settings.Write();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

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

			GUI.enabled = true;

			// Save the preferences
			if (GUI.changed)
			{
				PreferenceHelper.SetBool("enabled", folderIconEnabled);
				PreferenceHelper.SetBool("simpleclick", simpleClickEnabled);
				CustomProjectView.RepaintProjectViews();
			}
		}

		[InitializeOnLoadMethod]
		private static void UpdatePreferences()
		{
			folderIconEnabled = PreferenceHelper.GetBool("enabled", true);
			simpleClickEnabled = PreferenceHelper.GetBool("simpleclick", false);

			settings = Settings.Read();
		}

		public static void AutoConvertAssets()
		{
			if (!settings.converted)
			{
				settings.useNewUI = ShouldUseNewUI();
				ResourceUtil.Refresh();
				StyleConverter.ConvertStyles();
				settings.converted = true;
				settings.Write();
			}
		}

		public static void InitPreferences()
		{
			if (!PreferenceHelper.HasKey("enabled"))
			{
				PreferenceHelper.SetBool("enabled", true);
				PreferenceHelper.SetBool("simpleclick", false);
				settings = Settings.Read();
			}
		}

		private static bool ShouldUseNewUI()
		{
			return EditorGUIUtility.FindTexture("Folder Icon").width > 64;
		}
	}
}
