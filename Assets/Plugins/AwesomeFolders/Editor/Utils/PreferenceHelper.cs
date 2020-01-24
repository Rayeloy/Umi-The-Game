using UnityEngine;
using UnityEditor;
using System;

namespace AwesomeFolders
{
	public class PreferenceHelper
	{
		public static void SetInt(string prefKey, int value)
		{
			EditorPrefs.SetInt(GetPrefKey(prefKey), value);
		}

		public static void SetFloat(string prefKey, float value)
		{
			EditorPrefs.SetFloat(GetPrefKey(prefKey), value);
		}

		public static void SetBool(string prefKey, bool value)
		{
			EditorPrefs.SetBool(GetPrefKey(prefKey), value);
		}

		public static void SetString(string prefKey, string value)
		{
			EditorPrefs.SetString(GetPrefKey(prefKey), value);
		}

		public static int GetInt(string prefKey, int defaultValue = 0)
		{
			return EditorPrefs.GetInt(GetPrefKey(prefKey), defaultValue);
		}

		public static float GetFloat(string prefKey, float defaultValue = 0.0f)
		{
			return EditorPrefs.GetFloat(GetPrefKey(prefKey), defaultValue);
		}

		public static bool GetBool(string prefKey, bool defaultValue = false)
		{
			return EditorPrefs.GetBool(GetPrefKey(prefKey), defaultValue);
		}

		public static string GetString(string prefKey, string defaultValue = "")
		{
			return EditorPrefs.GetString(GetPrefKey(prefKey), defaultValue);
		}

		public static bool HasKey(string prefKey)
		{
			return EditorPrefs.HasKey(GetPrefKey(prefKey));
		}

		private static string GetPrefKey(string prefKey)
		{
			return string.Concat("ext_", ResourceUtil.ExtensionName, "_", prefKey);
		}
	}
}
