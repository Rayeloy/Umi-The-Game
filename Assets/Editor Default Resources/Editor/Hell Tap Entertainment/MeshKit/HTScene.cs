////////////////////////////////////////////////////////////////////////////////////////////////
//
//  HTScene.cs
//
//	Editor Wrapper for the old EditorApplication API functions
//
//	© 2015 Melli Georgiou.
//	Hell Tap Entertainment LTD
//
////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System.Collections;
using HellTap.MeshKit;

// Use HellTap Namespace
namespace HellTap.MeshKit {

	// Class
	static class HTScene{

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	GET CURRENT SCENE
		//	NOTE: This is a relative file path like: "Assets/MyScenes/MyScene.unity".
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static string CurrentScene(){
			// BEFORE UNITY 5.3
			#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2

				return EditorApplication.currentScene; 

			// UNITY 5.3 AND UP
			#else

				return UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path ;
			
			#endif
		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	SAVE SCENE
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static bool SaveScene( string path, bool saveAsCopy ){

			// BEFORE UNITY 5.3
			#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2

				return EditorApplication.SaveScene(path, saveAsCopy); 

			// UNITY 5.3 AND UP
			#else

				return UnityEditor.SceneManagement.EditorSceneManager.SaveScene( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene(), path, saveAsCopy);
			
			#endif

		}

		// No argument version
		public static bool SaveScene(){

			// BEFORE UNITY 5.3
			#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2

				return EditorApplication.SaveScene(); 

			// UNITY 5.3 AND UP
			#else

				return UnityEditor.SceneManagement.EditorSceneManager.SaveScene( 
					UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene(), 
					UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path, 
					false);
			
			#endif

		}
		
		// Mark the current scene as dirty so Unity knows to save it to disk!
		public static void MarkSceneAsDirty(){

			// BEFORE UNITY 5.3
			#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2

				// Marks the scene as dirty in older versions of Unity (untested)				
				EditorApplication.MarkSceneDirty();

			// UNITY 5.3 AND UP
			#else

				// Marks the scene as dirty
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() );
			
			#endif

		}

	}
}


