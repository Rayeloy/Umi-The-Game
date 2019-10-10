////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MeshDecimation.cs
//
//	Decimates Meshes of all children objects in the Editor
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

// Use HellTap Namespace
namespace HellTap.MeshKit {
	public class MeshDecimation : UnityEditor.EditorWindow {

		// Helper Variables
		public static int currentAssetCount = 0;				// We set this using how many assets are in the Combined Mesh folder
		public static float progress = 0;
	    public static float maxProgress = 0;

		// To Update Progress Bar
	    void OnInspectorUpdate() { Repaint(); }	

	    ////////////////////////////////////////////////////////////////////////////////////////////////
		//	START DECIMATION
		//	This method begins the batch decimation process
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static void StartDecimation( GameObject go, bool recursive, bool useMeshFilters, bool useSkinnedMeshRenderers,bool enabledRenderersOnly, float quality, bool recalculateNormals, bool preserveBorders = false, bool preserveSeams = false, bool preserveFoldovers = false ){

			// Setup Support Folders and then cache the current number of Assets in the Combined Mesh Folder
			MeshAssets.HaveSupportFoldersBeenCreated();
			currentAssetCount = 0;
			currentAssetCount = MeshAssets.HowManyFilesAtAssetPath(MeshAssets.decimatedMeshFolder);

			// =====================
			//	MAIN ROUTINE
			// =====================

			// Placeholder variables for recursive actions
			MeshFilter[] meshfilters = new MeshFilter[0];
			SkinnedMeshRenderer[] skinnedMeshRenderers = new SkinnedMeshRenderer[0];

			// If we're decimating the child objects, cache the components and tally up the max Progress
			if( recursive ){

				// Get all the MeshFilters and SkinnedMeshRenderers in this object and its children
				if(useMeshFilters){ meshfilters = go.GetComponentsInChildren<MeshFilter>() as MeshFilter[]; }
				if(useSkinnedMeshRenderers){ skinnedMeshRenderers = go.GetComponentsInChildren<SkinnedMeshRenderer>() as SkinnedMeshRenderer[]; }
				
				// Tally up the max progress
				maxProgress = meshfilters.Length + skinnedMeshRenderers.Length;

			} else {
				maxProgress = 0;
			}

			// Setup Progress Bar
			progress = 0;


			// ==================================
			//	APPLY TO THIS OBJECT ONLY
			// ==================================

			// Apply to this object only
			if( recursive == false ){

				// Make sure this object has a MeshFilter/SkinnedMeshRenderer and a mesh
				if(
					(
						useMeshFilters && go.GetComponent<MeshFilter>()!=null && go.GetComponent<MeshFilter>().sharedMesh != null ||
						useSkinnedMeshRenderers && go.GetComponent<SkinnedMeshRenderer>()!=null && go.GetComponent<SkinnedMeshRenderer>().sharedMesh
					
					) && (

						enabledRenderersOnly == false ||
						enabledRenderersOnly && go.GetComponent<Renderer>() != null && go.GetComponent<Renderer>().enabled
					) 
				){

					// Set Max Progress to 1
					maxProgress = 1;

					// Show Progress Bar
					if( maxProgress > 0 ){
						EditorUtility.DisplayProgressBar("Decimating Mesh", "Decimating ...", 1.0f );
					} else {
						EditorUtility.ClearProgressBar();
					}
					
					// Check for Skinned Mesh Renderer and make sure its valid
					if( go.GetComponent<SkinnedMeshRenderer>() != null && 
						( enabledRenderersOnly == false || enabledRenderersOnly && go.GetComponent<SkinnedMeshRenderer>().enabled )
					){

						// Undo
						Undo.RecordObject ( go.GetComponent<SkinnedMeshRenderer>(), "MeshKit (Decimate)");

						// Run Process
						DecimateMesh( 
							go.GetComponent<SkinnedMeshRenderer>(), quality, recalculateNormals, preserveBorders, preserveSeams, preserveFoldovers
						);
					
					// Check for Mesh Filter
					} else if( go.GetComponent<MeshFilter>() != null && 
						( enabledRenderersOnly == false || enabledRenderersOnly && go.GetComponent<Renderer>().enabled )
					){

						// Undo
						Undo.RecordObject ( go.GetComponent<MeshFilter>(), "MeshKit (Decimate)");

						// Run Process
						DecimateMesh( 
							go.GetComponent<MeshFilter>(), quality, recalculateNormals, preserveBorders, preserveSeams, preserveFoldovers
						);
					}

				
				// If this object doesn't have a mesh, show it in the console	
				} else { Debug.Log( "MESHKIT: The GameObject "+go.name+" does not have a Mesh."); }

			// ==================================
			//	APPLY TO THIS OBJECT AND CHILDREN
			// ==================================

			} else {

				// -------------
				//	MESHFILTERS
				// -------------

				// Loop through the MeshFilters
				if(meshfilters.Length > 0){
					foreach( MeshFilter mf in meshfilters ){
						if( mf!=null && mf.sharedMesh != null && 
							(	enabledRenderersOnly == false ||
								enabledRenderersOnly && mf.gameObject.GetComponent<Renderer>() != null && mf.gameObject.GetComponent<Renderer>().enabled
							)
						 ){

						 	// Show Progress Bar
							if( maxProgress > 0 ){
								EditorUtility.DisplayProgressBar("Decimating Mesh", "Decimating MeshFilters ...", progress / maxProgress );
							} else {
								EditorUtility.ClearProgressBar();
							}

							// Undo
							Undo.RecordObject ( mf, "MeshKit (Decimate)");

						 	// Run Process (one mesh per frame)
							DecimateMesh( 
								mf, quality, recalculateNormals, preserveBorders, preserveSeams, preserveFoldovers
							);

							// Increment Progress
							progress++;
						}
					}
				}

				// ------------------------
				//	SKINNED MESH RENDERERS
				// ------------------------

				// Loop through the MeshFilters
				if(skinnedMeshRenderers.Length > 0){
					foreach( SkinnedMeshRenderer smr in skinnedMeshRenderers ){
						if( smr!=null && smr.sharedMesh != null && 
							(	enabledRenderersOnly == false ||
								enabledRenderersOnly && smr.enabled == true
							)
						 ){

						 	// Show Progress Bar
							if( maxProgress > 0 ){
								EditorUtility.DisplayProgressBar("Decimating Mesh", "Decimating SkinnedMeshRenderers ...", progress / maxProgress );
							} else {
								EditorUtility.ClearProgressBar();
							}

							// Undo
							Undo.RecordObject ( smr, "MeshKit (Decimate)");	

						 	// Run Process (one mesh per frame)
							DecimateMesh( 
								smr, quality, recalculateNormals, preserveBorders, preserveSeams, preserveFoldovers
							);

							// Increment Progress
							progress++;
						}
					}
				}
			}

			// No meshes were found
			if( maxProgress == 0 ){

				if(MeshKitGUI.verbose){ Debug.Log("No meshes can be decimated in this group."); }
				EditorUtility.DisplayDialog("Mesh Decimation", "No valid MeshFilters or SkinnedMeshRenderers can be decimated in this group.", "Okay");
			}

			// Stop Progress Bar
			maxProgress = 0;
			progress = 0;
			EditorUtility.ClearProgressBar();


		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		//	DECIMATE SKINNED MESH
		//	Decimates a mesh on a Skinned Mesh Renderer
		////////////////////////////////////////////////////////////////////////////////////////////////

		// Decimate A Skinned Mesh
		public static Mesh DecimateMesh( SkinnedMeshRenderer smr, float quality, bool recalculateNormals, bool preserveBorders = false, bool preserveSeams = false, bool preserveFoldovers = false ){
			if( smr != null && smr.sharedMesh != null ){

				// Helper variables
				var rendererTransform = smr.transform;
				var mesh = smr.sharedMesh;
				var meshTransform = smr.transform.worldToLocalMatrix * rendererTransform.localToWorldMatrix;
				quality = Mathf.Clamp01(quality);

				// Decimate the mesh
				Mesh decimatedMesh = MeshDecimatorUtility.DecimateMesh(mesh, meshTransform, quality, recalculateNormals, null/*statusCallback*/, preserveBorders, preserveSeams, preserveFoldovers);
				
				// If the mesh was decimated successfully, attempt to create an asset out of the mesh
				if( decimatedMesh != null ){

					// Fix Mesh Name
					decimatedMesh.name.MakeFileSystemSafe();

					// Try to create the asset
					Mesh newMesh = MeshAssets.CreateMeshAsset( decimatedMesh, MeshAssets.ProcessFileName(MeshAssets.decimatedMeshFolder, smr.gameObject.name, "Decimated", false) );

					// If the newMesh was crated succesfully ...
					if(newMesh!=null){
						
						smr.sharedMesh = newMesh;
						return newMesh;

					} else {
						Debug.Log("MESH KIT DECIMATION: Couldn't Decimate Mesh on GameObject: "+ smr.gameObject.name);
						return null;
					}

				} else {

					// Warning Logs
					Debug.LogWarning("MESH KIT DECIMATION: A skinned mesh couldn't be decimated. Skipping ..." );
					return null;
				}	
			}

			// Warning Logs
			Debug.LogWarning("MESH KIT DECIMATION: SkinnedMeshRenderer or its shared mesh was null. Skipping ..." );

			// Otherwise, return null
			return null;
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		//	DECIMATE MESH FILTER MESH
		//	Decimates a mesh on a Mesh Filter
		////////////////////////////////////////////////////////////////////////////////////////////////

		// Decimate A MeshFilter Mesh
		public static Mesh DecimateMesh( MeshFilter mf, float quality, bool recalculateNormals, bool preserveBorders = false, bool preserveSeams = false, bool preserveFoldovers = false ){
			if( mf != null && mf.sharedMesh != null ){

				// Settings
				//FastQuadricMeshSimplification.PreserveBorders = true;

				// Helper variables
				var rendererTransform = mf.transform;
				var mesh = mf.sharedMesh;
				var meshTransform = mf.transform.worldToLocalMatrix * rendererTransform.localToWorldMatrix;
				quality = Mathf.Clamp01(quality);

				// Decimate the mesh
				Mesh decimatedMesh = MeshDecimatorUtility.DecimateMesh(mesh, meshTransform, quality, recalculateNormals, null/*statusCallback*/, preserveBorders, preserveSeams, preserveFoldovers);
				
				// If the mesh was decimated successfully, attempt to create an asset out of the mesh
				if( decimatedMesh != null ){

					// Fix Mesh Name
					decimatedMesh.name.MakeFileSystemSafe();

					// Try to create the asset
					Mesh newMesh = MeshAssets.CreateMeshAsset( decimatedMesh, MeshAssets.ProcessFileName(MeshAssets.decimatedMeshFolder, mf.gameObject.name, "Decimated", false) );

					// If the newMesh was crated succesfully ...
					if(newMesh!=null){
						
						mf.sharedMesh = newMesh;
						return newMesh;

					} else {
						Debug.Log("MESH KIT DECIMATION: Couldn't Decimate Mesh on GameObject: "+ mf.gameObject.name);
						return null;
					}

				} else {

					// Warning Logs
					Debug.LogWarning("MESH KIT DECIMATION: A mesh couldn't be decimated. Skipping ..." );
					return null;
				}	
			}

			// Warning Logs
			Debug.LogWarning("MESH KIT DECIMATION: MeshFilter or its shared mesh was null. Skipping ..." );

			// Otherwise, return null
			return null;
		}


	}
}