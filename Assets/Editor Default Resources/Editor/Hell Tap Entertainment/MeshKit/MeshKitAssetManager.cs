////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MeshKitAssetManager.cs
//
//	Triggers certain functions even without the MeshKit GUI Window being open.
//
//	© 2015 - 2018 Melli Georgiou.
//	Hell Tap Entertainment LTD
//
////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System.Collections;

// Use HellTap Namespace
namespace HellTap.MeshKit {
	
	// Make sure this is limited to the Unity Editor
	#if UNITY_EDITOR

		// Class
		[InitializeOnLoad]
		public static class MeshKitAssetManager {

			// Variables to track
			static string currentScene = "";						// Track if the current scene has changed

			////////////////////////////////////////////////////////////////////////////////////////////////
			//	MESHKIT TRACKER
			//	Initializes the tracker to run in the background of the Unity Editor
			////////////////////////////////////////////////////////////////////////////////////////////////

			// Initializer
			static MeshKitAssetManager(){
				Debug.Log("MESHKIT: Asset Manager Started.");
				#if UNITY_2018_1_OR_NEWER
					EditorApplication.hierarchyChanged += HierarchyWindowChanged;
				#else
					EditorApplication.hierarchyWindowChanged += HierarchyWindowChanged;
				#endif
			}

			////////////////////////////////////////////////////////////////////////////////////////////////
			//	HIERARCHY WINDOW CHANGED
			//	Tracks if there has been a change in the Hierarchy of the Unity Window.
			//	Called when a GameObject is created, renamed, parented, unparented or destroyed 
			//	and also when a new scene is loaded.
			////////////////////////////////////////////////////////////////////////////////////////////////

			static void HierarchyWindowChanged(){

				// Never do anything if Unity is compiling or the game is playing or about to play
				if( EditorApplication.isCompiling == false && EditorApplication.isPlayingOrWillChangePlaymode == false ){

					// ===================================
		    		//	CHECK IF A NEW SCENE WAS LOADED
		    		// ===================================

			    	// Check To See if the Scene was changed, and then update MeshAssets
			    	if( MeshKitAssetManager.currentScene != HTScene.CurrentScene() ){
						MeshKitAssetManager.currentScene = HTScene.CurrentScene();
						MeshAssets.SceneWasLoadedInEditor();
			    	}

			    	// ===================================
		    		//	LIVE ASSET TRACKING
		    		// ===================================

		    		// Make sure this is NOT a backed up scene
					if( MeshAssets.IsThisABackedUpMeshKitScene() == false ){

			    		// Fix Any Prefabs That are not global
						if( EditorPrefs.GetBool ("MeshKitLivePrefabTracking", false) ){
							MeshAssets.KeepPrefabsGlobal();
						}
						
						// Fix Any meshes that belong to a different scene
						if( EditorPrefs.GetBool ("MeshKitLiveMeshTracking", false) ){
							MeshAssets.KeepMeshKitFilesLocal();
						}

					}
				}
			}
		}

	#endif

}