////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MeshLOD.cs
//
//	Handles creating LOD setups in the Editor
//
//	© 2018 Melli Georgiou.
//	Hell Tap Entertainment LTD
//
////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

using HellTap.MeshDecimator.Unity;
using HellTap.MeshDecimator.Algorithms;

namespace HellTap.MeshKit {
	public class MeshLOD : UnityEditor.EditorWindow {

		// Helper Variables
		public static int currentAssetCount = 0;				// We set this using how many assets are in the Combined Mesh folder
		public static float progress = 0;
		public static float maxProgress = 0;

		// To Update Progress Bar
		void OnInspectorUpdate() { Repaint(); }	


		////////////////////////////////////////////////////////////////////////////////////////////////
		//	CREATE NEW MESHKIT AUTO LOD
		//	Adds the MeshKitAutoLOD component onto the GameObject
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static void CreateNewMeshKitAutoLOD( GameObject go, bool generateLODsRightAway ){

			Undo.SetCurrentGroupName("Setup Automatic LOD");
			Undo.AddComponent<MeshKitAutoLOD>(go);

			// Also Generate the LODs right away
			if( generateLODsRightAway && go.GetComponent<MeshKitAutoLOD>() != null ){
				MeshLOD.StartGenerateLOD( go.GetComponent<MeshKitAutoLOD>() );
			}
			
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		//	START LOD
		//	Allows us to generate LOD
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static void StartGenerateLOD( MeshKitAutoLOD target ){

			// Setup Support Folders and then cache the current number of Assets in the Combined Mesh Folder
			MeshAssets.HaveSupportFoldersBeenCreated();
			currentAssetCount = 0;
			currentAssetCount = MeshAssets.HowManyFilesAtAssetPath(MeshAssets.decimatedMeshFolder);

			// ===========================
			//	MAIN STUFF ...
			// ===========================

			// Generate the LOD
			GenerateLODs( target );

			
			// =====================
			//	END
			// =====================

			/*
			// No meshes were found
			if( maxProgress == 0 ){

				if(MeshKitGUI.verbose){ Debug.Log("No LODs can be created on this GameObject."); }
				EditorUtility.DisplayDialog("Mesh LOD", "No LODs can be created on this GameObject.", "Okay");
			}
			*/

			// Stop Progress Bar
			maxProgress = 0;
			progress = 0;
			EditorUtility.ClearProgressBar();


		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		//	GENERATE LODs
		//	Allows us to generate LOD
		////////////////////////////////////////////////////////////////////////////////////////////////

		// Method
		private static void GenerateLODs( MeshKitAutoLOD target ){

			// Save the current GameObject
			// Undo
			Undo.SetCurrentGroupName("Generate LODs");
			Undo.RecordObject ( target.gameObject, "Generate LODs");

			EditorUtility.DisplayProgressBar("Generating LODs", "Preparations...", 0f);
			try {
				int levelCount = target.Levels.Length;
				target.GenerateLODs((lodLevel, iteration, originalTris, currentTris, targetTris) =>
				{
					float reduction = (1f - ((float)currentTris / (float)originalTris)) * 100f;
					string statusText;
					if (targetTris >= 0)
					{
						//statusText = string.Format("Level {0}/{1}, triangles {2}/{3} ({4:0.00}% reduction), target {5}", lodLevel + 1, levelCount, currentTris, originalTris, reduction, targetTris);

						statusText = string.Format("Level {0}/{1}, triangles {2}/{3} ({4:0.00}% reduction)", lodLevel + 1, levelCount, currentTris, originalTris, reduction);
					}
					else
					{
						statusText = string.Format("Level {0}/{1}, triangles {2}/{3} ({4:0.00}% reduction)", lodLevel + 1, levelCount, currentTris, originalTris, reduction);
					}
					float progress = (float)lodLevel / (float)levelCount;
					EditorUtility.DisplayProgressBar("Generating LODs", statusText, progress);
				});
				EditorUtility.ClearProgressBar();

				// Have MeshKit create and update the real mesh assets
				CreateMeshKitAssetsForLODs( target );

			// Clear Progress bar when done	
			} finally {
				EditorUtility.ClearProgressBar();
			}

			target.generated = true;

			// Mark Scene as dirty
			HTScene.MarkSceneAsDirty();
		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	CREATE MESH KIT ASSETS FOR LODS
		//	After the LODs are created with meshes in memory, MeshKit can save them into Assets and
		//	re-apply them
		////////////////////////////////////////////////////////////////////////////////////////////////

		private static void CreateMeshKitAssetsForLODs( MeshKitAutoLOD target ){

			// Make sure the MeshKit Auto LOD has created a LODGroup component
			if( target != null && target.gameObject.GetComponent<LODGroup>() != null ){

				// Cache the LOD Group
				LOD[] lods = target.gameObject.GetComponent<LODGroup>().GetLODs();

				//Debug.Log("LOD Group Cached: " + lods.Length );

				// ==========================================
				//	CACHE THE NAME OF THE PRIMARY MESH (LOD0)
				// ==========================================

				// Main Mesh Name
				string originalMeshName = "Mesh";
				if( lods[0].renderers[0] != null ){
					if( lods[0].renderers[0].GetComponent<MeshFilter>() != null &&
						lods[0].renderers[0].GetComponent<MeshFilter>().sharedMesh != null
					){ 
						originalMeshName = lods[0].renderers[0].GetComponent<MeshFilter>().sharedMesh.name;

					} else if(	lods[0].renderers[0].GetComponent<SkinnedMeshRenderer>() != null &&
								lods[0].renderers[0].GetComponent<SkinnedMeshRenderer>().sharedMesh != null
					){ 
						originalMeshName = lods[0].renderers[0].GetComponent<SkinnedMeshRenderer>().sharedMesh.name;
					}
				}

				// =========================================
				//	LOOP THROUGH THE LODS AND CREATE ASSETS
				// =========================================

				// Loop through each of the LODs, making sure it is valid and has renderers
				for( int i = 1; i < lods.Length; i++ ){	// <- Skip LOD0 because that is the primary mesh!
					if( lods[i].renderers != null ){

						EditorUtility.DisplayProgressBar("Generating LODs", "Creating MeshKit Assets For LOD" + i.ToString(), (float)i / (float)lods.Length);

						// Loop through each renderer of this LOD, making sure it is valid
						for( int r = 0; r < lods[i].renderers.Length; r++ ){
							if( lods[i].renderers[r] != null ){


								// Helper variables
								Mesh meshToConvert = null;
								MeshFilter mf = null;
								SkinnedMeshRenderer smr = null;

								// ==============================================================
								//	FIGURE OUT IF MESH IF ON MESHFILTER OR SKINNED MESH RENDERER
								// ==============================================================

								// Check if this Renderer also has a MeshFilter on the GameObject
								if( lods[i].renderers[r].GetComponent<MeshFilter>() != null ){
								//	Debug.Log("Found MeshFilter!");
									mf = lods[i].renderers[r].GetComponent<MeshFilter>();
									meshToConvert = mf.sharedMesh;
								}

								// Check if this Renderer is actually a SkinnedMeshRenderer
								else if( lods[i].renderers[r].GetComponent<SkinnedMeshRenderer>() != null ){
								//	Debug.Log("Found Skinned Mesh Renderer!");
									smr = lods[i].renderers[r].GetComponent<SkinnedMeshRenderer>();
									meshToConvert = smr.sharedMesh;
								}

								// ======================================
								//	CREATE MESHKIT ASSETS FOR LOD MESHES
								// ======================================

								// If the mesh was decimated successfully, attempt to create an asset out of the mesh
								if( meshToConvert != null ){

									// Try to create the asset
									Mesh newMesh = MeshAssets.CreateMeshAsset( meshToConvert, MeshAssets.ProcessFileName(MeshAssets.lodMeshFolder, originalMeshName, "AutoLOD"+ i.ToString(), false) );

									// If the newMesh was crated succesfully ...
									if(newMesh!=null){
										
										if(mf!=null){ mf.sharedMesh = newMesh; }
										if(smr!=null){ smr.sharedMesh = newMesh; }

									} else {
										Debug.LogWarning("MESH KIT LOD: Couldn't Create MeshKit Asset For GameObject: "+ lods[i].renderers[r].gameObject.name );
									}

								} else {

									// Warning Logs
									Debug.LogWarning("MESH KIT LOD: No Mesh Was Found To Convert On GameObject: "+ lods[i].renderers[r].gameObject.name );
								}



							}
						}

					}
				}
			}

		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	RESET LODs
		//	Allows us to Reset the LODs
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static void ResetLODs( MeshKitAutoLOD target )
		{
			target.generated = false;

			// Reset LODs (removes the LOD meshes and LODGroup)
			target.ResetLODs();

			// Mark Scene as dirty
			HTScene.MarkSceneAsDirty();
		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	COMPLETELY REMOVE LOD SYSTEM AND COMPONENTS
		//	Removes the LOD system by resetting LODs and removing any LODGroup and MeshKitAutoLOD.
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static void CompletelyRemoveLODSystemAndComponents( MeshKitAutoLOD target, string undoName ){
			
			// Make sure the target is valid
			if( target != null ){
			
				// Cache GameObject
				GameObject go = target.gameObject;

				Undo.SetCurrentGroupName( undoName );

				// Reset the LODs
				ResetLODs( target );

				// Remove the LODGroup
				if( go.GetComponent<LODGroup>() != null ){ Undo.DestroyObjectImmediate ( go.GetComponent<LODGroup>() ); }

				// Remove the MeshKitAutoLOD
				if( target != null ){ Undo.DestroyObjectImmediate ( target ); }
			}
		}

	}
}