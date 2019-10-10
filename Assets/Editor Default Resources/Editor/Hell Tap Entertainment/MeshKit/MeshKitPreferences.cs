////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MeshKitPreferences.cs
//
//	Unity Preferences For MeshKit.
//
//	© 2015 Melli Georgiou.
//	Hell Tap Entertainment LTD
//
////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System.Collections;

// Use HellTap Namespace
namespace HellTap.MeshKit {

	// Class
	public class MeshKitPreferences {

		// Have we loaded the prefs yet
		private static bool prefsLoaded = false;
		
		// The Preferences
		public static bool useSmallIcons = true;
		public static bool livePrefabTracking = false;
		public static bool liveMeshTracking = false;
		public static bool automaticSceneBackups = false;
		public static bool verboseMode = false;
		
		// Add preferences section named "My Preferences" to the Preferences Window
		[PreferenceItem ("MeshKit")]
		static void PreferencesGUI () {
			
			// =====================
			//	LOAD PREFERENCES
			// =====================

			if (!prefsLoaded) {
				useSmallIcons = EditorPrefs.GetBool ("MeshKitUseSmallIcons", true);
				livePrefabTracking = EditorPrefs.GetBool ("MeshKitLivePrefabTracking", false);
				liveMeshTracking = EditorPrefs.GetBool ("MeshKitLiveMeshTracking", false);
				automaticSceneBackups = EditorPrefs.GetBool ("MeshKitAutomaticallyBackupScenes", false);
				verboseMode = EditorPrefs.GetBool ("MeshKitVerboseMode", false);
				prefsLoaded = true;
			}
			
			// ==================
			//	PREFERENCES GUI
			// ==================

			// Initial Space
			GUILayout.Space(8);

			// Live Asset Tracking
			GUILayout.Label("Asset Management", "BoldLabel");
			GUILayout.Label("MeshKit automatically scans (and can fix) problematic meshes\nand prefabs every time you load a new scene. Alternatively, this\ncan be performed in realtime as you are working in the Editor.");
			GUILayout.Space(4);
			livePrefabTracking = EditorGUILayout.Toggle ("Realtime Prefab Checks", livePrefabTracking);
			liveMeshTracking = EditorGUILayout.Toggle ("Realtime Mesh Checks", liveMeshTracking);
			automaticSceneBackups = EditorGUILayout.Toggle ("Backup Scenes Without Asking", automaticSceneBackups);
			GUILayout.Space(16);

			/*
			// Automatic Scene Backups
			GUILayout.Label("Automatic Scene Backups", "BoldLabel");
			GUILayout.Label("Before MeshKit applies its first change to a scene in the editor,\nit will be automatically backed up without any prompts.");
			GUILayout.Space(4);
			automaticSceneBackups = EditorGUILayout.Toggle ("Backup Scenes Without Asking", automaticSceneBackups);
			GUILayout.Space(16);
			*/

			// GUI Settings
			GUILayout.Label("MeshKit GUI Settings", "BoldLabel");
			GUILayout.Label("Display settings for the MeshKit Window.");
			GUILayout.Space(4);
			useSmallIcons = EditorGUILayout.Toggle ("Use Small Icons", useSmallIcons);
			GUILayout.Space(16);

			// Verbose Mode
			GUILayout.Label("Verbose Mode", "BoldLabel");
			GUILayout.Label("Shows verbose console messages for MeshKit in the Editor.");
			GUILayout.Space(4);
			verboseMode = EditorGUILayout.Toggle ("Enable Verbose Mode", verboseMode);
			GUILayout.Space(16);
			
			// =====================
			//	SAVE CHANGES
			// =====================

			if (GUI.changed){
				EditorPrefs.SetBool ("MeshKitUseSmallIcons", useSmallIcons );
				EditorPrefs.SetBool ("MeshKitLivePrefabTracking", livePrefabTracking);
				EditorPrefs.SetBool ("MeshKitLiveMeshTracking", liveMeshTracking);
				EditorPrefs.SetBool ("MeshKitAutomaticallyBackupScenes", automaticSceneBackups);
				EditorPrefs.SetBool ("MeshKitVerboseMode", verboseMode);
			}
		}
	}
}