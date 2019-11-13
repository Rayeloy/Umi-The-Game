using UnityEngine;
using UnityEditor;
using System.IO;

namespace AwesomeFolders
{
	/// <summary>
	/// Contains utils methods to quickly access plugin paths
	/// Handle plugin being moved elsewhere
	/// </summary>
	public class ResourceUtil : ScriptableObject
	{
		public const string ExtensionName = "Awesome Folders";
		public static ResourceUtil Instance { get; private set; }
		public static string ExtensionPath { get; private set; }
		public static string TexturesPath { get; private set; }
		public static string CustomStylesPath { get; private set; }
		public static string StyleIconsPath { get; private set; }

		// Refresh paths if needed 
		public static void Refresh()
		{
			if(Instance == null)
			{
				OnScriptsReloaded();
			}
		}

		[UnityEditor.Callbacks.DidReloadScripts]
		private static void OnScriptsReloaded()
		{
			// On Script reload fetch this script path
			if(Instance != null)
			{
				DestroyImmediate(Instance);
			}
			Instance = CreateInstance<ResourceUtil>();

			MonoScript ms = MonoScript.FromScriptableObject(Instance);
			FileInfo fi = new FileInfo(AssetDatabase.GetAssetPath(ms));
			ExtensionPath = fi.Directory.Parent.ToString().Replace('\\', '/');

			// Convert to relative path
			if(ExtensionPath.StartsWith(Application.dataPath))
			{
				ExtensionPath = "Assets" + ExtensionPath.Substring(Application.dataPath.Length);
			}
			else
			{
				Debug.LogError(ExtensionName + ": Extension path is wrong: " + ExtensionPath);
			}

			// Set other path
			TexturesPath = ExtensionPath + "/Textures";
			CustomStylesPath = TexturesPath + "/Styles/Custom";
			StyleIconsPath = TexturesPath + "/Styles/Icons";
		}

		/// <summary>
		/// Reset all folders to their original state
		/// </summary>
		public static void ClearAllFolders()
		{
			// For each folder
			string[] files = AssetDatabase.FindAssets("t:defaultasset", new string[] { "Assets" });
			for (int i = 0; i < files.Length; i++)
			{
				string path = AssetDatabase.GUIDToAssetPath(files[i]);

				if (AssetDatabase.IsValidFolder(path))
				{
					AssetImporter folderImporter = AssetImporter.GetAtPath(path);
					string[] guids = folderImporter.userData.Split(';');
					if (guids.Length == 2)
					{
						folderImporter.userData = "";
						folderImporter.SaveAndReimport();
					}
				}
			}

			CustomProjectView.RepaintProjectViews();
		}

		/// <summary>
		/// Delete all user created styles
		/// </summary>
		public static void DeleteAllStyles()
		{
			ClearAllFolders();

			string[] files = AssetDatabase.FindAssets("t:texture2D", new string[] { CustomStylesPath });
			for (int i = 0; i < files.Length; i++)
			{
				string texPath = AssetDatabase.GUIDToAssetPath(files[i]);
				Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
				if(texture.width == 64 || texture.width == 16)
				{
					AssetDatabase.DeleteAsset(texPath);
				}
			}
			AssetDatabase.Refresh();
			Selection.activeObject = null;
		}
	}
}
