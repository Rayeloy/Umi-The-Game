////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MeshAssets.cs
//
//	Handles saving assets and all MeshKit asset management.
//
//	© 2015 - 2019 Melli Georgiou.
//	Hell Tap Entertainment LTD
//
////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.IO;
using HellTap.MeshKit;

// Use HellTap Namespace
namespace HellTap.MeshKit {

	// Class
	public class MeshAssets : EditorWindow {

		// Important Folder Directories
		static public string seperatedMeshFolder = "";			// Where we save SubMeshes - has a "/" at the end
		static public string combinedMeshFolder = "";			// Where we save Combined Meshes - has a "/" at the end
		static public string invertedMeshFolder = "";			// Where we save Inverted Meshes - has a "/" at the end
		static public string doubleSidedMeshFolder = "";		// Where we save Double-Sided Meshes - has a "/" at the end
		static public string rebuiltMeshFolder = "";			// Where we save Rebuilt Meshes - has a "/" at the end
		static public string decimatedMeshFolder = "";			// Where we save Decimated Meshes - has a "/" at the end
		static public string lodMeshFolder = "";				// Where we save AutoLOD Meshes - has a "/" at the end
		static public string backupSceneFolder = "";			// Backup of the scene before using Mesh Tools

		// Filename Helpers
		static public string sceneName = "";					// The name of the Scene without ".unity" on the end
		static public string sceneParentFolder = "";			// The filepath to the parent folder of the Scene - eg "Assets/Path/To/"
		static public string prefix = ""; 						// The absolute OS prefix of the filename - eg "Users/Me/Desktop/UnityProject/""

		// Progress For Asset Cleaning
		public static float progress = 0;
	    public static float maxProgress = 0;

	    // Error Handling
	    public static bool errorCreatingAsset = false;			// Was there an error creating the last asset?

		// Options
		static bool updateSupportFoldersAndResources = true;	// If this is true, it forces HaveSupportFoldersBeenCreated function.


		////////////////////////////////////////////////////////////////////////////////////////////////
		//	SCENE WAS LOADED IN EDITOR
		//	Updates Support Folders and resources when the editor changes scenes.
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static void SceneWasLoadedInEditor(){

			// Reset variables
			if(MeshKitGUI.verbose){ Debug.Log("MeshAssets: updateSupportFoldersAndResources = true"); }
			updateSupportFoldersAndResources = true;
			errorCreatingAsset = false;

			// Is this a NORMAL scene and not a back up?
			if( IsThisABackedUpMeshKitScene() == false ){

				// Fix Any Prefabs That are not global
				KeepPrefabsGlobal();

				// Make sure all MeshKit Files in this scene are in the local MeshKit folder.
				KeepMeshKitFilesLocal();
			
			// Show Prompt about Backup Scenes
			} else {
				if(MeshKitGUI.verbose){ Debug.Log("MESHKIT: This is a backed up scene!"); }
				EditorUtility.DisplayDialog("Backup Scene Detected", "MeshKit has detected that this scene is actually the backup of another scene. If you want to restore this scene you should duplicate it and move it out of the \"MeshKit/Backup Scene\" folder that it is residing in.\n\nIn the meantime, MeshKit has disabled editor operations for this scene to help prevent any accidental dataloss. ", "Okay");
			}
		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	IS THIS A BACKED UP MESHKIT SCENE?
		//	Checks to see if this Scene is a Backed Up Scene. Defined if its parent folder is "Backup 
		//	Scene" and that parent folder is "MeshKit"
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static bool IsThisABackedUpMeshKitScene(){

			// Make sure the scene name isn't blank
			if( HTScene.CurrentScene() != "" ){

				// Get the directory of the asset and split it into sections
				string[] directorySections = Path.GetDirectoryName( HTScene.CurrentScene() ).Split('/');

				// Make sure there are at least 3 levels of directories
				if( directorySections.Length >= 3 ){

					// Make sure the 1st parent is "Backup Scene" and the 2nd parent is "MeshKit"
					if( directorySections[directorySections.Length-1] == "Backup Scene" &&
						directorySections[directorySections.Length-2] == "MeshKit"
					){
						// Return true if this checks out!
						return true;
					}
				}
			}

			// Return false by default
			return false;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	HAVE SUPPORT FOLDERS BEEN CREATED?
		//	Checks if the support folders have been created, if not, it creates them dynamically.
		//	If the scene hasn't been saved, it returns false - if all goes well it returns true.
		//	NOTE: This function also sets MeshSplitter.subMeshFolder so we know where to save.
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static bool HaveSupportFoldersBeenCreated(){

			// If ths is a saved scene and everything has already been checked, return true!
			if( HTScene.CurrentScene() != "" && updateSupportFoldersAndResources == false ){
				
				return true;

			// Make sure this is a saved scene.
			} else if( HTScene.CurrentScene() != "" && updateSupportFoldersAndResources == true ){

				if(MeshKitGUI.verbose){ Debug.Log("currentScene: "+HTScene.CurrentScene() ); }											
				sceneName = Path.GetFileNameWithoutExtension( HTScene.CurrentScene() );	// eg MyScene
				if(MeshKitGUI.verbose){ Debug.Log("sceneName: "+sceneName); }
				sceneParentFolder = HTScene.CurrentScene().Replace(Path.GetFileName( HTScene.CurrentScene() ), "");	// Path/To/
				if(MeshKitGUI.verbose){ Debug.Log("sceneParentFolder: "+sceneParentFolder); }

				prefix = Application.dataPath.Replace("/Assets","")+"/";
				if(MeshKitGUI.verbose){ Debug.Log("prefix: "+prefix); }

				// Create Support Folders
				if( Directory.Exists(prefix+sceneParentFolder)  ){

					// If We do not have the maing support Folder created (same name as scene), Create it now ...
					if(MeshKitGUI.verbose){ Debug.Log(prefix+sceneParentFolder+sceneName+"/"); }
					if( !Directory.Exists(prefix+sceneParentFolder+sceneName+"/")  ){
						Directory.CreateDirectory(prefix+sceneParentFolder+sceneName+"/");
					}

					// If We do not have the support Folder created (MeshKit), Create it now ...
					if(MeshKitGUI.verbose){ Debug.Log(prefix+sceneParentFolder+sceneName+"/MeshKit/"); }
					if( !Directory.Exists(prefix+sceneParentFolder+sceneName+"/MeshKit/")  ){
						Directory.CreateDirectory(prefix+sceneParentFolder+sceneName+"/MeshKit/");
					}
					
					// If we don't have the Seperated Meshes Directory, create it now.
					if( !Directory.Exists(prefix+sceneParentFolder + sceneName + "/MeshKit/Seperated Meshes")  ){
						Directory.CreateDirectory(prefix+sceneParentFolder + sceneName + "/MeshKit/Seperated Meshes");
					}

					// If we don't have the Combined Meshes Directory, create it now.
					if( !Directory.Exists(prefix+sceneParentFolder + sceneName + "/MeshKit/Combined Meshes")  ){
						Directory.CreateDirectory(prefix+sceneParentFolder + sceneName + "/MeshKit/Combined Meshes");
					}

					// If we don't have the Inverted Meshes Directory, create it now.
					if( !Directory.Exists(prefix+sceneParentFolder + sceneName + "/MeshKit/Inverted Meshes")  ){
						Directory.CreateDirectory(prefix+sceneParentFolder + sceneName + "/MeshKit/Inverted Meshes");
					}

					// If we don't have the Double-Sided Meshes Directory, create it now.
					if( !Directory.Exists(prefix+sceneParentFolder + sceneName + "/MeshKit/DoubleSided Meshes")  ){
						Directory.CreateDirectory(prefix+sceneParentFolder + sceneName + "/MeshKit/DoubleSided Meshes");
					}

					// If we don't have the Rebuilt Meshes Directory, create it now.
					if( !Directory.Exists(prefix+sceneParentFolder + sceneName + "/MeshKit/Rebuilt Meshes")  ){
						Directory.CreateDirectory(prefix+sceneParentFolder + sceneName + "/MeshKit/Rebuilt Meshes");
					}


					// If we don't have the Decimated Meshes Directory, create it now.
					if( !Directory.Exists(prefix+sceneParentFolder + sceneName + "/MeshKit/Decimated Meshes")  ){
						Directory.CreateDirectory(prefix+sceneParentFolder + sceneName + "/MeshKit/Decimated Meshes");
					}

					// If we don't have the LOD Meshes Directory, create it now.
					if( !Directory.Exists(prefix+sceneParentFolder + sceneName + "/MeshKit/LOD Meshes")  ){
						Directory.CreateDirectory(prefix+sceneParentFolder + sceneName + "/MeshKit/LOD Meshes");
					}

					// If we don't have the Scene Backup Directory, create it now.
					if( !Directory.Exists(prefix+sceneParentFolder + sceneName + "/MeshKit/Backup Scene")  ){
						Directory.CreateDirectory(prefix+sceneParentFolder + sceneName + "/MeshKit/Backup Scene");
					}

					// Make A Backup of the unity scene if it doesn't exist ...
					if( Directory.Exists(prefix+sceneParentFolder + sceneName + "/MeshKit/Backup Scene") &&
						!File.Exists(prefix+sceneParentFolder + sceneName + "/MeshKit/Backup Scene/"+sceneName+"_MeshKitBackup.unity") 
					){
						// Ask user if they want a backup of their scene - or do it automatically if we've setup automatic backups
						if( EditorPrefs.GetBool ("MeshKitAutomaticallyBackupScenes", false) || EditorUtility.DisplayDialog("Backup Scene", "Before performing this operation, would you like MeshKit to automatically create a backup copy of this Unity scene?", "Yes", "No")
						){
							Debug.Log("MESHKIT: Backing Up Unity Scene to:  "+sceneParentFolder + sceneName + "/MeshKit/Backup Scene/"+sceneName+"_MeshKitBackup.unity");
						
							// Save A Copy of the current scene to the new location without affecting the current scene
							HTScene.SaveScene(sceneParentFolder + sceneName + "/MeshKit/Backup Scene/"+sceneName+"_MeshKitBackup.unity", true);
							AssetDatabase.Refresh();

						}
					}

					// Setup the seperatedMeshFolder and others so we know where to create our new meshes ...
					MeshAssets.backupSceneFolder = sceneParentFolder + sceneName + "/MeshKit/Backup Scene/";
					MeshAssets.seperatedMeshFolder = sceneParentFolder + sceneName + "/MeshKit/Seperated Meshes/";
					MeshAssets.combinedMeshFolder = sceneParentFolder + sceneName + "/MeshKit/Combined Meshes/";
					MeshAssets.invertedMeshFolder = sceneParentFolder + sceneName + "/MeshKit/Inverted Meshes/";
					MeshAssets.doubleSidedMeshFolder = sceneParentFolder + sceneName + "/MeshKit/DoubleSided Meshes/";
					MeshAssets.rebuiltMeshFolder = sceneParentFolder + sceneName + "/MeshKit/Rebuilt Meshes/";
					MeshAssets.decimatedMeshFolder = sceneParentFolder + sceneName + "/MeshKit/Decimated Meshes/";
					MeshAssets.lodMeshFolder = sceneParentFolder + sceneName + "/MeshKit/LOD Meshes/";

					// Debug
					if(MeshKitGUI.verbose){ 
						Debug.Log("BackupSceneFolder: "+ MeshAssets.backupSceneFolder); 
						Debug.Log("seperatedMeshFolder: "+ MeshAssets.seperatedMeshFolder); 
						Debug.Log("combinedMeshFolder: "+ MeshAssets.combinedMeshFolder); 
						Debug.Log("invertedMeshFolder: "+ MeshAssets.invertedMeshFolder); 
						Debug.Log("DoubleSidedMeshes: "+ MeshAssets.doubleSidedMeshFolder); 
						Debug.Log("RebuiltMeshFolder: "+ MeshAssets.rebuiltMeshFolder); 
						Debug.Log("DecimatedMeshFolder: "+ MeshAssets.decimatedMeshFolder); 
						Debug.Log("lodMeshFolder: "+ MeshAssets.lodMeshFolder); 
					}
				}

				// This turns off this check until the scene changes again (the main GUI Window updates this script when that happens!)
				updateSupportFoldersAndResources = false;

				// Return true
				return true;
			} 

			// Return false if something went wrong...
			return false;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	CREATE MESH ASSET
		//	Creates a Mesh Asset in a special way, preserving older bakes.
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static Mesh CreateMeshAsset( UnityEngine.Object asset, string path ){

			// Make sure the asset and path are valid ...
			if( asset != null && string.IsNullOrEmpty(path) == false ){

				try {

					// This method ensures that the new meshes that are overwritten, do not delete older references
					Mesh outputObject = AssetDatabase.LoadMainAssetAtPath (path) as Mesh;
					if (outputObject != null) {

						EditorUtility.CopySerialized (asset, outputObject);
						AssetDatabase.SaveAssets();

					// If this is the first time the asset is being created, it should be done so normally...	
					} else {

						AssetDatabase.CreateAsset (asset, path);
						AssetDatabase.SaveAssets();
						AssetDatabase.ImportAsset(path);
					}

					// Return the Asset, loaded from the database directly ...
					return AssetDatabase.LoadAssetAtPath(path, typeof(Mesh) ) as Mesh;

				} catch (Exception e) {
					MeshAssets.errorCreatingAsset = true; // Let the calling script know there was a problem creating this asset.
	        		Debug.Log(" MESH ASSETS: Error Creating Asset: "+e);
	        		return null;
	    		}  

			} else {
				Debug.Log(" MESH ASSETS: Could not create new asset because of a problem with the source file or path.");
			}

			// return null if something goes wrong ...
			return null;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	HOW MANY FILES AT ASSET PATH
		//	Finds out how many files there are in an asset folder
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static int HowManyFilesAtAssetPath( string path ){

			if(MeshKitGUI.verbose){ Debug.Log("Searching for how many files there are in: "+ path); }
			string localPrefix = Application.dataPath.Replace("/" + "Assets","")+"/";
			var info = new DirectoryInfo(localPrefix+path);
			if(info != null && info.GetFiles() != null ){
				FileInfo[] fileInfo = info.GetFiles();
				if( fileInfo != null ){
					if(MeshKitGUI.verbose){ Debug.Log("Found "+ fileInfo.Length + " Files At: "+ path); }
					return fileInfo.Length;
				}
			}
	 		
	 		// If something goes wrong ...
	 		Debug.Log("MESHKIT: Returning 0 files at Asset Path because something went wrong accessing filepath: "+ path);
	 		return 0;	
		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	GET ASSET FILENAMES FROM FOLDER
		//	Returns a list of filenames as string[] from files ending in ".asset". 
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static string[] GetAssetFilenamesFromFolder( string path ){

			ArrayList array = new ArrayList();
			array.Clear();

			var info = new DirectoryInfo( path );
			var fileInfo = info.GetFiles();
			foreach (FileInfo file in fileInfo){
				if( Path.GetExtension( file.ToString() ) == ".asset"){ 
					array.Add( file.ToString().Replace(prefix, "") );
				}
			}

			return (string[])array.ToArray(typeof(string));
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		//	ARE MESH ASSETS BEING USED IN THE SCENE?
		//	Goes through all the meshes in the Combined and SubMeshes folders and checks to see if they
		//	are in use in the scene.
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static void AreMeshAssetsBeingUsedInTheScene(){

			// Setup an ArrayList to track how many assets are not being used in the Scene.
			ArrayList unusedAssetsArray = new ArrayList();
			unusedAssetsArray.Clear();

			// Make sure we have created the Support Folders
			if( HaveSupportFoldersBeenCreated() ){

				// ===========================================
				//	CACHE ALL THE MESH FILTERS IN THE SCENE
				// ===========================================

				// Cache all the MeshFilters In This Scene
				MeshFilter[] allMeshfilters = Resources.FindObjectsOfTypeAll(typeof(MeshFilter)) as MeshFilter[];
				ArrayList filteredMFs = new ArrayList(); filteredMFs.Clear();
				foreach( MeshFilter mf in allMeshfilters){
					// If this MeshFilter is a weird internal object that cant be edited, dont count it ...
					if (mf.gameObject.hideFlags == HideFlags.NotEditable || mf.gameObject.hideFlags == HideFlags.HideAndDontSave){
	            		continue;
					}
					// If this MeshFilter is attached to a GameObject thats root exists in the Project pane, skip it ...
					string assetPath = AssetDatabase.GetAssetPath(mf.gameObject.transform.root.gameObject);
			        if (!string.IsNullOrEmpty(assetPath)){
			            continue;
			        }
			        // Otherwise, add it to the new list
			        filteredMFs.Add(mf);
				}
				MeshFilter[] meshfilters = (MeshFilter[])filteredMFs.ToArray(typeof(MeshFilter));


				// ===================================================
				//	CACHE ALL THE SKINNED MESH RENDERERS IN THE SCENE
				// ===================================================

				// Cache all the MeshFilters In This Scene
				SkinnedMeshRenderer[] allSkinnedMeshRenderers = Resources.FindObjectsOfTypeAll(typeof(SkinnedMeshRenderer)) as SkinnedMeshRenderer[];
				ArrayList filteredSMRs = new ArrayList(); filteredSMRs.Clear();
				foreach( SkinnedMeshRenderer smr in allSkinnedMeshRenderers){
					// If this SkinnedMeshRenderer is a weird internal object that cant be edited, dont count it ...
					if (smr.gameObject.hideFlags == HideFlags.NotEditable || smr.gameObject.hideFlags == HideFlags.HideAndDontSave){
	            		continue;
					}
					// If this SkinnedMeshRenderer is attached to a GameObject thats root exists in the Project pane, skip it ...
					string assetPath = AssetDatabase.GetAssetPath(smr.gameObject.transform.root.gameObject);
			        if (!string.IsNullOrEmpty(assetPath)){
			            continue;
			        }
			        // Otherwise, add it to the new list
			        filteredSMRs.Add(smr);
				}
				SkinnedMeshRenderer[] skinnedMeshRenderers = (SkinnedMeshRenderer[])filteredSMRs.ToArray(typeof(SkinnedMeshRenderer));


				// ===========================================================================
				//	FIND ALL ASSET FILENAMES IN THE SUBMESH AND COMBINED MESH FOLDERS
				// ===========================================================================

				// Debug Progress
				if(MeshKitGUI.verbose){ Debug.Log(meshfilters.Length +" MeshFilters found in scene!"); }

				// Find all asset filenames in the SubMesh Folder
	 			string[] subMeshAssets = GetAssetFilenamesFromFolder( prefix+seperatedMeshFolder );
	 			if(MeshKitGUI.verbose){ Debug.Log(subMeshAssets.Length + " SubMesh Assets Found."); }

	 			// Find all asset filenames in the Combined Folder
	 			string[] combinedMeshAssets = GetAssetFilenamesFromFolder( prefix+combinedMeshFolder );
	 			if(MeshKitGUI.verbose){ Debug.Log(combinedMeshAssets.Length + " Combined Meshes Found."); }

	 			// Find all asset filenames in the Inverted Folder
	 			string[] invertedMeshAssets = GetAssetFilenamesFromFolder( prefix+invertedMeshFolder );
	 			if(MeshKitGUI.verbose){ Debug.Log(invertedMeshAssets.Length + " Inverted Meshes Found."); }

	 			// Find all asset filenames in the Double Sided Folder
	 			string[] doubleSidedMeshAssets = GetAssetFilenamesFromFolder( prefix+doubleSidedMeshFolder );
	 			if(MeshKitGUI.verbose){ Debug.Log(doubleSidedMeshAssets.Length + " Double-Sided Meshes Found."); }

	 			// Find all asset filenames in the Rebuilt Folder
	 			string[] rebuiltMeshAssets = GetAssetFilenamesFromFolder( prefix+rebuiltMeshFolder );
	 			if(MeshKitGUI.verbose){ Debug.Log(rebuiltMeshAssets.Length + " Rebuilt Meshes Found."); }

	 			// Find all asset filenames in the Decimated Folder
	 			string[] decimatedMeshAssets = GetAssetFilenamesFromFolder( prefix+decimatedMeshFolder );
	 			if(MeshKitGUI.verbose){ Debug.Log(decimatedMeshAssets.Length + " Decimated Meshes Found."); }

	 			// Find all asset filenames in the LOD Folder
	 			string[] lodMeshAssets = GetAssetFilenamesFromFolder( prefix+lodMeshFolder );
	 			if(MeshKitGUI.verbose){ Debug.Log(lodMeshAssets.Length + " LOD Meshes Found."); }

	 			// Setup Progress Bar (add all the mesh assets together)
	 			maxProgress = subMeshAssets.Length + combinedMeshAssets.Length + invertedMeshAssets.Length + doubleSidedMeshAssets.Length + rebuiltMeshAssets.Length + decimatedMeshAssets.Length + lodMeshAssets.Length;
	 			progress = 0f;

	 			// ===========================================================================
				//	LOOP THROUGH SUBMESH ASSETS
				// ===========================================================================

				// NOTE: We use a Replace(Path.DirectorySeparatorChar.ToString(), "/") to get this to work on Windows too. On Mac, it will always return / so this should work properly on both platforms.

	 			// Loop through the SubMesh Assets
	 			foreach( string s in subMeshAssets ){
	 				if(MeshKitGUI.verbose){ Debug.Log("Sending: " + (s.Replace(Path.DirectorySeparatorChar.ToString(), "/")).Replace(prefix, "") ); }
	 				if( IsThisMeshAssetBeingUsedInTheScene((s.Replace(Path.DirectorySeparatorChar.ToString(), "/")).Replace(prefix, ""), meshfilters, skinnedMeshRenderers ) == false ){
	 					unusedAssetsArray.Add( (s.Replace(Path.DirectorySeparatorChar.ToString(), "/")).Replace(prefix, "") );
	 				}

	 				// Show Progress Bar
					if( maxProgress > 0 ){
						EditorUtility.DisplayProgressBar(" Clean Mesh Assets", "Scanning SubMeshes ...", progress / maxProgress);
					} else {
						EditorUtility.ClearProgressBar();
					}

					// Increment Progress
					progress += 1;
	 			}

	 			// ===========================================================================
				//	LOOP THROUGH COMBINED MESH ASSETS
				// ===========================================================================

	 			// Loop through the SubMesh Assets
	 			foreach( string s2 in combinedMeshAssets ){
	 				if(MeshKitGUI.verbose){ Debug.Log("Sending: " + (s2.Replace(Path.DirectorySeparatorChar.ToString(), "/")).Replace(prefix, "") ); }
	 				if( IsThisMeshAssetBeingUsedInTheScene((s2.Replace(Path.DirectorySeparatorChar.ToString(), "/")).Replace(prefix, ""), meshfilters, skinnedMeshRenderers ) == false ){
	 					unusedAssetsArray.Add( (s2.Replace(Path.DirectorySeparatorChar.ToString(), "/")).Replace(prefix, "") );
	 				}

	 				// Show Progress Bar
					if( maxProgress > 0 ){
						EditorUtility.DisplayProgressBar(" Clean Mesh Assets", "Scanning Combined Meshes ...", progress / maxProgress);
					} else {
						EditorUtility.ClearProgressBar();
					}

					// Increment Progress
					progress += 1;
	 			}

	 			// ===========================================================================
				//	LOOP THROUGH INVERTED MESH ASSETS
				// ===========================================================================

	 			// Loop through the SubMesh Assets
	 			foreach( string s3 in invertedMeshAssets ){
	 				if(MeshKitGUI.verbose){ Debug.Log("Sending: " + (s3.Replace(Path.DirectorySeparatorChar.ToString(), "/")).Replace(prefix, "") ); }
	 				if( IsThisMeshAssetBeingUsedInTheScene((s3.Replace(Path.DirectorySeparatorChar.ToString(), "/")).Replace(prefix, ""), meshfilters, skinnedMeshRenderers ) == false ){
	 					unusedAssetsArray.Add( (s3.Replace(Path.DirectorySeparatorChar.ToString(), "/")).Replace(prefix, "") );
	 				}

	 				// Show Progress Bar
					if( maxProgress > 0 ){
						EditorUtility.DisplayProgressBar(" Clean Mesh Assets", "Scanning Inverted Meshes ...", progress / maxProgress);
					} else {
						EditorUtility.ClearProgressBar();
					}

					// Increment Progress
					progress += 1;
	 			}

	 			// ===========================================================================
				//	LOOP THROUGH DOUBLE-SIDED MESH ASSETS
				// ===========================================================================

	 			// Loop through the SubMesh Assets
	 			foreach( string s4 in doubleSidedMeshAssets ){
	 				if(MeshKitGUI.verbose){ Debug.Log("Sending: " + (s4.Replace(Path.DirectorySeparatorChar.ToString(), "/")).Replace(prefix, "") ); }
	 				if( IsThisMeshAssetBeingUsedInTheScene((s4.Replace(Path.DirectorySeparatorChar.ToString(), "/")).Replace(prefix, ""), meshfilters, skinnedMeshRenderers ) == false ){
	 					unusedAssetsArray.Add( (s4.Replace(Path.DirectorySeparatorChar.ToString(), "/")).Replace(prefix, "") );
	 				}

	 				// Show Progress Bar
					if( maxProgress > 0 ){
						EditorUtility.DisplayProgressBar(" Clean Mesh Assets", "Scanning Double-Sided Meshes ...", progress / maxProgress);
					} else {
						EditorUtility.ClearProgressBar();
					}

					// Increment Progress
					progress += 1;
	 			}

	 			// ===========================================================================
				//	LOOP THROUGH REBUILT MESH ASSETS
				// ===========================================================================

	 			// Loop through the SubMesh Assets
	 			foreach( string s5 in rebuiltMeshAssets ){
	 				if(MeshKitGUI.verbose){ Debug.Log("Sending: " + (s5.Replace(Path.DirectorySeparatorChar.ToString(), "/")).Replace(prefix, "") ); }
	 				if( IsThisMeshAssetBeingUsedInTheScene((s5.Replace(Path.DirectorySeparatorChar.ToString(), "/")).Replace(prefix, ""), meshfilters, skinnedMeshRenderers ) == false ){
	 					unusedAssetsArray.Add( (s5.Replace(Path.DirectorySeparatorChar.ToString(), "/")).Replace(prefix, "") );
	 				}

	 				// Show Progress Bar
					if( maxProgress > 0 ){
						EditorUtility.DisplayProgressBar(" Clean Mesh Assets", "Scanning Rebuilt Meshes ...", progress / maxProgress);
					} else {
						EditorUtility.ClearProgressBar();
					}

					// Increment Progress
					progress += 1;
	 			}

	 			// ===========================================================================
				//	LOOP THROUGH DECIMATED MESH ASSETS
				// ===========================================================================

	 			// Loop through the SubMesh Assets
	 			foreach( string s6 in decimatedMeshAssets ){
	 				if(MeshKitGUI.verbose){ Debug.Log("Sending: " + (s6.Replace(Path.DirectorySeparatorChar.ToString(), "/")).Replace(prefix, "") ); }
	 				if( IsThisMeshAssetBeingUsedInTheScene((s6.Replace(Path.DirectorySeparatorChar.ToString(), "/")).Replace(prefix, ""), meshfilters, skinnedMeshRenderers ) == false ){
	 					unusedAssetsArray.Add( (s6.Replace(Path.DirectorySeparatorChar.ToString(), "/")).Replace(prefix, "") );
	 				}

	 				// Show Progress Bar
					if( maxProgress > 0 ){
						EditorUtility.DisplayProgressBar(" Clean Mesh Assets", "Scanning Decimated Meshes ...", progress / maxProgress);
					} else {
						EditorUtility.ClearProgressBar();
					}

					// Increment Progress
					progress += 1;
	 			}

	 			// ===========================================================================
				//	LOOP THROUGH LOD MESH ASSETS
				// ===========================================================================

	 			// Loop through the LOD Assets
	 			foreach( string s7 in lodMeshAssets ){
	 				if(MeshKitGUI.verbose){ Debug.Log("Sending: " + (s7.Replace(Path.DirectorySeparatorChar.ToString(), "/")).Replace(prefix, "") ); }
	 				if( IsThisMeshAssetBeingUsedInTheScene((s7.Replace(Path.DirectorySeparatorChar.ToString(), "/")).Replace(prefix, ""), meshfilters, skinnedMeshRenderers ) == false ){
	 					unusedAssetsArray.Add( (s7.Replace(Path.DirectorySeparatorChar.ToString(), "/")).Replace(prefix, "") );
	 				}

	 				// Show Progress Bar
					if( maxProgress > 0 ){
						EditorUtility.DisplayProgressBar(" Clean Mesh Assets", "Scanning LOD Meshes ...", progress / maxProgress);
					} else {
						EditorUtility.ClearProgressBar();
					}

					// Increment Progress
					progress += 1;
	 			}

	 			// Stop Progress Bar
				maxProgress = 0;
				progress = 0;
				EditorUtility.ClearProgressBar();

				// Show Result
	 			if(MeshKitGUI.verbose){ Debug.Log("RESULT: There were "+ unusedAssetsArray.Count + " Meshes that were not being used in this scene."); }

	 			// ===========================================================================
				//	DELETE UNUSED MESHES
				// ===========================================================================

	 			// Allow user to delete the meshes if more than 1 were found
	 			if( unusedAssetsArray.Count > 0){

		 			if( EditorUtility.DisplayDialog(" Clean Mesh Assets", unusedAssetsArray.Count + " unused Mesh Asset/s were detected.\n\nThese objects are meshes that were originally optimized and created for this scene but no MeshRenderers in your scene appear to be using them. Do you want to delete these assets?\n\nNOTE: This cannot be undone.", "Yes", "No") ){

		 				progress = 0;
		 				maxProgress = unusedAssetsArray.Count;

		 				// Loop through the Unused Assets And Delete Them ...
		 				foreach( string deletePath in unusedAssetsArray ){
		 					if(MeshKitGUI.verbose){ Debug.Log("Deleting Asset: "+deletePath); }
		 					AssetDatabase.DeleteAsset(deletePath);

		 					// Show Progress Bar
							if( maxProgress > 0 ){
								EditorUtility.DisplayProgressBar("Clean Mesh Assets", "Deleting Unused Mesh ( "+ progress.ToString() + " / " + maxProgress.ToString() + " )", progress / maxProgress);
							} else {
								EditorUtility.ClearProgressBar();
							}

							// Increment Progrerss
							progress++;
		 				}

		 			} else {
		 				if(MeshKitGUI.verbose){ Debug.Log("Clean Mesh Assets: Leaving unused Mesh Assets in place."); }
		 			}

		 		// Otherwise let the user know everything is all good!
	 			} else {
	 				EditorUtility.DisplayDialog("Clean Mesh Assets", "No unused Meshes were detected in this scene!", "Okay");
	 			}	

	 			// Reset Progress Bar
	 			progress = 0;
		 		maxProgress = 0;
	 			EditorUtility.ClearProgressBar();
			}
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		//	IS THIS MESH ASSET BEING USED IN THE SCENE
		//	Loads an Asset by its filename and checks to see if it is being used by any Mesh Filters
		//	As of MeshKit v2: This also checks SkinnedMeshRenderers in the scene too!
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static bool IsThisMeshAssetBeingUsedInTheScene( string path, MeshFilter[] mfs, SkinnedMeshRenderer[] smrs ){

			Mesh m = AssetDatabase.LoadAssetAtPath(path, typeof(Mesh) ) as Mesh;
			if( m != null ){
				
				// Loop through all the MeshFilters and see if this Mesh is being used
				if( mfs != null ){
					foreach( MeshFilter mf in mfs ){
						if( mf.sharedMesh == m ){
							return true;
						}
					}
				}

				// Loop through all the SkinnedMeshRenderers and see if this Mesh is being used
				if( smrs != null ){
					foreach( SkinnedMeshRenderer smr in smrs ){
						if( smr.sharedMesh == m ){
							return true;
						}
					}
				}

				// If we didn't find a MeshFilter for this Mesh, mark it false.
				return false;
			}

			// Return true if there was a problem with the asset file (its probably not a Mesh, or a file the user put there by mistake).
			return true;
		}		

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	PROCESS FILENAME
		//	A unified way of saving files in MeshKit. Provides a full filepath
		//
		//			numberOfFilesInDirectory + _ + GameObject Name + _ + operationVerb + ".asset"
		//	eg.   	00001_Name_Of_Original_GameObject_Inverted.asset
		//			00003_NameOf_OriginalGameObject_Combined.asset
		//
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static string ProcessFileName( string saveDirectory, string goName, string operationVerb, bool onlyReturnFileName ){

			// Get the number of files in the saveDirectory and format it as a 5 number string
			string numberOfFiles = HowManyFilesAtAssetPath(saveDirectory).ToString("D5")+"_";

			// Take the name of the gameobject, make it system safe and limit it to 30 characters long.
			string filename = goName.MakeFileSystemSafe( char.Parse("_") ) + "_";
			if(filename.Length > 30){
				filename = filename.Substring(0,28)+"_";
			}

			// Make sure the Operation Verb is also system Safe and limited to 20 characters
			operationVerb.MakeFileSystemSafe( char.Parse("_") );
			if(operationVerb.Length > 20){
				operationVerb = filename.Substring(0,19);
			}

			// Return the filename
			if(onlyReturnFileName){
				return numberOfFiles + filename + operationVerb + ".asset";
			}

			// Return the full directory
			return saveDirectory + numberOfFiles + filename + operationVerb + ".asset";
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		//	IS MESH MANAGED BY MESHKIT?
		//	Checks to see if a MeshFilters sharedMesh is managed By MeshKit. Accepts meshes directly or a MeshFilter
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static bool IsThisMeshManagedByMeshkit( MeshFilter mf, bool ignoreSceneName ){	

			// Make sure this asset actually has an asset path and doesn't belong to any other asset (ie a subchild of another mesh)
			if( mf.sharedMesh != null && 
				AssetDatabase.IsSubAsset( mf.sharedMesh ) == false &&
				AssetDatabase.GetAssetPath( mf.sharedMesh ) != null && 
				AssetDatabase.GetAssetPath( mf.sharedMesh ) != ""
			){

				// Get the directory of the asset and split it into sections
				string[] directorySections = Path.GetDirectoryName( AssetDatabase.GetAssetPath( mf.sharedMesh ) ).Split('/');

				// Make sure there are at least 3 levels of directories
				if( directorySections.Length >= 3 ){
				//	Debug.Log("1st Parent: "+ directorySections[directorySections.Length-1] );	// Combined Meshes, Inverted Meshes, etc.
				//	Debug.Log("2nd Parent: "+ directorySections[directorySections.Length-2] );	// MeshKit
				//	Debug.Log("3rd Parent: "+ directorySections[directorySections.Length-3] );	// Unity Scene Name

					// Make sure the 2nd parent is "MeshKit" and the third parent is NOT the name of the current Unity Scene
					if( directorySections[directorySections.Length-2] == "MeshKit" && 
						(ignoreSceneName || !ignoreSceneName && directorySections[directorySections.Length-3] != Path.GetFileName(HTScene.CurrentScene() ).Replace(".unity",""))
					){

						// If this mesh is in one of the core asset folders, return true
						if( directorySections[directorySections.Length-1] == "Combined Meshes" ||
							directorySections[directorySections.Length-1] == "DoubleSided Meshes" ||
							directorySections[directorySections.Length-1] == "Inverted Meshes" ||
							directorySections[directorySections.Length-1] == "Seperated Meshes" ||
							directorySections[directorySections.Length-1] == "Rebuilt Meshes" ||
							directorySections[directorySections.Length-1] == "Decimated Meshes"||
							directorySections[directorySections.Length-1] == "LOD Meshes"
						){
							// Return true if this checks out!
							return true;
						}
					}
				}
			}

			// Return false
			return false;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	IS MESH MANAGED BY MESHKIT?
		//	Overloaded version of the above that checks SkinnedMeshRenderers
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static bool IsThisMeshManagedByMeshkit( SkinnedMeshRenderer smr, bool ignoreSceneName ){	

			// Make sure this asset actually has an asset path and doesn't belong to any other asset (ie a subchild of another mesh)
			if( smr.sharedMesh != null && 
				AssetDatabase.IsSubAsset( smr.sharedMesh ) == false &&
				AssetDatabase.GetAssetPath( smr.sharedMesh ) != null && 
				AssetDatabase.GetAssetPath( smr.sharedMesh ) != ""
			){

				// Get the directory of the asset and split it into sections
				string[] directorySections = Path.GetDirectoryName( AssetDatabase.GetAssetPath( smr.sharedMesh ) ).Split('/');

				// Make sure there are at least 3 levels of directories
				if( directorySections.Length >= 3 ){
				//	Debug.Log("1st Parent: "+ directorySections[directorySections.Length-1] );	// Combined Meshes, Inverted Meshes, etc.
				//	Debug.Log("2nd Parent: "+ directorySections[directorySections.Length-2] );	// MeshKit
				//	Debug.Log("3rd Parent: "+ directorySections[directorySections.Length-3] );	// Unity Scene Name

					// Make sure the 2nd parent is "MeshKit" and the third parent is NOT the name of the current Unity Scene
					if( directorySections[directorySections.Length-2] == "MeshKit" && 
						(ignoreSceneName || !ignoreSceneName && directorySections[directorySections.Length-3] != Path.GetFileName(HTScene.CurrentScene() ).Replace(".unity",""))
					){

						// If this mesh is in one of the core asset folders, return true
						if( directorySections[directorySections.Length-1] == "Combined Meshes" ||
							directorySections[directorySections.Length-1] == "DoubleSided Meshes" ||
							directorySections[directorySections.Length-1] == "Inverted Meshes" ||
							directorySections[directorySections.Length-1] == "Seperated Meshes" ||
							directorySections[directorySections.Length-1] == "Rebuilt Meshes" ||
							directorySections[directorySections.Length-1] == "Decimated Meshes" ||
							directorySections[directorySections.Length-1] == "LOD Meshes"
						){
							// Return true if this checks out!
							return true;
						}
					}
				}
			}

			// Return false
			return false;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	KEEP PREFABS GLOBAL
		//	Checks to see if any MeshFilters on prefabs are being managed by MeshKit.
		//	If they are, they need to be copied over to a central folder where it isn't managed.
		////////////////////////////////////////////////////////////////////////////////////////////////

		// Class to save Prefabs
		public class MeshKitPrefabToSaveLater{
			public bool isMeshFilter = true;	// <- Allows us to check if this is a mesh filter or skinnedmeshrenderer
			public MeshFilter mf;
			public SkinnedMeshRenderer smr;
			public GameObject go;
		}

		public static void KeepPrefabsGlobal(){

			// ============================================================
			//	CACHE ALL THE MESH FILTERS IN THE SCENE THAT ARE PREFABS
			// ============================================================

			// Cache all the MeshFilters In This Scene
			MeshFilter[] allMeshfilters = Resources.FindObjectsOfTypeAll(typeof(MeshFilter)) as MeshFilter[];
			ArrayList filteredMFs = new ArrayList(); filteredMFs.Clear();
		//	ArrayList prefabArray = new ArrayList(); prefabArray.Clear();		// <- i dont think this is used [DELETE LATER]
			foreach( MeshFilter mf in allMeshfilters ){
				// If this MeshFilter is a weird internal object that cant be edited, dont count it ...
				if (mf.gameObject.hideFlags == HideFlags.NotEditable || mf.gameObject.hideFlags == HideFlags.HideAndDontSave){
            		continue;
				}
				// If this MeshFilter is attached to a GameOBject thats root exists in the Project pane, skip it ...
				string assetPath = AssetDatabase.GetAssetPath(mf.gameObject.transform.root.gameObject);
		        if (!string.IsNullOrEmpty(assetPath)){
		            continue;
		        }
		        // Otherwise, add it to the new list
		        if( mf.sharedMesh != null && // Make sure there IS a shared Mesh in the first place!
		        	MeshKitPrefabUtility.GetPrefabObject(mf.gameObject) != null && // This lets us know if this object is actually a prefab
		        	MeshKitPrefabUtility.FindRootGameObjectWithSameParentPrefab(mf.gameObject) != null &&	// Can we grab the parent prefab
		        	IsThisMeshManagedByMeshkit( mf, true ) // We ignore the name of the scene to determine if this is managed by MeshKit.
		        ){
		        	// Add the MeshFilter
		        	filteredMFs.Add(mf);
		    	}
			}

			// Convert the arrays into builtin lists
			MeshFilter[] meshfilters = (MeshFilter[])filteredMFs.ToArray(typeof(MeshFilter));
			if(MeshKitGUI.verbose){ Debug.Log(meshfilters.Length + " MeshFilters Found in this scene ( KeepPrefabsGlobal )"); }


			// ====================================================================
			//	CACHE ALL THE SKINNED MESH RENDERERS IN THE SCENE THAT ARE PREFABS
			// ====================================================================

			// Cache all the MeshFilters In This Scene
			SkinnedMeshRenderer[] allSkinnedMeshRenderers = Resources.FindObjectsOfTypeAll(typeof(SkinnedMeshRenderer)) as SkinnedMeshRenderer[];
			ArrayList filteredSMRs = new ArrayList(); filteredSMRs.Clear();
		//	ArrayList prefabArray = new ArrayList(); prefabArray.Clear();		// <- i dont think this is used [DELETE LATER]
			foreach( SkinnedMeshRenderer smr in allSkinnedMeshRenderers ){
				// If this SkinnedMeshRenderer is a weird internal object that cant be edited, dont count it ...
				if (smr.gameObject.hideFlags == HideFlags.NotEditable || smr.gameObject.hideFlags == HideFlags.HideAndDontSave){
            		continue;
				}
				// If this SkinnedMeshRenderer is attached to a GameOBject thats root exists in the Project pane, skip it ...
				string assetPath = AssetDatabase.GetAssetPath(smr.gameObject.transform.root.gameObject);
		        if (!string.IsNullOrEmpty(assetPath)){
		            continue;
		        }
		        // Otherwise, add it to the new list
		        if( smr.sharedMesh != null && // Make sure there IS a shared Mesh in the first place!
		        	MeshKitPrefabUtility.GetPrefabObject(smr.gameObject) != null && // This lets us know if this object is actually a prefab
		        	MeshKitPrefabUtility.FindRootGameObjectWithSameParentPrefab(smr.gameObject) != null &&	// Can we grab the parent prefab
		        	IsThisMeshManagedByMeshkit( smr, true ) // We ignore the name of the scene to determine if this is managed by MeshKit.
		        ){
		        	// Add the SkinnedMeshRenderer
		        	filteredSMRs.Add(smr);
		    	}
			}

			// Convert the arrays into builtin lists
			SkinnedMeshRenderer[] skinnedMeshRenderers = (SkinnedMeshRenderer[])filteredSMRs.ToArray(typeof(SkinnedMeshRenderer));
			if(MeshKitGUI.verbose){ Debug.Log(skinnedMeshRenderers.Length + " SkinnedMeshRenderers Found in this scene ( KeepPrefabsGlobal )"); }

			// ============================================================
			//	FIX PREFABS
			// ============================================================

			// Show success message
			if(meshfilters.Length > 0 || skinnedMeshRenderers.Length > 0 ){
				if( EditorUtility.DisplayDialog("Prefabs Are Using Local Meshes!", "MeshKit has detected that you are using Prefabs with meshes that are being managed for a specific Unity Scene. By making a change in that scene you may inadvertently delete the mesh in your prefab.\n\nMeshKit can automatically rebuild these Prefabs for you by duplicating their Mesh files into the Assets folder of your project where they will no longer be tracked by MeshKit (and you will be able to use them normally).\n\nWould you like MeshKit to automatically fix your Prefabs?", "Yes", "No")
				){

					// Setup Progress Bar
					maxProgress = meshfilters.Length + skinnedMeshRenderers.Length;
					progress = 0;

					// Create a MeshKit Prefabs Folder
					string assetsFilePath = Application.dataPath+"/"+"MeshKit Prefab Meshes"+"/";
					//Debug.Log(assetsFilePath); 
					if( !Directory.Exists( assetsFilePath )  ){
						AssetDatabase.CreateFolder("Assets","MeshKit Prefab Meshes");
						AssetDatabase.ImportAsset("Assets/MeshKit Prefab Meshes");
					}

					// Setup a list of files that have been copied
					ArrayList copiedFiles = new ArrayList();
					copiedFiles.Clear();

					// ============================================================
					//	PREPARE AN ARRAY TO STORE THE PREFABS TO COPY
					// ============================================================

					// Create a new Array where we store all the MeshFilters and SkinnedMeshRendereres that we recorded Changes on
					ArrayList meshKitPrefabArray = new ArrayList();  meshKitPrefabArray.Clear();

					// ============================================================
					//	COPY OVER THE MESH FILTERS AND REPLACE THE OLD REFERENCES
					// ============================================================

					// Copy the meshes needed
					if(meshfilters.Length > 0 ){
						foreach( MeshFilter mf2 in meshfilters ){
							if( mf2 != null && mf2.sharedMesh != null ){

								// Show Progress Bar
								if( maxProgress > 0 ){
									EditorUtility.DisplayProgressBar("Fixing Prefabs", "Copying shared meshes and updating Prefabs ... ( "+ progress.ToString() + " / " + maxProgress.ToString() + " )", progress / maxProgress);
								} else {
									EditorUtility.ClearProgressBar();
								}

								// Create a MeshKit Prefabs subFolder for each object (we also change assetsFilePath at this point)
								string goFolderName = mf2.gameObject.name.MakeFileSystemSafe(' '); // Folder name based on GameObject
								// If we can get a name from the root gameobject that would probably be better ...
								if( MeshKitPrefabUtility.FindRootGameObjectWithSameParentPrefab(mf2.gameObject) != null ){
									goFolderName = MeshKitPrefabUtility.FindRootGameObjectWithSameParentPrefab(mf2.gameObject).name.MakeFileSystemSafe(' ');
								}

								assetsFilePath = Application.dataPath+"/"+"MeshKit Prefab Meshes"+"/"+goFolderName;
								if(MeshKitGUI.verbose){ Debug.Log("goFolderName: "+ goFolderName); }
								if(MeshKitGUI.verbose){ Debug.Log("Asset File Path: " +assetsFilePath); }
								if( !Directory.Exists( assetsFilePath )  ){
									if(MeshKitGUI.verbose){ Debug.Log("Creating Folder: "+ "Assets/MeshKit Prefab Meshes" + goFolderName); }
									AssetDatabase.CreateFolder("Assets/MeshKit Prefab Meshes", goFolderName);
									AssetDatabase.ImportAsset("Assets/MeshKit Prefab Meshes/" + goFolderName);
								}
								if(MeshKitGUI.verbose){ Debug.Log("Finished Creating Folder MeshKit Prefab Folders."); }

								// If the MeshFilter is valid and includes a shared Mesh
								if(mf2!=null && mf2.sharedMesh != null ){

									// Get the number of files in the copyTO directory
									int numberOfFiles = HowManyFilesAtAssetPath( /*prefix + */"Assets/MeshKit Prefab Meshes/" + goFolderName);
									//Debug.Log("Checking number of files in: "+prefix+ "Assets/MeshKit Prefab Meshes/"+ goFolderName);
									if(MeshKitGUI.verbose){ Debug.Log("NumberOfFiles: "+numberOfFiles); }

									// Set up copy variables
									string copyFrom = AssetDatabase.GetAssetPath(mf2.sharedMesh);
									string copyTo = "Assets/MeshKit Prefab Meshes/" + goFolderName +"/"+ Path.GetFileName( AssetDatabase.GetAssetPath(mf2.sharedMesh));

									// Debug
									if(MeshKitGUI.verbose){ 
										Debug.Log("copyFrom: "+ copyFrom);
										Debug.Log("copyTo: "+ copyTo);
									}

									// If this file hasn't already been copied, process it ... 
									// NOTE: copying over the same file twice messes everything up and causes missing meshes!
									if( MeshAssets.StringExistsInArrayList(copiedFiles, copyFrom) == false ){

										// If a file exists in the copy destination, delete it before copying the new one, otherwise
										// unity will display ghost copies in the Project view until you re-open the application.
									//	Debug.Log("File Exists? assetsFilePath+Path.GetFileName( AssetDatabase.GetAssetPath(mf2.sharedMesh)");
										if( File.Exists( assetsFilePath+Path.GetFileName( AssetDatabase.GetAssetPath(mf2.sharedMesh)) )
										){
											if(MeshKitGUI.verbose){ Debug.Log("Deleting Old File: "+ assetsFilePath+Path.GetFileName( AssetDatabase.GetAssetPath(mf2.sharedMesh)) ); }
											AssetDatabase.DeleteAsset(copyTo);
										}

										// Copy The File
										AssetDatabase.CopyAsset( copyFrom, copyTo );
										AssetDatabase.ImportAsset(copyTo);

										// Add this file to the ArrayList
										copiedFiles.Add(copyFrom);

									}

									// Once copied, attempt to load it back and set it as the new Shared Mesh.
									Mesh newMesh = AssetDatabase.LoadAssetAtPath(copyTo, typeof(Mesh) ) as Mesh;
									if( newMesh != null ){

										// Set the new Mesh to the old prefab
										mf2.sharedMesh = newMesh;

										// Make sure we can access the prefab GameObject
										if( MeshKitPrefabUtility.FindValidUploadPrefabInstanceRoot( mf2.gameObject ) as GameObject != null ){

											// Find the main GameObject of this prefab and check if its already in this new array
											bool exists = false;
											if( meshKitPrefabArray.Count > 0 ){
												foreach( MeshKitPrefabToSaveLater x in meshKitPrefabArray ){
													if(MeshKitGUI.verbose){ Debug.Log( "IF " + x.go + " == " + (MeshKitPrefabUtility.FindValidUploadPrefabInstanceRoot(mf2.gameObject) as GameObject) ); }
													if( x!=null && x.go ==  MeshKitPrefabUtility.FindValidUploadPrefabInstanceRoot(mf2.gameObject) as GameObject ){
														if(MeshKitGUI.verbose){ Debug.Log("Exists!"); }
														exists = true;
													}
												}
											}

											// If this prefab doesnt already exist in this array, add it
											if(exists == false){

												// Create a new MeshKitPrefabToSaveLater to store the component and the GameObject I want to save later ( this one is a MESHFILTER )
												MeshKitPrefabToSaveLater savePrefabLater = new MeshKitPrefabToSaveLater();
												savePrefabLater.isMeshFilter = true;
												savePrefabLater.mf = mf2;
												savePrefabLater.smr = null;
												savePrefabLater.go = MeshKitPrefabUtility.FindValidUploadPrefabInstanceRoot(mf2.gameObject) as GameObject;

												meshKitPrefabArray.Add( savePrefabLater );
											}
										}

									} else { Debug.LogWarning("MESHKIT: couldn't load Mesh at: "+ copyTo); }
								}
							}

							// Increment the Prefab Fixxer
							progress++;
						}
					}

					// ======================================================================
					//	COPY OVER THE SKINNED MESH RENDERERS AND REPLACE THE OLD REFERENCES
					// =====================================================================

					// Copy the meshes needed
					if(skinnedMeshRenderers.Length > 0 ){
						foreach( SkinnedMeshRenderer smr2 in skinnedMeshRenderers ){
							if( smr2 != null && smr2.sharedMesh != null ){

								// Show Progress Bar
								if( maxProgress > 0 ){
									EditorUtility.DisplayProgressBar("Fixing Prefabs", "Copying shared meshes and updating Prefabs ... ( "+ progress.ToString() + " / " + maxProgress.ToString() + " )", progress / maxProgress);
								} else {
									EditorUtility.ClearProgressBar();
								}

								// Create a MeshKit Prefabs subFolder for each object (we also change assetsFilePath at this point)
								string goFolderName = smr2.gameObject.name.MakeFileSystemSafe(' '); // Folder name based on GameObject
								// If we can get a name from the root gameobject that would probably be better ...
								if( MeshKitPrefabUtility.FindRootGameObjectWithSameParentPrefab(smr2.gameObject) != null ){
									goFolderName = MeshKitPrefabUtility.FindRootGameObjectWithSameParentPrefab(smr2.gameObject).name.MakeFileSystemSafe(' ');
								}

								assetsFilePath = Application.dataPath+"/"+"MeshKit Prefab Meshes"+"/"+goFolderName;
								if(MeshKitGUI.verbose){ Debug.Log("goFolderName: "+ goFolderName); }
								if(MeshKitGUI.verbose){ Debug.Log("Asset File Path: " +assetsFilePath); }
								if( !Directory.Exists( assetsFilePath )  ){
									if(MeshKitGUI.verbose){ Debug.Log("Creating Folder: "+ "Assets/MeshKit Prefab Meshes" + goFolderName); }
									AssetDatabase.CreateFolder("Assets/MeshKit Prefab Meshes", goFolderName);
									AssetDatabase.ImportAsset("Assets/MeshKit Prefab Meshes/" + goFolderName);
								}
								if(MeshKitGUI.verbose){ Debug.Log("Finished Creating Folder MeshKit Prefab Folders."); }

								// If the Skinned Mesh Renderer is valid and includes a shared Mesh
								if(smr2!=null && smr2.sharedMesh != null ){

									// Get the number of files in the copyTO directory
									int numberOfFiles = HowManyFilesAtAssetPath( /*prefix + */"Assets/MeshKit Prefab Meshes/" + goFolderName);
									//Debug.Log("Checking number of files in: "+prefix+ "Assets/MeshKit Prefab Meshes/"+ goFolderName);
									if(MeshKitGUI.verbose){ Debug.Log("NumberOfFiles: "+numberOfFiles); }

									// Set up copy variables
									string copyFrom = AssetDatabase.GetAssetPath(smr2.sharedMesh);
									string copyTo = "Assets/MeshKit Prefab Meshes/" + goFolderName +"/"+ Path.GetFileName( AssetDatabase.GetAssetPath(smr2.sharedMesh));

									// Debug
									if(MeshKitGUI.verbose){ 
										Debug.Log("copyFrom: "+ copyFrom);
										Debug.Log("copyTo: "+ copyTo);
									}

									// If this file hasn't already been copied, process it ... 
									// NOTE: copying over the same file twice messes everything up and causes missing meshes!
									if( MeshAssets.StringExistsInArrayList(copiedFiles, copyFrom) == false ){

										// If a file exists in the copy destination, delete it before copying the new one, otherwise
										// unity will display ghost copies in the Project view until you re-open the application.
									//	Debug.Log("File Exists? assetsFilePath+Path.GetFileName( AssetDatabase.GetAssetPath(smr2.sharedMesh)");
										if( File.Exists( assetsFilePath+Path.GetFileName( AssetDatabase.GetAssetPath(smr2.sharedMesh)) )
										){
											if(MeshKitGUI.verbose){ Debug.Log("Deleting Old File: "+ assetsFilePath+Path.GetFileName( AssetDatabase.GetAssetPath(smr2.sharedMesh)) ); }
											AssetDatabase.DeleteAsset(copyTo);
										}

										// Copy The File
										AssetDatabase.CopyAsset( copyFrom, copyTo );
										AssetDatabase.ImportAsset(copyTo);

										// Add this file to the ArrayList
										copiedFiles.Add(copyFrom);

									}

									// Once copied, attempt to load it back and set it as the new Shared Mesh.
									Mesh newMesh = AssetDatabase.LoadAssetAtPath(copyTo, typeof(Mesh) ) as Mesh;
									if( newMesh != null ){

										// Set the new Mesh to the old prefab
										smr2.sharedMesh = newMesh;

										// Make sure we can access the prefab GameObject
										if( MeshKitPrefabUtility.FindValidUploadPrefabInstanceRoot( smr2.gameObject ) as GameObject != null ){

											// Find the main GameObject of this prefab and check if its already in this new array
											bool exists = false;
											if( meshKitPrefabArray.Count > 0 ){
												foreach( MeshKitPrefabToSaveLater x in meshKitPrefabArray ){
													if(MeshKitGUI.verbose){ Debug.Log( "IF " + x.go + " == " + (MeshKitPrefabUtility.FindValidUploadPrefabInstanceRoot(smr2.gameObject) as GameObject) ); }
													if( x!=null && x.go ==  MeshKitPrefabUtility.FindValidUploadPrefabInstanceRoot(smr2.gameObject) as GameObject ){
														if(MeshKitGUI.verbose){ Debug.Log("Exists!"); }
														exists = true;
													}
												}
											}

											// If this prefab doesnt already exist in this array, add it
											if(exists == false){

												// Create a new MeshKitPrefabToSaveLater to store the component and the GameObject I want to save later ( this one is a SKINNED MESH RENDERER )
												MeshKitPrefabToSaveLater savePrefabLater = new MeshKitPrefabToSaveLater();
												savePrefabLater.isMeshFilter = false;
												savePrefabLater.mf = null;
												savePrefabLater.smr = smr2;
												savePrefabLater.go = MeshKitPrefabUtility.FindValidUploadPrefabInstanceRoot(smr2.gameObject) as GameObject;

												meshKitPrefabArray.Add( savePrefabLater );
											}
										}

									} else { Debug.LogWarning("MESHKIT: couldn't load Mesh at: "+ copyTo); }
								}
							}

							// Increment the Prefab Fixxer
							progress++;
						}
					}

					// ============================================================
					//	APPLY TO PREFABS
					// ============================================================

					if(MeshKitGUI.verbose){ Debug.Log( meshKitPrefabArray.Count + " PREFAB OBJECTS FOUND TO APPLY CHANGES TO!"); }

					// Copy the meshes needed
					if(meshKitPrefabArray.Count > 0 ){
						foreach( MeshKitPrefabToSaveLater p in meshKitPrefabArray ){

							// Show Debug Info
							if( p != null && p.go != null &&
								// Make sure mesh filter is valid if it should be, or the skinnedMeshRenderer is
								( p.isMeshFilter == true && p.mf != null || p.isMeshFilter == false && p.smr != null  ) 
							){
								if(MeshKitGUI.verbose){ Debug.Log("MESHKIT: Attempting To Apply Prefab Changes to GameObject: "+ p.go ); }
							}

							// Save Prefab on each step
							if( p != null && p.go != null &&
								// Make sure mesh filter is valid if it should be, or the skinnedMeshRenderer is
								( p.isMeshFilter == true && p.mf != null || p.isMeshFilter == false && p.smr != null  ) &&
								
								// Make sure this is actually a prefab
								#if UNITY_2018_4_OR_NEWER

									// NOTE: All other options are prefabs in different states (but still prefabs!)
									PrefabUtility.GetPrefabInstanceStatus( MeshKitPrefabUtility.FindValidUploadPrefabInstanceRoot(p.go) ) != PrefabInstanceStatus.NotAPrefab &&

								#else	

									// Original Method
									PrefabUtility.GetPrefabType(MeshKitPrefabUtility.FindValidUploadPrefabInstanceRoot(p.go)) == PrefabType.PrefabInstance &&
								#endif

								// Make sure we can see this prefab's parent
								#if UNITY_2018_2_OR_NEWER
									PrefabUtility.GetCorrespondingObjectFromSource(p.go) != null
								#else
									PrefabUtility.GetPrefabParent(p.go) != null
								#endif
							){

								// Cache the prefab we want to replace
								#if UNITY_2018_2_OR_NEWER
									GameObject replacePrefabGameObject = PrefabUtility.GetCorrespondingObjectFromSource(p.go); 
								#else
									GameObject replacePrefabGameObject = (GameObject)PrefabUtility.GetPrefabParent(p.go);
								#endif

								// New Method
								#if UNITY_2018_4_OR_NEWER

									// NOTE:
									// We don't need to do anything on 2018.4. The prefab's mesh is updated but not
									// saved back to disk as the user is told to apply changed themselves. 
									// This is the standard (and safer) behaviour anyway.
									// Debug.LogWarning("DEBUG: Do Nothing On Unity 2018.4 and up.");

								// Older Method
								#else

									// This pretty much only applies to legacy prefab system...
									// Replace parent's prefab origin with new parent as a prefab
									PrefabUtility.ReplacePrefab( 	
																	p.go,
																	replacePrefabGameObject,
																	ReplacePrefabOptions.Default 
																);
								#endif

							} else { Debug.LogWarning("MESHKIT: Problem applying changes to Prefab.", p.go ); }

						}
					}

					// Reset Progress Bar
	 				progress = 0;
		 			maxProgress = 0;
	 				EditorUtility.ClearProgressBar();

	 				// Show success message
					EditorUtility.DisplayDialog("Fixing Prefabs Complete!", "The automated prefab fixer tool has just completed.\n\nYou can find your new mesh assets for these prefabs located in:\n\nAssets/MeshKit Prefab Meshes/\n\nIt would be a good idea to move them out of this folder and rename them as MeshKit is no longer managing these assets.\n\nLastly, don't forget to double check your prefabs and apply the changes!", "Okay");

					Debug.Log("MESHKIT: Find your new Prefab Assets here: Assets/MeshKit Prefab Meshes/");

				}
			}
		}



		////////////////////////////////////////////////////////////////////////////////////////////////
		//	KEEP MESHKIT FILES LOCAL
		//	Checks to see if any MeshFilters and Skinned Mesh Renderers are using Meshes that were created
		//	in a different scene. If we are, MeshKit allows us to fix it by copying local copies over 
		//	and replacing the meshes.
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static void KeepMeshKitFilesLocal(){

			// ===========================================
			//	CACHE ALL THE MESH FILTERS IN THE SCENE
			// ===========================================

			// Cache all the MeshFilters In This Scene
			MeshFilter[] allMeshfilters = Resources.FindObjectsOfTypeAll(typeof(MeshFilter)) as MeshFilter[];
			ArrayList filteredMFs = new ArrayList(); filteredMFs.Clear();
			foreach( MeshFilter mf in allMeshfilters){
				// If this MeshFilter is a weird internal object that cant be edited, dont count it ...
				if (mf.gameObject.hideFlags == HideFlags.NotEditable || mf.gameObject.hideFlags == HideFlags.HideAndDontSave){
            		continue;
				}
				// If this MeshFilter is attached to a GameOBject thats root exists in the Project pane, skip it ...
				string assetPath = AssetDatabase.GetAssetPath(mf.gameObject.transform.root.gameObject);
		        if (!string.IsNullOrEmpty(assetPath)){
		            continue;
		        }
		        // Otherwise, as long as this MeshFilter actually has a mesh, add it to the new list
		        if(mf.sharedMesh!=null){
		        	filteredMFs.Add(mf);
		    	}
			}
			MeshFilter[] meshfilters = (MeshFilter[])filteredMFs.ToArray(typeof(MeshFilter));
			if(MeshKitGUI.verbose){ Debug.Log(meshfilters.Length + " MeshFilters Found in this scene ( KeepMeshKitFilesLocal )"); }

			// ==================================================================================
			//	LOOP THROUGH ALL THE MESH FILTERS AND WORK OUT WHICH ONES WERE MADE WITH MESHKIT
			// ==================================================================================

			// Create a new array to store Meshes that were created by MeshKit.
			ArrayList mkMeshFilters = new ArrayList(); mkMeshFilters.Clear();
			foreach( MeshFilter mf2 in meshfilters ){

				if(MeshKitGUI.verbose){ Debug.Log( AssetDatabase.GetAssetPath(mf2.sharedMesh) ); }
				
				// If this Mesh is not null and is managed By MeshKit (dont ignore scene name)?
				if( mf2.sharedMesh != null && IsThisMeshManagedByMeshkit(mf2, false) == true ){
					mkMeshFilters.Add(mf2);
				}
			}


			// ===================================================
			//	CACHE ALL THE SKINNED MESH RENDERERS IN THE SCENE
			// ===================================================

			// Cache all the MeshFilters In This Scene
			SkinnedMeshRenderer[] allSkinnedMeshRenderers = Resources.FindObjectsOfTypeAll(typeof(SkinnedMeshRenderer)) as SkinnedMeshRenderer[];
			ArrayList filteredSMRs = new ArrayList(); filteredSMRs.Clear();
			foreach( SkinnedMeshRenderer smr in allSkinnedMeshRenderers){
				// If this SkinnedMeshRenderer is a weird internal object that cant be edited, dont count it ...
				if (smr.gameObject.hideFlags == HideFlags.NotEditable || smr.gameObject.hideFlags == HideFlags.HideAndDontSave){
            		continue;
				}
				// If this SkinnedMeshRenderer is attached to a GameOBject thats root exists in the Project pane, skip it ...
				string assetPath = AssetDatabase.GetAssetPath(smr.gameObject.transform.root.gameObject);
		        if (!string.IsNullOrEmpty(assetPath)){
		            continue;
		        }
		        // Otherwise, as long as this SkinnedMeshRenderer actually has a mesh, add it to the new list
		        if(smr.sharedMesh!=null){
		        	filteredSMRs.Add(smr);
		    	}
			}
			SkinnedMeshRenderer[] skinnedMeshRenderers = (SkinnedMeshRenderer[])filteredSMRs.ToArray(typeof(SkinnedMeshRenderer));
			if(MeshKitGUI.verbose){ Debug.Log(skinnedMeshRenderers.Length + " SkinnedMeshRenderers Found in this scene ( KeepMeshKitFilesLocal )"); }


			// ============================================================================================
			//	LOOP THROUGH ALL THE SKINNED MESH RENDERERS AND WORK OUT WHICH ONES WERE MADE WITH MESHKIT
			// ============================================================================================

			// Create a new array to store Meshes that were created by MeshKit.
			ArrayList mkSkinnedMeshRenderers = new ArrayList(); mkSkinnedMeshRenderers.Clear();
			foreach( SkinnedMeshRenderer smr2 in skinnedMeshRenderers ){

				if(MeshKitGUI.verbose){ Debug.Log( AssetDatabase.GetAssetPath(smr2.sharedMesh) ); }
				
				// If this Mesh is not null and is managed By MeshKit (dont ignore scene name)?
				if( smr2.sharedMesh != null && IsThisMeshManagedByMeshkit(smr2, false) == true ){
					mkSkinnedMeshRenderers.Add(smr2);
				}
			}
			

			// ============================================================================
			//	TELL THE USER WE CAN FIX IT
			// ============================================================================
			if( mkMeshFilters.Count > 0 || mkSkinnedMeshRenderers.Count > 0 ){

				// Ask user if they want to fix it or not
				if( EditorUtility.DisplayDialog("Shared MeshKit Assets Detected!", "IMPORTANT: MeshKit has detected that you are using " + (mkMeshFilters.Count + mkSkinnedMeshRenderers.Count).ToString() + " mesh/es that were created in another scene (this may be due to moving/duplicating a scene, or setting a MeshFilter or SkinnedMeshRenderer using a mesh built for a specific Unity Scene).\n\nTo prevent dataloss MeshKit can fix this by making copies of the originals and setting them up for this Unity scene automatically.\n\nWould you like MeshKit to fix this for you now (Recommended)?", "Yes", "No") ){

					// Setup Progress Bar
					maxProgress = mkMeshFilters.Count + mkSkinnedMeshRenderers.Count;
					progress = 0;

					// Setup a list of files that have been copied
					ArrayList copiedFiles = new ArrayList();
					copiedFiles.Clear();
				
					// Make sure we have support Folders created
					if( HaveSupportFoldersBeenCreated() ){

						// ====================
						//	FIX MESH FILTERS
						// ====================

						// Loop through the MeshKit MeshFilters
						foreach( MeshFilter mf3 in mkMeshFilters ){

							// Show Progress Bar
							if( maxProgress > 0 ){
								EditorUtility.DisplayProgressBar("Fixing Shared MeshKit Assets", "Creating and setting up new meshes ... ( "+ progress.ToString() + " / " + maxProgress.ToString() + " )", progress / maxProgress);
							} else {
								EditorUtility.ClearProgressBar();
							}

							// Get the directory of the asset and split it into sections
							string[] directorySectionsB = Path.GetDirectoryName( AssetDatabase.GetAssetPath(mf3.sharedMesh) ).Split('/');

							// Make sure there are at least 3 levels of directories
							if( directorySectionsB.Length >= 3 ){
							//	Debug.Log("1st Parent: "+ directorySectionsB[directorySectionsB.Length-1] );	// Combined Meshes, Inverted Meshes, etc.

								// Set up copy variables
								string copyFrom = AssetDatabase.GetAssetPath(mf3.sharedMesh);
								string copyTo = "";

								// Setup the correct path depending on where this Mesh was located
								if( directorySectionsB[directorySectionsB.Length-1] == "Seperated Meshes" ){
									copyTo = seperatedMeshFolder + Path.GetFileName(AssetDatabase.GetAssetPath(mf3.sharedMesh));
								}
								else if( directorySectionsB[directorySectionsB.Length-1] == "Combined Meshes" ){
									copyTo = combinedMeshFolder + Path.GetFileName(AssetDatabase.GetAssetPath(mf3.sharedMesh));
								}
								else if( directorySectionsB[directorySectionsB.Length-1] == "Inverted Meshes" ){
									copyTo = invertedMeshFolder + Path.GetFileName(AssetDatabase.GetAssetPath(mf3.sharedMesh));
								}
								else if( directorySectionsB[directorySectionsB.Length-1] == "DoubleSided Meshes" ){
									copyTo = doubleSidedMeshFolder + Path.GetFileName(AssetDatabase.GetAssetPath(mf3.sharedMesh));
								}
								else if( directorySectionsB[directorySectionsB.Length-1] == "Rebuilt Meshes" ){
									copyTo = rebuiltMeshFolder + Path.GetFileName(AssetDatabase.GetAssetPath(mf3.sharedMesh));
								}
								else if( directorySectionsB[directorySectionsB.Length-1] == "Decimated Meshes" ){
									copyTo = decimatedMeshFolder + Path.GetFileName(AssetDatabase.GetAssetPath(mf3.sharedMesh));
								}
								else if( directorySectionsB[directorySectionsB.Length-1] == "LOD Meshes" ){
									copyTo = lodMeshFolder + Path.GetFileName(AssetDatabase.GetAssetPath(mf3.sharedMesh));
								}

								// If this file hasn't already been copied, process it ... 
								// NOTE: copying over the same file twice messes everything up and causes missing meshes!
								if( MeshAssets.StringExistsInArrayList(copiedFiles, copyFrom) == false ){

									// If a file exists in the copy destination, delete it before copying the new one, otherwise
									// unity will display ghost copies in the Project view until you re-open the application.
									if( File.Exists( prefix+sceneParentFolder + sceneName + "/" +"MeshKit"+"/"+ directorySectionsB[directorySectionsB.Length-1]+"/"+Path.GetFileName( AssetDatabase.GetAssetPath(mf3.sharedMesh)) )
									){
										AssetDatabase.DeleteAsset(copyTo);
									}

									// Copy The File
									AssetDatabase.CopyAsset( copyFrom, copyTo );
									AssetDatabase.ImportAsset(copyTo);

									// Add this file to the ArrayList
									copiedFiles.Add(copyFrom);

								}

								// MESH COLLIDER FIX
								// Check to see if this mesh is using a mesh collider
								bool sameMeshIsUsedInMeshCollider = false;
								if( mf3.gameObject.GetComponent<MeshCollider>() != null &&
									mf3.sharedMesh == mf3.gameObject.GetComponent<MeshCollider>().sharedMesh
								){
									sameMeshIsUsedInMeshCollider = true;
								}

								// Once copied, attempt to load it back and set it as the new Shared Mesh.
								Mesh newMesh = AssetDatabase.LoadAssetAtPath(copyTo, typeof(Mesh) ) as Mesh;
								if( newMesh != null ){
									mf3.sharedMesh = newMesh;

									// Also update the Mesh Collider if needed ...
									if( sameMeshIsUsedInMeshCollider && mf3.gameObject.GetComponent<MeshCollider>() != null ){
										mf3.gameObject.GetComponent<MeshCollider>().sharedMesh = newMesh;
									}

								} else {
									Debug.LogWarning("MESHKIT: couldn't load Mesh at: "+ copyTo);
								}
							}

							// Increment progress
							progress++;
						}


						// ============================
						//	FIX SKINNED MESH RENDERERS
						// ============================

						// Loop through the MeshKit MeshFilters
						foreach( SkinnedMeshRenderer smr3 in mkSkinnedMeshRenderers ){

							// Show Progress Bar
							if( maxProgress > 0 ){
								EditorUtility.DisplayProgressBar("Fixing Shared MeshKit Assets", "Creating and setting up new meshes ... ( "+ progress.ToString() + " / " + maxProgress.ToString() + " )", progress / maxProgress);
							} else {
								EditorUtility.ClearProgressBar();
							}

							// Get the directory of the asset and split it into sections
							string[] directorySectionsB = Path.GetDirectoryName( AssetDatabase.GetAssetPath(smr3.sharedMesh) ).Split('/');

							// Make sure there are at least 3 levels of directories
							if( directorySectionsB.Length >= 3 ){
							//	Debug.Log("1st Parent: "+ directorySectionsB[directorySectionsB.Length-1] );	// Combined Meshes, Inverted Meshes, etc.

								// Set up copy variables
								string copyFrom = AssetDatabase.GetAssetPath(smr3.sharedMesh);
								string copyTo = "";

								// Setup the correct path depending on where this Mesh was located
								if( directorySectionsB[directorySectionsB.Length-1] == "Seperated Meshes" ){
									copyTo = seperatedMeshFolder + Path.GetFileName(AssetDatabase.GetAssetPath(smr3.sharedMesh));
								}
								else if( directorySectionsB[directorySectionsB.Length-1] == "Combined Meshes" ){
									copyTo = combinedMeshFolder + Path.GetFileName(AssetDatabase.GetAssetPath(smr3.sharedMesh));
								}
								else if( directorySectionsB[directorySectionsB.Length-1] == "Inverted Meshes" ){
									copyTo = invertedMeshFolder + Path.GetFileName(AssetDatabase.GetAssetPath(smr3.sharedMesh));
								}
								else if( directorySectionsB[directorySectionsB.Length-1] == "DoubleSided Meshes" ){
									copyTo = doubleSidedMeshFolder + Path.GetFileName(AssetDatabase.GetAssetPath(smr3.sharedMesh));
								}
								else if( directorySectionsB[directorySectionsB.Length-1] == "Rebuilt Meshes" ){
									copyTo = rebuiltMeshFolder + Path.GetFileName(AssetDatabase.GetAssetPath(smr3.sharedMesh));
								}
								else if( directorySectionsB[directorySectionsB.Length-1] == "Decimated Meshes" ){
									copyTo = decimatedMeshFolder + Path.GetFileName(AssetDatabase.GetAssetPath(smr3.sharedMesh));
								}
								else if( directorySectionsB[directorySectionsB.Length-1] == "LOD Meshes" ){
									copyTo = lodMeshFolder + Path.GetFileName(AssetDatabase.GetAssetPath(smr3.sharedMesh));
								}

								// If this file hasn't already been copied, process it ... 
								// NOTE: copying over the same file twice messes everything up and causes missing meshes!
								if( MeshAssets.StringExistsInArrayList(copiedFiles, copyFrom) == false ){

									// If a file exists in the copy destination, delete it before copying the new one, otherwise
									// unity will display ghost copies in the Project view until you re-open the application.
									if( File.Exists( prefix+sceneParentFolder + sceneName + "/" +"MeshKit"+"/"+ directorySectionsB[directorySectionsB.Length-1]+"/"+Path.GetFileName( AssetDatabase.GetAssetPath(smr3.sharedMesh)) )
									){
										AssetDatabase.DeleteAsset(copyTo);
									}

									// Copy The File
									AssetDatabase.CopyAsset( copyFrom, copyTo );
									AssetDatabase.ImportAsset(copyTo);

									// Add this file to the ArrayList
									copiedFiles.Add(copyFrom);

								}

								// MESH COLLIDER FIX
								// Check to see if this mesh is using a mesh collider
								bool sameMeshIsUsedInMeshCollider = false;
								if( smr3.gameObject.GetComponent<MeshCollider>() != null &&
									smr3.sharedMesh == smr3.gameObject.GetComponent<MeshCollider>().sharedMesh
								){
									sameMeshIsUsedInMeshCollider = true;
								}

								// Once copied, attempt to load it back and set it as the new Shared Mesh.
								Mesh newMesh = AssetDatabase.LoadAssetAtPath(copyTo, typeof(Mesh) ) as Mesh;
								if( newMesh != null ){
									smr3.sharedMesh = newMesh;

									// Also update the Mesh Collider if needed ...
									if( sameMeshIsUsedInMeshCollider && smr3.gameObject.GetComponent<MeshCollider>() != null ){
										smr3.gameObject.GetComponent<MeshCollider>().sharedMesh = newMesh;
									}

								} else {
									Debug.LogWarning("MESHKIT: couldn't load Mesh at: "+ copyTo);
								}
							}

							// Increment progress
							progress++;
						}
					}

					// Mark the scene as dirty so the user can save the changes
					HTScene.MarkSceneAsDirty();

					// Reset Progress Bar
	 				progress = 0;
		 			maxProgress = 0;
	 				EditorUtility.ClearProgressBar();

	 				// Show success message
					EditorUtility.DisplayDialog("Shared Meshes Copied!", "The automated Scene Fixer tool has completed copying the shared meshes. Now there shouldn't be any conflict between any of your scenes.\n\nPlease inspect the meshes of your scene to make sure everything looks fine. If it does, you should save this Unity Scene as soon as possible.\n\nIn the unlikely event something has gone wrong with this scene, you can always reload it (as it hasn't been saved yet), or rollback to the automated saved backup scene in the local MeshKit folder (if you allowed MeshKit to create one for you).", "Okay");
				}
			}	
		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	STRING EXISTS IN ARRAY LIST
		//	Does a string exist in an ArrayList?
		////////////////////////////////////////////////////////////////////////////////////////////////
		
		static bool StringExistsInArrayList( ArrayList arr, string compare ){

			// If the String exists in the ArrayList, return true
			if(arr.Count > 0){
				foreach( string s in arr ){
					if(s == compare){ return true; }
				}
			}

			// If this string does not exist, return false
			return false;
		}

	}
}

