using UnityEngine;
using UnityEditor;

namespace AwesomeFolders
{
	public class GUIHelper
	{
		public static GUIContent ContentFromAssets(string text, string texturePath, string tooltip = "")
		{
			return new GUIContent(text, LoadTexture(texturePath), tooltip);
		}

		public static GUIContent ContentFromEditor(string text, string iconName, string tooltip = "")
		{
			GUIContent newContent = EditorGUIUtility.IconContent(iconName);
			newContent.text = text;
			newContent.tooltip = tooltip;

			return newContent;
		}

		public static Texture LoadTexture(string texturePath)
		{
			return AssetDatabase.LoadAssetAtPath<Texture>(texturePath);
		}
	}
}
