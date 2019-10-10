////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MeshCombiner.cs
//
//	Combines Meshes of all children objects in the Editor
//
//	© 2015 Melli Georgiou.
//	Hell Tap Entertainment LTD
//
////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using HellTap.MeshKit;
//using System.Linq;	// <- we need to get rid of this later
//using System;
//using System.Reflection;

// Use HellTap Namespace
namespace HellTap.MeshKit {

	// Class
	public class MeshCombiner : EditorWindow {

		// Constants
		public const int maxVertices = 65534;					// Unity's upper limit of vertices that can be combined

		// Helper Variables
		public static int currentAssetCount = 0;				// We set this using how many assets are in the Combined Mesh folder
		public static float progress = 0;
		public static float maxProgress = 0;

		// shared Variables
		public static ArrayList renderersThatWereDisabled;
		public static ArrayList createdCombinedMeshObjects;

		// To Update Progress Bar
		void OnInspectorUpdate() { Repaint(); }	

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	START COMBINE
		//	We create the new Meshes at start
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static void StartCombine( Transform theTransform, bool optimizeMeshes, int createNewObjectsWithLayer, string createNewObjectsWithTag, bool enabledRenderersOnly, bool deleteSourceObjects, bool createNewObjectsWithMeshColliders, bool deleteObjectsWithDisabledRenderers, bool deleteEmptyObjects, int userMaxVertices = maxVertices, bool useMeshFilters = true, bool useSkinnedMeshRenderers = false, bool optionRestoreBindposes = false ){
			
			// Combine Mesh Filters
			bool meshFiltersWereCombined = false;
			if( useMeshFilters == true ){

				// Setup Support Folders and then cache the current number of Assets in the Combined Mesh Folder
				MeshAssets.HaveSupportFoldersBeenCreated();
				currentAssetCount = 0;
				currentAssetCount = MeshAssets.HowManyFilesAtAssetPath(MeshAssets.combinedMeshFolder);

				// =====================
				//	MAIN ROUTINE
				// =====================

				// Setup Progress Bar
				maxProgress = 4;
				progress = 0;

				// Make sure userMaxVertices doesn't exceed the actual maxVertices
				if(userMaxVertices > maxVertices ){ 
					Debug.Log("MESHKIT: Maximum vertices cannot be higher than " + maxVertices + ". Your settings have been changed.");
					userMaxVertices = maxVertices; 
				}

				// Prepare variables to store the new meshes, materials, etc.
				Matrix4x4 myTransform = theTransform.worldToLocalMatrix;
				Dictionary<Material, List<CombineInstance>> combines = new Dictionary<Material, List<CombineInstance>>();
				MeshRenderer[] meshRenderers = theTransform.GetComponentsInChildren<MeshRenderer>();

				// Track Renderers That Are Disabled
				renderersThatWereDisabled = new ArrayList();
				renderersThatWereDisabled.Clear();

				// Loop through the MeshRenderers inside of this group ...
				foreach (var meshRenderer in meshRenderers){

					// Show Progress Bar
					if( maxProgress > 0 ){
						EditorUtility.DisplayProgressBar("Combining Mesh", "Preparing MeshRenderers ...", 1.0f / 3.0f);
					} else {
						EditorUtility.ClearProgressBar();
					}

					// Make sure the MeshRenderers we are checking are valid!
					if(meshRenderer!=null){

						// Only combine meshes that have a single subMesh and have MeshRenderers that are enabled!
						if( meshRenderer.gameObject.GetComponent<MeshFilter>() != null &&
							meshRenderer.gameObject.GetComponent<MeshFilter>().sharedMesh != null &&
							meshRenderer.gameObject.GetComponent<MeshFilter>().sharedMesh.subMeshCount == 1 &&
							( !enabledRenderersOnly || meshRenderer.enabled == true )
						){

							// Loop through the materials of each renderer ...
							foreach (var material in meshRenderer.sharedMaterials){

								// If the material doesn't exist in the "combines" list then add it
								if (material != null && !combines.ContainsKey(material)){
									combines.Add(material, new List<CombineInstance>());
								}
							}
						}
					}
				}

				// Loop through the MeshFilters inside of this group ...
				int howManyMeshCombinationsWereThere = 0;
				MeshFilter[] meshFilters = theTransform.GetComponentsInChildren<MeshFilter>();
				MeshFilter[] compatibleMeshFilters = new MeshFilter[0];

				// Loop through the MeshFilters to see which ones are compatible
				foreach(var filter in meshFilters){

					// Show Progress Bar
					if( maxProgress > 0 ){
						EditorUtility.DisplayProgressBar("Combining Mesh", "Analyzing Data From Mesh Filters ...", 2.0f / 3.0f);
					} else {
						EditorUtility.ClearProgressBar();
					}

					// If there isn't a mesh applied, skip it..
					if (filter.sharedMesh == null){
						continue;
					}

					// If this is connected to a MeshRenderer that is disabled ...
					else if (filter.gameObject.GetComponent<MeshRenderer>() != null &&
						enabledRenderersOnly && filter.gameObject.GetComponent<MeshRenderer>().enabled == false
					){
						continue;
					}

					// Make sure it doesn't have too many vertices
					else if ( filter.sharedMesh.vertexCount >= userMaxVertices ){
						continue;
					}

					// If this mesh has subMeshes, skip it..
					else if ( filter.sharedMesh.subMeshCount > 1){
						continue;
					} else {

						// Add this to the compatibleMeshFilters array
						Arrays.AddItemFastest( ref compatibleMeshFilters, filter );

						// Combine The Meshes
						CombineInstance ci = new CombineInstance();
						ci.mesh = filter.sharedMesh;
						#if !UNITY_5_5_OR_NEWER
							// Run Legacy optimization. This apparantly isn't needed anymore after Unity 5.5.
							if(optimizeMeshes){ ci.mesh.Optimize(); }
						#endif

						ci.transform = myTransform * filter.transform.localToWorldMatrix;

						// Make sure the current filter has a Renderer and that Renderer has a sharedMaterial
						if( filter.GetComponent<Renderer>() != null && filter.GetComponent<Renderer>().sharedMaterial != null ){
							combines[filter.GetComponent<Renderer>().sharedMaterial].Add(ci);

							// Turn off the original renderer
							Undo.RecordObject ( filter.GetComponent<Renderer>(), "MeshKit (Combine)");
							renderersThatWereDisabled.Add(filter.GetComponent<Renderer>());
							filter.GetComponent<Renderer>().enabled = false;

							// Increment how many mesh combinations there were
							howManyMeshCombinationsWereThere++;
						}
					}	
				}

				// After we've sorted out MeshFilters, replace the original array with the updated one
				//Debug.Log( "Total MeshFilters: " + meshFilters.Length );
				//Debug.Log( "Compatible MeshFilters: " + compatibleMeshFilters.Length );
				meshFilters = compatibleMeshFilters;

				// Create The Combined Meshes only if there are more materials then meshes
				if(MeshKitGUI.verbose){ Debug.Log("Mesh Combinations: "+ howManyMeshCombinationsWereThere); }
				if(MeshKitGUI.verbose){ Debug.Log("Material Count: "+ combines.Keys.Count); }
				if( combines.Keys.Count < howManyMeshCombinationsWereThere ){

					// Loop through the materials in the "combine" list ...
					float localProgressCount = 0f;
					float localCombineKeyCount = combines.Keys.Count;
					foreach(Material m in combines.Keys){

						// Show Progress Bar
						localProgressCount++;
						if( maxProgress > 0 ){
							EditorUtility.DisplayProgressBar("Combining Mesh", "Building New Combined Meshes ...", localProgressCount /localCombineKeyCount );
						} else {
							EditorUtility.ClearProgressBar();
						}

						// increment the number of assets
						currentAssetCount++;

						// NOTE: We should try to scan the size of these meshes before combining them to make sure they are within the limit
						float totalVertsCount = 0f;
						foreach( CombineInstance countCI in combines[m].ToArray() ){
							totalVertsCount += countCI.mesh.vertexCount;
						}

						// If there are less than 64k meshes, we can create the new GameObject normally ...
						if( totalVertsCount < userMaxVertices ){

							// Create a new GameObject based on the name of the material
							GameObject go = new GameObject(currentAssetCount.ToString("D4")+"_combined_" + m.name + "  ["+m.shader.name+"]");
							go.transform.parent = theTransform;
							go.transform.localPosition = Vector3.zero;
							go.transform.localRotation = Quaternion.identity;
							go.transform.localScale = Vector3.one;
							
							// Create a combined mesh using a new variable (avoids errors!)
							Mesh newMesh = new Mesh();
							newMesh.Clear();
							newMesh.name = currentAssetCount.ToString("D4")+"_combined_" + m.name + "  ["+m.shader.name+"]";
							newMesh.CombineMeshes(combines[m].ToArray(), true, true );

							// Create An Array to track which new GameObjects were created (so we can destroy them if something goes wrong)
							createdCombinedMeshObjects = new ArrayList();
							createdCombinedMeshObjects.Clear();

							// Save Combined Mesh and handle errors in a seperate function
							SaveCombinedMesh( go, newMesh, m, createNewObjectsWithLayer, createNewObjectsWithTag, createNewObjectsWithMeshColliders, 0 ); // When creating assets normally, we use 0 as the group int.

						// Otherwise we need to break apart this CombinedInstance and create seperate ones within the limits.
						} else {

							// Debug Message
							if(MeshKitGUI.verbose){ Debug.Log("MESHKIT: Too many verts detected! Attempting To divide combined mesh for material \""+ m.name +"\" ..."); }

							// Count the index, the arrays created, the current total verts, and also a new combineInstance Array to build new combines from this oversized one ...
							int i = 0;
							int arraysCreated = 0;
							float currentVertsCount = 0f;
							ArrayList newCombineInstance = new ArrayList(); 
							newCombineInstance.Clear();

							// Loop through each of the CombineInstances and create new ones
							foreach( CombineInstance countCI in combines[m].ToArray() ){

								// --------------------------------------------------
								// BUILD A NEW COMBINED MESH BEFORE IT GETS TOO BIG
								// --------------------------------------------------

								// Will adding the new mesh make this combineInstance too large? AND -
								// If there is at least 1 mesh in this group, we should build it now before it gets too large.
								if( currentVertsCount + countCI.mesh.vertexCount >= userMaxVertices &&
									newCombineInstance.Count > 0 
								){
									// Create a new GameObject based on the name of the material
									GameObject go = new GameObject(currentAssetCount.ToString("D4")+"_combined_" + m.name + "_"+arraysCreated.ToString() +"  ["+m.shader.name+"]");
									go.transform.parent = theTransform;
									go.transform.localPosition = Vector3.zero;
									go.transform.localRotation = Quaternion.identity;
									go.transform.localScale = Vector3.one;

									// Create a combined mesh using a new variable (avoids errors!)
									Mesh newMesh = new Mesh();
									newMesh.Clear();
									newMesh.name = currentAssetCount.ToString("D4")+"_combined_" + m.name + "_" + arraysCreated.ToString() + "  ["+m.shader.name+"]";

									// Convert the CombineInstance into the builtin array and combine the meshes
									CombineInstance[] newCombineArr = (CombineInstance[]) newCombineInstance.ToArray( typeof( CombineInstance ) );
									newMesh.CombineMeshes( newCombineArr, true, true );

									// Create An Array to track which new GameObjects were created (so we can destroy them if something goes wrong)
									createdCombinedMeshObjects = new ArrayList();
									createdCombinedMeshObjects.Clear();

									// Save Combined Mesh and handle errors in a seperate function
									SaveCombinedMesh( go, newMesh, m, createNewObjectsWithLayer, createNewObjectsWithTag, createNewObjectsWithMeshColliders, arraysCreated ); // When creating assets normally, we use 0 as the group int.

									// Reset The array to build the next group
									currentVertsCount = 0f;
									newCombineInstance = new ArrayList(); 
									newCombineInstance.Clear();

									// Increment the array count
									arraysCreated++;
								}

								// ----------------------------------------------
								// ADD THE NEW MESH TO THE ARRAY IF THERE IS ROOM
								// ----------------------------------------------

								// If theres space, add it to the group
								if( currentVertsCount + countCI.mesh.vertexCount < userMaxVertices ){

									// Add this CombineInstance into the new array...
									newCombineInstance.Add(countCI);

									// Update the total vertices so far ...
									currentVertsCount += countCI.mesh.vertexCount;

									// If this is the last loop - we should build the mesh here.
									if( i == combines[m].Count - 1 ){

										// Create a new GameObject based on the name of the material
										GameObject go2 = new GameObject(currentAssetCount.ToString("D4")+"_combined_" + m.name + "_"+arraysCreated.ToString() +"  ["+m.shader.name+"]");
										go2.transform.parent = theTransform;
										go2.transform.localPosition = Vector3.zero;
										go2.transform.localRotation = Quaternion.identity;
										go2.transform.localScale = Vector3.one;

										// Create a combined mesh using a new variable (avoids errors!)
										Mesh newMesh = new Mesh();
										newMesh.Clear();
										newMesh.name = currentAssetCount.ToString("D4")+"_combined_" + m.name + "_" + arraysCreated.ToString() + "  ["+m.shader.name+"]";

										// Convert the CombineInstance into the builtin array and combine the meshes
										CombineInstance[] newCombineArr = (CombineInstance[]) newCombineInstance.ToArray( typeof( CombineInstance ) );
										newMesh.CombineMeshes( newCombineArr, true, true );

										// Create An Array to track which new GameObjects were created (so we can destroy them if something goes wrong)
										createdCombinedMeshObjects = new ArrayList();
										createdCombinedMeshObjects.Clear();

										// Save Combined Mesh and handle errors in a seperate function
										SaveCombinedMesh( go2, newMesh, m, createNewObjectsWithLayer, createNewObjectsWithTag, createNewObjectsWithMeshColliders, arraysCreated ); // When creating assets normally, we use 0 as the group int.

									}
								}

								// ----------------------------------------------
								// IF A SINGLE MESH IS TOO BIG TO BE ADDED
								// ----------------------------------------------

								else if( countCI.mesh.vertexCount >= maxVertices ){
									// Show warnings for meshes that are too large
									Debug.LogWarning("MESHKIT: MeshKit detected a Mesh called \"" + countCI.mesh.name + "\" with "+ countCI.mesh.vertexCount + " vertices using the material \""+ m.name + "\". This is beyond Unity's limitations and cannot be combined. This mesh was skipped.");
								}
					
								// Update index
								i++;
									
							}
						}
					}

					// =====================
					// 	CLEANUP
					// =====================
					
					// If we have chosen to delete the source objects ...
					if(deleteSourceObjects){

						// Show Progress Bar
						if( maxProgress > 0 ){
							EditorUtility.DisplayProgressBar("Combining Mesh", "Cleaning Up ...", 3.0f / 3.0f);
						} else {
							EditorUtility.ClearProgressBar();
						}

						// Loop through the original Renderers list ...
						foreach (var meshRenderer2 in meshRenderers){
							if( meshRenderer2 != null ){

								// Skip this meshRenderer if it has a MeshFilter with submeshes
								if ( meshRenderer2.gameObject.GetComponent<MeshFilter>() &&
									meshRenderer2.gameObject.GetComponent<MeshFilter>().sharedMesh != null &&
									meshRenderer2.gameObject.GetComponent<MeshFilter>().sharedMesh.subMeshCount > 1){
									continue;
								}

								// If this original object didn't have a collider, it is no longer needed and can be destroyed.
								else if( meshRenderer2 != null && meshRenderer2.gameObject.GetComponent<Collider>() == null ){
									//DestroyImmediate(meshRenderer2.gameObject);
									Undo.DestroyObjectImmediate (meshRenderer2.gameObject);

								// Otherwise, destroy uneeded Components ...
								} else {

									// Otherwise, Destroy MeshRenderers
									if( meshRenderer2 != null && meshRenderer2.gameObject.GetComponent<MeshRenderer>() != null ){
									//	DestroyImmediate(meshRenderer2.gameObject.GetComponent<MeshRenderer>());
										Undo.DestroyObjectImmediate (meshRenderer2.gameObject.GetComponent<MeshRenderer>());
									}

									// Destroy MeshFilters
									if( meshRenderer2 != null && meshRenderer2.gameObject.GetComponent<MeshFilter>() != null ){
									//	DestroyImmediate(meshRenderer2.gameObject.GetComponent<MeshFilter>());
										Undo.DestroyObjectImmediate (meshRenderer2.gameObject.GetComponent<MeshFilter>());
									}
								}
							}
						}

						// Delete Any object with disabled Renderers
						if( deleteObjectsWithDisabledRenderers ){
							Renderer[] theRenderers = theTransform.GetComponentsInChildren<Renderer>(true) as Renderer[];
							foreach(Renderer r in theRenderers){
								if( r!=null && r.enabled == false && r.gameObject != null){
									Destroy(r.gameObject);
								}
							}
						}

						// Loop through all the transforms and destroy any blank objects
						if( deleteEmptyObjects ){
							DeleteOldBlankObjectsInEditor( theTransform );
						}

					}

					// Mark MeshFilters were combined so we know whether to show display dialogs
					meshFiltersWereCombined = true;

				// If we aren't recreating more Meshes, restore the MeshRenderers	
				} else {
					if(MeshKitGUI.verbose){ Debug.Log("No mesh filters can be combined in this group."); }
					foreach( MeshRenderer mr in renderersThatWereDisabled ){
						if(mr!=null){ mr.enabled = true; }
					}
					if( useSkinnedMeshRenderers == false ){
						EditorUtility.DisplayDialog("Mesh Combiner", "No mesh filters can be combined in this group.", "Okay");
					}
				}

				// Stop Progress Bar
				maxProgress = 0;
				progress = 0;
				EditorUtility.ClearProgressBar();	
			}

			// Do Skinned Mesh Renderers Too
			if( useSkinnedMeshRenderers == true ){
				StartCombineSMR( theTransform, optimizeMeshes, createNewObjectsWithLayer, createNewObjectsWithTag, enabledRenderersOnly, deleteSourceObjects, createNewObjectsWithMeshColliders, deleteObjectsWithDisabledRenderers, deleteEmptyObjects, userMaxVertices, optionRestoreBindposes, meshFiltersWereCombined );
			}
		}
		
		////////////////////////////////////////////////////////////////////////////////////////////////
		//	SAVE COMBINED MESH
		//	Allows us to save the combined mesh in a seperate function to make the code cleaner.
		////////////////////////////////////////////////////////////////////////////////////////////////
		
		public static void SaveCombinedMesh( GameObject go, Mesh newMesh, Material m, int createNewObjectsWithLayer, string createNewObjectsWithTag, bool createNewObjectsWithMeshColliders, int groupInt ){

			// If the Support Folders exist ... Save The Mesh!
			if( MeshAssets.HaveSupportFoldersBeenCreated() ){

				// Add A MeshFilter to the new GameObject
				MeshFilter filter = go.AddComponent<MeshFilter>();

				// Create and return the Mesh
				Mesh cMesh = MeshAssets.CreateMeshAsset( newMesh, MeshAssets.ProcessFileName(MeshAssets.combinedMeshFolder, m.name + "_" + groupInt.ToString() + " ["+m.shader.name+"]", "Combined", false) );

				if(cMesh!=null){
					filter.sharedMesh = cMesh;
					createdCombinedMeshObjects.Add(filter.gameObject);
				}

				// ========================
				//	ON ERROR
				// ========================

				// Was there an error creating the last asset?
				if(MeshAssets.errorCreatingAsset){

					// Reset the flag
					MeshAssets.errorCreatingAsset = false;

					// Stop Progress Bar
					maxProgress = 0;
					progress = 0;
					EditorUtility.ClearProgressBar();	

					// Reset the old MeshRenderers
					foreach( MeshRenderer mr in renderersThatWereDisabled ){
						if(mr!=null){ mr.enabled = true; }
					}	

					// Destroy the previously created combined Mesh GameObjects
					foreach( GameObject createdGO in createdCombinedMeshObjects){
						if(createdGO!=null){ 
							Undo.ClearUndo(createdGO);
							DestroyImmediate(createdGO); 
						}
					}
					// Destroy the currently created GameObject too!
					if(filter.gameObject != null ){ DestroyImmediate(filter.gameObject); } 
					
					// Show Error Message To User:
					EditorUtility.DisplayDialog("Mesh Combiner", "Unfortunately there was a problem with the combined mesh of group: \n\n"+m.name + "  ["+m.shader.name+"]\n\nThe Combine process must now be aborted.", "Okay");

					// Break out of the function
					return;
				}
			}
		 
			var renderer = go.AddComponent<MeshRenderer>();
			renderer.material = m;

			// Set Layer and Tag
			if(createNewObjectsWithLayer >= 0){ go.layer = createNewObjectsWithLayer; }
			if(createNewObjectsWithTag!=""){ go.tag = createNewObjectsWithTag;}

			// Create Mesh Colliders
			if( createNewObjectsWithMeshColliders ){ go.AddComponent<MeshCollider>(); }

			// Register the creation of this last gameobject
			Undo.RegisterCreatedObjectUndo (go, "MeshKit (Combine)");

		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		//	DELETE OLD BLANK OBJECTS IN EDITOR
		//	EDITOR VERSION ONLY - Doesnt Work At Runtime!
		////////////////////////////////////////////////////////////////////////////////////////////////
		
		public static void DeleteOldBlankObjectsInEditor( Transform theTransform  ){

			// Loop through all the transforms and destroy any blank objects
			for( int x = 0; x < 20; x++ ){
				// Loop through all the transforms and destroy any blank objects
				Transform[] theTransforms = theTransform.GetComponentsInChildren<Transform>();
				foreach(var t in theTransforms){
					if(t.gameObject.GetComponents<Component>().Length == 1 && t.childCount == 0 ){
						// DestroyImmediate(t.gameObject);
						Undo.DestroyObjectImmediate (t.gameObject);
					}
				}
			}
		}




// ->


		////////////////////////////////////////////////////////////////////////////////////////////////
		//	START COMBINE (SKINNED MESH RENDERERS)
		//	We create the new Meshes at start
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static void StartCombineSMR( Transform theTransform, bool optimizeMeshes, int createNewObjectsWithLayer, string createNewObjectsWithTag, bool enabledRenderersOnly, bool deleteSourceObjects, bool createNewObjectsWithMeshColliders, bool deleteObjectsWithDisabledRenderers, bool deleteEmptyObjects, int userMaxVertices = maxVertices, bool optionRestoreBindposes = false, bool meshFiltersWereAlreadyCombined = false ){
			
			// MEL EDITS (Combiner needs to be at the origin to reduce issues):
			Vector3 originalPosition = theTransform.position;
			Quaternion originalRotation = theTransform.rotation;
			Vector3 originalLocalScale = theTransform.localScale;
			theTransform.position = Vector3.zero;
			theTransform.rotation = Quaternion.identity;
			theTransform.localScale = Vector3.one;

			// Setup Support Folders and then cache the current number of Assets in the Combined Mesh Folder
			MeshAssets.HaveSupportFoldersBeenCreated();
			currentAssetCount = 0;
			currentAssetCount = MeshAssets.HowManyFilesAtAssetPath(MeshAssets.combinedMeshFolder);

			// =====================
			//	MAIN ROUTINE
			// =====================

			// Setup Progress Bar
			maxProgress = 4;
			progress = 0;

			// Make sure userMaxVertices doesn't exceed the actual maxVertices
			if(userMaxVertices > maxVertices ){ 
				Debug.Log("MESHKIT: Maximum vertices cannot be higher than " + maxVertices + ". Your settings have been changed.");
				userMaxVertices = maxVertices; 
			}

			// Prepare variables to store the new meshes, materials, etc.
			Matrix4x4 myTransform = theTransform.worldToLocalMatrix;
			Dictionary<Material, List<CombineInstance>> combines = new Dictionary<Material, List<CombineInstance>>();
			SkinnedMeshRenderer[] skinnedMeshRenderers = theTransform.GetComponentsInChildren<SkinnedMeshRenderer>();

			// Later on, this helps us find the SkinnedMeshRenderer based on the CombineInstance
			Dictionary<CombineInstance, SkinnedMeshRenderer> smrLoopup = new Dictionary<CombineInstance, SkinnedMeshRenderer>();

			// Turn off all animators
			Animator[] animators = theTransform.GetComponentsInChildren<Animator>();
			foreach( Animator anim in animators ){ anim.enabled = false; }
			
			// RESTORE BIND POSE ( Attempt To Fix Bindposes by enforcing T-Pose )
			if( optionRestoreBindposes ){ RestoreBindPose( theTransform ); } // <- this fixes some meshes and messes up others.

			// Track Renderers That Are Disabled
			renderersThatWereDisabled = new ArrayList();
			renderersThatWereDisabled.Clear();

			// Loop through the MeshRenderers inside of this group ...
			foreach (var smr in skinnedMeshRenderers){

				// Show Progress Bar
				if( maxProgress > 0 ){
					EditorUtility.DisplayProgressBar("Combining Mesh", "Preparing Skinned Mesh Renderers ...", 1.0f / 3.0f);
				} else {
					EditorUtility.ClearProgressBar();
				}

				// Make sure the MeshRenderers we are checking are valid!
				if(smr!=null){

					// Only combine meshes that have a single subMesh and have MeshRenderers that are enabled!
					if( smr.gameObject.GetComponent<SkinnedMeshRenderer>() != null &&
						smr.gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh != null &&
						smr.gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh.subMeshCount == 1 &&
						( !enabledRenderersOnly || smr.enabled == true )
					){

						// Loop through the materials of each renderer ...
						foreach (var material in smr.sharedMaterials){

							// If the material doesn't exist in the "combines" list then add it
							if (material != null && !combines.ContainsKey(material)){
								combines.Add(material, new List<CombineInstance>());
							}
						}
					}
				}
			}

			// Helper Variables for bones
			List<Transform> bones = new List<Transform>(); 
			List<BoneWeight> boneWeights = new List<BoneWeight>(); 

			int boneOffset = 0;

			// Loop through the MeshFilters inside of this group ...
			int howManyMeshCombinationsWereThere = 0;
			foreach(var smr in skinnedMeshRenderers){

				// Show Progress Bar
				if( maxProgress > 0 ){
					EditorUtility.DisplayProgressBar("Combining Mesh", "Analyzing Data From Mesh Filters ...", 2.0f / 3.0f);
				} else {
					EditorUtility.ClearProgressBar();
				}

				// If there isn't a mesh applied, skip it..
				if (smr.sharedMesh == null){
					continue;
				}

				// If this is connected to a MeshRenderer that is disabled ...
				else if (smr.gameObject.GetComponent<SkinnedMeshRenderer>() != null &&
					enabledRenderersOnly && smr.gameObject.GetComponent<SkinnedMeshRenderer>().enabled == false
				){
					continue;
				}

				// If this mesh has subMeshes, skip it..
				else if ( smr.sharedMesh.subMeshCount > 1){
					continue;
				} else {

					// Combine The Meshes
					CombineInstance ci = new CombineInstance();
					ci.mesh = smr.sharedMesh;
					#if !UNITY_5_5_OR_NEWER
						// Run Legacy optimization. This apparantly isn't needed anymore after Unity 5.5.
						if(optimizeMeshes){ ci.mesh.Optimize(); }
					#endif

					ci.transform = myTransform * smr.transform.localToWorldMatrix;

					// Make sure the current smr has a SkinnedMeshRenderer and that SkinnedMeshRenderer has a sharedMaterial
					if( smr.GetComponent<SkinnedMeshRenderer>() != null && smr.GetComponent<SkinnedMeshRenderer>().sharedMaterial != null ){
						combines[smr.GetComponent<SkinnedMeshRenderer>().sharedMaterial].Add(ci);

						// MEL: Also, track this CombineInstance with its Skinned Mesh Renderer in the smrLoopup
						smrLoopup.Add( ci, smr.GetComponent<SkinnedMeshRenderer>() );

						// Turn off the original renderer
						Undo.RecordObject ( smr.GetComponent<SkinnedMeshRenderer>(), "MeshKit (Combine)");
						renderersThatWereDisabled.Add(smr.GetComponent<SkinnedMeshRenderer>());
						smr.GetComponent<SkinnedMeshRenderer>().enabled = false;

						// Increment how many mesh combinations there were
						howManyMeshCombinationsWereThere++;
					}
				}	
			}

			// Create The Combined Meshes only if there are more materials then meshes
			if(MeshKitGUI.verbose){ Debug.Log("Skinned Mesh Combinations: "+ howManyMeshCombinationsWereThere); }
			if(MeshKitGUI.verbose){ Debug.Log("Skinned Material Count: "+ combines.Keys.Count); }
			if( combines.Keys.Count < howManyMeshCombinationsWereThere ){

				// Loop through the materials in the "combine" list ...
				float localProgressCount = 0f;
				float localCombineKeyCount = combines.Keys.Count;
				foreach(Material m in combines.Keys){

					// Show Progress Bar
					localProgressCount++;
					if( maxProgress > 0 ){
						EditorUtility.DisplayProgressBar("Combining Skinned Mesh", "Building New Combined Meshes ...", localProgressCount /localCombineKeyCount );
					} else {
						EditorUtility.ClearProgressBar();
					}

					// increment the number of assets
					currentAssetCount++;

					// NOTE: We should try to scan the size of these meshes before combining them to make sure they are within the limit
					float totalVertsCount = 0f;
					foreach( CombineInstance countCI in combines[m].ToArray() ){
						totalVertsCount += countCI.mesh.vertexCount;
					}

					// =================================
					//	LESS THAN 64K VERTICES
					// =================================

					// If there are less than 64k meshes, we can create the new GameObject normally ...
					if( totalVertsCount < userMaxVertices ){

						// NEW SECTION FOR BONES!
						// Reset the boneWeights before checking the CombineInstances
						boneWeights.Clear();

						// Loop through the CombineInstances for this material
						foreach( CombineInstance countCI in combines[m].ToArray() ){

							// MEL: May want to modify this if the renderer shares bones as unnecessary bones will get added.
							foreach( BoneWeight bw in countCI.mesh.boneWeights ){
							
								BoneWeight bWeight = bw;
								bWeight.boneIndex0 += boneOffset;
								bWeight.boneIndex1 += boneOffset;
								bWeight.boneIndex2 += boneOffset;
								bWeight.boneIndex3 += boneOffset;

								boneWeights.Add( bWeight );
							}

							// MEL We previously used the smrLookup Dictionary to link SkinnedMeshRenderers with the CombineInstance
							// We use this to configure the bones on the fly.
							foreach( KeyValuePair<CombineInstance, SkinnedMeshRenderer> lookup in smrLoopup ){
							
								// We need to use the Equals method here because structs can't use the "==" operator
								if( lookup.Key.Equals( countCI ) && lookup.Value != null ){	// Key = CombineInstance, Value = SMR
									
									// Debug.LogWarning("FOUND KEY and it has a SMR!");
									
									// The boneOffset should be number of bones being used in the original Skinned Mesh Renderer
									boneOffset += lookup.Value.bones.Length;

									Transform[] meshBones = lookup.Value.bones;
									foreach( Transform bone in meshBones ){
										// Debug.Log("Add bone: " + bone.name );
										bones.Add( bone );
									}
									break;
								}
							}
						}


						// FIGURING OUT ISSUE WITH CERTAIN CHARACTERS
						// - 	[NOPE] Negative scale in bones?
						// - 	[NOPE] Do we need to turn off the animator first? Seems likely as described in github example
						// -	It seems it has to do with the initial bindPose. Some models are not using the normal T-Pose
						// 	 	first, and the RestoreBindPose() method fixes the zombie character for example.
						// - 	Are the bones on feet and hands not being used some how?
						

						// POSSIBLE LINKS:
						// https://forum.unity.com/threads/cant-see-mesh-after-combining-it-skinned-mesh-renderer.430881/
						// https://gist.github.com/radiatoryang/3707b42341f6f7b3aa67b8387e1f8e68
						// https://answers.unity.com/questions/625243/combining-skinned-meshes-1.html
						// https://usermanual.wiki/Pdf/Manual.1214472972/html <= MeshBaker manual
						// https://forum.unity.com/threads/stitch-multiple-body-parts-into-one-character.16485/

						// FIGURING OUT ISSUE: Check for negative scale in bones -> Nope lol.
						/*
						for( int b = 0; b < bones.Count; b++ ) {
							if( bones[b].localScale.x < 0 || bones[b].localScale.y < 0 || bones[b].localScale.z < 0 ){
								Debug.LogWarning("Negative scale in bones detected!");
								break;
							}
						}
						*/

						// FIGURING OUT ISSUE: Test to see if all the bones exist in the new array -> Nope.
						/*
						Debug.Log("Checking Bones ...");
						bool boneFound = false;
						SkinnedMeshRenderer smr = null;
						foreach( KeyValuePair<CombineInstance, SkinnedMeshRenderer> lookup in smrLoopup ){
							smr = lookup.Value;
							for( int i = 0; i < smr.bones.Length; i++ ){
								boneFound = false;
								for( int b = 0; b < bones.Count; b++ ){
									if( smr.bones[i] == bones[b] ){
										boneFound = true;
									}
								}
								if(!boneFound){ Debug.LogWarning("Couldn't find bone: " + smr.bones[i].name ); }
							}
						}
						*/

						

						// NEW FOR SKINNED MESH RENDERERS: Setup bindposes from bones
						List<Matrix4x4> bindposes = new List<Matrix4x4>(); 
						for( int b = 0; b < bones.Count; b++ ) {
							bindposes.Add( bones[b].worldToLocalMatrix * theTransform.worldToLocalMatrix );
						}

						// Create a new GameObject based on the name of the material
						GameObject go = new GameObject(currentAssetCount.ToString("D4")+"_combined_" + m.name + "  ["+m.shader.name+"]");
						go.transform.parent = theTransform;
						go.transform.localPosition = Vector3.zero;
						go.transform.localRotation = Quaternion.identity;
						go.transform.localScale = Vector3.one;
						
						// Create a combined mesh using a new variable (avoids errors!)
						Mesh newMesh = new Mesh();
						newMesh.Clear();
						newMesh.name = currentAssetCount.ToString("D4")+"_combined_" + m.name + "  ["+m.shader.name+"]";
						newMesh.CombineMeshes(combines[m].ToArray(), true, true );	// <- original way
						//newMesh = CombineMeshes( combines[m].ToArray() );	// <- the MeshKit custom method ( better to use built-in version for now)

						//Debug.LogWarning( "MATERIALS TO ADD: " + combines.Keys.Count );

						// -> Normal code
						// Apply bones to mesh (bones are sent with the SaveCombinedSkinnedMesh command)
						newMesh.boneWeights = boneWeights.ToArray();
						newMesh.bindposes = bindposes.ToArray();
						newMesh.RecalculateBounds();

						// Create An Array to track which new GameObjects were created (so we can destroy them if something goes wrong)
						createdCombinedMeshObjects = new ArrayList();
						createdCombinedMeshObjects.Clear();

						// Save Combined Mesh and handle errors in a seperate function
						SaveCombinedSkinnedMesh( go, newMesh, m, createNewObjectsWithLayer, createNewObjectsWithTag, createNewObjectsWithMeshColliders, 0, bones ); // When creating assets normally, we use 0 as the group int.


						// DEBUG ONLY
						// Debug Check Bone Results
						/*
						int originalTotalBones = 0;
						int originalBoneWeights = 0;
						int originalBindPoses = 0;
						foreach(var smr in skinnedMeshRenderers){

							originalTotalBones += smr.bones.Length;
							originalBoneWeights += smr.sharedMesh.boneWeights.Length;
							originalBindPoses += smr.sharedMesh.bindposes.Length;
						}

						
						Debug.LogWarning("DEBUG: Original number of total bones in SMR: " + originalTotalBones );
						Debug.LogWarning("DEBUG: Original number of total boneWeights in SMR: " + originalBoneWeights );
						Debug.LogWarning("DEBUG: Original number of total bindPoses in SMR: " + originalBindPoses );
						Debug.LogWarning("DEBUG: ->" );
						Debug.LogWarning("DEBUG: Total bones in combined object: " + go.GetComponent<SkinnedMeshRenderer>().bones.Length );
						Debug.LogWarning("DEBUG: Total boneWeights in combined object: " + go.GetComponent<SkinnedMeshRenderer>().sharedMesh.boneWeights.Length );
						Debug.LogWarning("DEBUG: Total bindposes in combined object: " + go.GetComponent<SkinnedMeshRenderer>().sharedMesh.bindposes.Length );
						*/



					// =================================
					//	MORE THAN 64K VERTICES
					// =================================

					// Otherwise we need to break apart this CombinedInstance and create seperate ones within the limits.
					} else {

						// Debug Message
						if(MeshKitGUI.verbose){ Debug.Log("MESHKIT: Too many verts detected! Attempting To divide combined mesh for material \""+ m.name +"\" ..."); }

						// Count the index, the arrays created, the current total verts, and also a new combineInstance Array to build new combines from this oversized one ...
						int i = 0;
						int arraysCreated = 0;
						float currentVertsCount = 0f;
						ArrayList newCombineInstance = new ArrayList(); 
						newCombineInstance.Clear();

						// Loop through each of the CombineInstances and create new ones
						foreach( CombineInstance countCI in combines[m].ToArray() ){

							// --------------------------------------------------
							// BUILD A NEW COMBINED MESH BEFORE IT GETS TOO BIG
							// --------------------------------------------------

							// Will adding the new mesh make this combineInstance too large? AND -
							// If there is at least 1 mesh in this group, we should build it now before it gets too large.
							if( currentVertsCount + countCI.mesh.vertexCount >= userMaxVertices &&
								newCombineInstance.Count > 0 
							){

								// NEW SECTION FOR BONES!
								
								// Reset the boneWeights before checking the CombineInstances
								boneWeights.Clear();
								bones.Clear();
								boneOffset = 0;

								// Loop through the CombineInstances for this material
								foreach( CombineInstance countCI2 in newCombineInstance ){

									// MEL: May want to modify this if the renderer shares bones as unnecessary bones will get added.
									foreach( BoneWeight bw in countCI2.mesh.boneWeights ){
						
										BoneWeight bWeight = bw;
										bWeight.boneIndex0 += boneOffset;
										bWeight.boneIndex1 += boneOffset;
										bWeight.boneIndex2 += boneOffset;
										bWeight.boneIndex3 += boneOffset;                
						 
										boneWeights.Add( bWeight );	
									}

									// MEL We previously used the smrLookup Dictionary to link SkinnedMeshRenderers with the CombineInstance
									// We use this to configure the bones on the fly.
									foreach( KeyValuePair<CombineInstance, SkinnedMeshRenderer> lookup in smrLoopup ){
									
										// We need to use the Equals method here because structs can't use the "==" operator
										if( lookup.Key.Equals( countCI2 ) && lookup.Value != null ){	// Key = CombineInstance, Value = SMR
											
											// Debug.LogWarning("FOUND KEY in countCI2 and it has a SMR! bones: " + lookup.Value.bones.Length);
											
											// The boneOffset should be number of bones being used in the original Skinned Mesh Renderer
											boneOffset += lookup.Value.bones.Length;

											Transform[] meshBones = lookup.Value.bones;
											foreach( Transform bone in meshBones ){
												bones.Add( bone );
											}
											break;
										}
									}
								}

								// NEW FOR SKINNED MESH RENDERERS: Setup bindposes from bones
								List<Matrix4x4> bindposes = new List<Matrix4x4>(); 
								for( int b = 0; b < bones.Count; b++ ) {
									bindposes.Add( bones[b].worldToLocalMatrix * theTransform.worldToLocalMatrix );
								}


								// Create a new GameObject based on the name of the material
								GameObject go = new GameObject(currentAssetCount.ToString("D4")+"_combined_" + m.name + "_"+arraysCreated.ToString() +"  ["+m.shader.name+"]");
								go.transform.parent = theTransform;
								go.transform.localPosition = Vector3.zero;
								go.transform.localRotation = Quaternion.identity;
								go.transform.localScale = Vector3.one;

								// Create a combined mesh using a new variable (avoids errors!)
								Mesh newMesh = new Mesh();
								newMesh.Clear();
								newMesh.name = currentAssetCount.ToString("D4")+"_combined_" + m.name + "_" + arraysCreated.ToString() + "  ["+m.shader.name+"]";

								// Convert the CombineInstance into the builtin array and combine the meshes
								CombineInstance[] newCombineArr = (CombineInstance[]) newCombineInstance.ToArray( typeof( CombineInstance ) );
								newMesh.CombineMeshes( newCombineArr, true, true );

								// Apply bones to mesh (bones are sent with the SaveCombinedSkinnedMesh command)
								newMesh.boneWeights = boneWeights.ToArray();
								newMesh.bindposes = bindposes.ToArray();
								newMesh.RecalculateBounds();

								// Create An Array to track which new GameObjects were created (so we can destroy them if something goes wrong)
								createdCombinedMeshObjects = new ArrayList();
								createdCombinedMeshObjects.Clear();

								// Save Combined Mesh and handle errors in a seperate function
								//SaveCombinedSkinnedMesh( go, newMesh, m, createNewObjectsWithLayer, createNewObjectsWithTag, createNewObjectsWithMeshColliders, arraysCreated ); // When creating assets normally, we use 0 as the group int.
								SaveCombinedSkinnedMesh( go, newMesh, m, createNewObjectsWithLayer, createNewObjectsWithTag, false, arraysCreated, bones ); // In SkinnedMeshRenderers, dont add MeshColliders!

								// Reset The array to build the next group
								currentVertsCount = 0f;
								newCombineInstance = new ArrayList(); 
								newCombineInstance.Clear();

								// Increment the array count
								arraysCreated++;

								// MEL: Reset the boneWeights for the next group
								boneWeights.Clear();
							}

							// ----------------------------------------------
							// ADD THE NEW MESH TO THE ARRAY IF THERE IS ROOM
							// ----------------------------------------------

							// If theres space, add it to the group
							if( currentVertsCount + countCI.mesh.vertexCount < userMaxVertices ){

								// Add this CombineInstance into the new array...
								newCombineInstance.Add(countCI);

								// Update the total vertices so far ...
								currentVertsCount += countCI.mesh.vertexCount;

								// If this is the last loop - we should build the mesh here.
								if( i == combines[m].Count - 1 ){

									// NEW SECTION FOR BONES!
								
									// Reset the boneWeights before checking the CombineInstances
									boneWeights.Clear();
									bones.Clear();
									boneOffset = 0;

									// Loop through the CombineInstances for this material
									foreach( CombineInstance countCI2 in newCombineInstance ){

										// MEL: May want to modify this if the renderer shares bones as unnecessary bones will get added.
										foreach( BoneWeight bw in countCI2.mesh.boneWeights ){
							
											BoneWeight bWeight = bw;
											bWeight.boneIndex0 += boneOffset;
											bWeight.boneIndex1 += boneOffset;
											bWeight.boneIndex2 += boneOffset;
											bWeight.boneIndex3 += boneOffset;                
							 
											boneWeights.Add( bWeight );	
										}

										// MEL We previously used the smrLookup Dictionary to link SkinnedMeshRenderers with the CombineInstance
										// We use this to configure the bones on the fly.
										foreach( KeyValuePair<CombineInstance, SkinnedMeshRenderer> lookup in smrLoopup ){
										
											// We need to use the Equals method here because structs can't use the "==" operator
											if( lookup.Key.Equals( countCI2 ) && lookup.Value != null ){	// Key = CombineInstance, Value = SMR
												
												// Debug.LogWarning("FOUND KEY in countCI2 and it has a SMR! bones: " + lookup.Value.bones.Length);
												
												// The boneOffset should be number of bones being used in the original Skinned Mesh Renderer
												boneOffset += lookup.Value.bones.Length;

												Transform[] meshBones = lookup.Value.bones;
												foreach( Transform bone in meshBones ){
													bones.Add( bone );
												}
												break;
											}
										}
									}

									// NEW FOR SKINNED MESH RENDERERS: Setup bindposes from bones
									List<Matrix4x4> bindposes = new List<Matrix4x4>(); 
									for( int b = 0; b < bones.Count; b++ ) {
										bindposes.Add( bones[b].worldToLocalMatrix * theTransform.worldToLocalMatrix );
									}

									// Create a new GameObject based on the name of the material
									GameObject go2 = new GameObject(currentAssetCount.ToString("D4")+"_combined_" + m.name + "_"+arraysCreated.ToString() +"  ["+m.shader.name+"]");
									go2.transform.parent = theTransform;
									go2.transform.localPosition = Vector3.zero;
									go2.transform.localRotation = Quaternion.identity;
									go2.transform.localScale = Vector3.one;

									// Create a combined mesh using a new variable (avoids errors!)
									Mesh newMesh = new Mesh();
									newMesh.Clear();
									newMesh.name = currentAssetCount.ToString("D4")+"_combined_" + m.name + "_" + arraysCreated.ToString() + "  ["+m.shader.name+"]";

									// Convert the CombineInstance into the builtin array and combine the meshes
									CombineInstance[] newCombineArr = (CombineInstance[]) newCombineInstance.ToArray( typeof( CombineInstance ) );
									newMesh.CombineMeshes( newCombineArr, true, true );

									// Apply bones to mesh (bones are sent with the SaveCombinedSkinnedMesh command)
									//Debug.Log("BoneWeights Length: " + boneWeights.Count );
									newMesh.boneWeights = boneWeights.ToArray();
									newMesh.bindposes = bindposes.ToArray();
									newMesh.RecalculateBounds();

									// Create An Array to track which new GameObjects were created (so we can destroy them if something goes wrong)
									createdCombinedMeshObjects = new ArrayList();
									createdCombinedMeshObjects.Clear();

									// Save Combined Mesh and handle errors in a seperate function
									SaveCombinedSkinnedMesh( go2, newMesh, m, createNewObjectsWithLayer, createNewObjectsWithTag, createNewObjectsWithMeshColliders, arraysCreated, bones ); // When creating assets normally, we use 0 as the group int.

									// MEL: Reset the boneWeights for the next group
									boneWeights.Clear();
								}
							}

							// ----------------------------------------------
							// IF A SINGLE MESH IS TOO BIG TO BE ADDED
							// ----------------------------------------------

							else if( countCI.mesh.vertexCount >= maxVertices ){
								// Show warnings for meshes that are too large
								Debug.LogWarning("MESHKIT: MeshKit detected a Mesh called \"" + countCI.mesh.name + "\" with "+ countCI.mesh.vertexCount + " vertices using the material \""+ m.name + "\". This is beyond Unity's limitations and cannot be combined. This mesh was skipped.");
							}
				
							// Update index
							i++;
								
						}
					}
				}

				// =====================
				// 	CLEANUP
				// =====================
				
				// If we have chosen to delete the source objects ...
				if(deleteSourceObjects){

					// Show Progress Bar
					if( maxProgress > 0 ){
						EditorUtility.DisplayProgressBar("Combining Mesh", "Cleaning Up ...", 3.0f / 3.0f);
					} else {
						EditorUtility.ClearProgressBar();
					}

					// Loop through the original Renderers list ...
					foreach (var smr2 in skinnedMeshRenderers){
						if( smr2 != null ){

							// Skip this meshRenderer if it has a SkinnedMeshRenderer with submeshes
							if ( smr2.gameObject.GetComponent<SkinnedMeshRenderer>() &&
								smr2.gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh != null &&
								smr2.gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh.subMeshCount > 1){
								continue;
							}

							// If this original object didn't have a collider, it is no longer needed and can be destroyed.
							else if( smr2 != null && smr2.gameObject.GetComponent<Collider>() == null ){
								//DestroyImmediate(smr2.gameObject);
								Undo.DestroyObjectImmediate (smr2.gameObject);

							// Otherwise, destroy uneeded Components ...
							} else {

								// Otherwise, Destroy SkinnedMeshRenderer
								if( smr2 != null && smr2.gameObject.GetComponent<SkinnedMeshRenderer>() != null ){
								//	DestroyImmediate(smr2.gameObject.GetComponent<MeshRenderer>());
									Undo.DestroyObjectImmediate (smr2.gameObject.GetComponent<SkinnedMeshRenderer>());
								}

							}
						}
					}

					// Delete Any object with disabled Renderers
					if( deleteObjectsWithDisabledRenderers ){
						SkinnedMeshRenderer[] theRenderers = theTransform.GetComponentsInChildren<SkinnedMeshRenderer>(true) as SkinnedMeshRenderer[];
						foreach(SkinnedMeshRenderer r in theRenderers){
							if( r!=null && r.enabled == false && r.gameObject != null){
								Destroy(r.gameObject);
							}
						}
					}

					// Loop through all the transforms and destroy any blank objects
					if( deleteEmptyObjects ){
						DeleteOldBlankObjectsInEditor( theTransform );
					}

				}

			// If we aren't recreating more Meshes, restore the MeshRenderers	
			} else {
				if(MeshKitGUI.verbose){ Debug.Log("No skinned meshes can be combined in this group."); }
				foreach( SkinnedMeshRenderer smr in renderersThatWereDisabled ){
					if(smr!=null){ smr.enabled = true; }
				}

				// If mesh Filters were not already combined and we didnt combine any skinned meshes, show dialog
				if( meshFiltersWereAlreadyCombined == false ){
					EditorUtility.DisplayDialog("Mesh Combiner", "No skinned meshes or MeshFilters can be combined in this group.", "Okay");
				}
			}

			// Stop Progress Bar
			maxProgress = 0;
			progress = 0;
			EditorUtility.ClearProgressBar();	

			// MEL EDITS: (Move position and rotation back to default)
			theTransform.position = originalPosition;
			theTransform.rotation = originalRotation;
			theTransform.localScale = originalLocalScale;

			// Turn animators back on
			foreach( Animator anim in animators ){ anim.enabled = true; }
		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	RESTORE BIND POSE
		//	Some Skinned Meshes are messed up and need to their T-poses restored before combining.
		//	based on some code by JoeStrout https://forum.unity3d.com/threads/mesh-bindposes.383752/
		//	NOTE: This seems to work on some meshes, and mess up others.
		////////////////////////////////////////////////////////////////////////////////////////////////
		
		/*
		[MenuItem ("Window/MeshKit -> Restore BindPose")]
		public static void RestoreBindPoseMenuItem(){
			if(Selection.activeGameObject!=null){
				RestoreBindPose( Selection.activeGameObject.transform, true );
			}
		}
		*/

		public static void RestoreBindPose ( Transform theTransform, bool moveRootsToOrigin = true ) {

			// if you're using Mixamo models, every SkinnedMeshRenderer has a fraction of the full bind pose data...
			// so first we have to search through all of the SMRs and combine all the bindpose data into a dictionary
			//var	skinnedMeshRenderers = theTransform.GetComponentsInChildren<SkinnedMeshRenderer>().OrderBy( smr => smr.bones.Length ).ToArray();

			// MEL changed this - I've skipped the re-ordering, not sure why its needed.
			SkinnedMeshRenderer[] skinnedMeshRenderers = theTransform.GetComponentsInChildren<SkinnedMeshRenderer>();

			Dictionary<Transform, Matrix4x4> bindPoseMap = new Dictionary<Transform, Matrix4x4>();

			// Loop through the skinned mesh renderers...
			foreach ( var smr in skinnedMeshRenderers ) {

				// Loop through each SMR's bones ...
				for( int i=0; i<smr.bones.Length; i++ ){
					// If this bone isn't already added to the bindPoseMap dictionary, add it
					if ( !bindPoseMap.ContainsKey( smr.bones[i] ) ) {
						bindPoseMap.Add( smr.bones[i], smr.sharedMesh.bindposes[i] );
					}
				}
			}
		
			// Based on data, now move the bones based on the bindPoseMap
			foreach( var kvp in bindPoseMap ) {

				// Helper variables
				Transform boneTrans = kvp.Key;
				Matrix4x4 bindPose = kvp.Value;

				// Recreate the local transform matrix of the bone
				Matrix4x4 localMatrix = bindPoseMap.ContainsKey(boneTrans.parent) ? (bindPose * bindPoseMap[boneTrans.parent].inverse).inverse : bindPose.inverse;

				// Recreate local transform from that matrix
				boneTrans.localPosition = localMatrix.MultiplyPoint (Vector3.zero);
				boneTrans.localRotation = Quaternion.LookRotation (localMatrix.GetColumn (2), localMatrix.GetColumn (1));
				boneTrans.localScale = new Vector3 (localMatrix.GetColumn (0).magnitude, localMatrix.GetColumn (1).magnitude, localMatrix.GetColumn (2).magnitude);
			}

			// Debug Message
			if(MeshKitGUI.verbose){ Debug.Log("Reset " + bindPoseMap.Count + " bones to bind pose"); }

		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	SAVE COMBINED SKINNED MESH
		//	Allows us to save the combined mesh in a seperate function to make the code cleaner.
		////////////////////////////////////////////////////////////////////////////////////////////////
		
		public static void SaveCombinedSkinnedMesh( GameObject go, Mesh newMesh, Material m, int createNewObjectsWithLayer, string createNewObjectsWithTag, bool createNewObjectsWithMeshColliders, int groupInt, List<Transform> bones ){

			// If the Support Folders exist ... Save The Mesh!
			if( MeshAssets.HaveSupportFoldersBeenCreated() ){

				// Add A SkinnedMeshRenderer to the new GameObject
				SkinnedMeshRenderer smr = go.AddComponent<SkinnedMeshRenderer>();
				smr.material = m;

				// Setup the bones
				if(bones!=null){ smr.bones = bones.ToArray(); }

				// Create and return the Mesh
				Mesh cMesh = MeshAssets.CreateMeshAsset( newMesh, MeshAssets.ProcessFileName(MeshAssets.combinedMeshFolder, m.name + "_" + groupInt.ToString() + " ["+m.shader.name+"]", "Combined", false) );

				if(cMesh!=null){
					smr.sharedMesh = cMesh;
					createdCombinedMeshObjects.Add(smr.gameObject);
				}

				// ========================
				//	ON ERROR
				// ========================

				// Was there an error creating the last asset?
				if(MeshAssets.errorCreatingAsset){

					// Reset the flag
					MeshAssets.errorCreatingAsset = false;

					// Stop Progress Bar
					maxProgress = 0;
					progress = 0;
					EditorUtility.ClearProgressBar();	

					// Reset the old MeshRenderers
					foreach( SkinnedMeshRenderer mr in renderersThatWereDisabled ){
						if(mr!=null){ mr.enabled = true; }
					}	

					// Destroy the previously created combined Mesh GameObjects
					foreach( GameObject createdGO in createdCombinedMeshObjects){
						if(createdGO!=null){ 
							Undo.ClearUndo(createdGO);
							DestroyImmediate(createdGO); 
						}
					}
					// Destroy the currently created GameObject too!
					if(smr.gameObject != null ){ DestroyImmediate(smr.gameObject); } 
					
					// Show Error Message To User:
					EditorUtility.DisplayDialog("Mesh Combiner", "Unfortunately there was a problem with the combined skinned mesh of group: \n\n"+m.name + "  ["+m.shader.name+"]\n\nThe Combine process must now be aborted.", "Okay");

					// Break out of the function
					return;
				}
			}
		 
			
			// Set Layer and Tag
			if(createNewObjectsWithLayer >= 0){ go.layer = createNewObjectsWithLayer; }
			if(createNewObjectsWithTag!=""){ go.tag = createNewObjectsWithTag;}

			// Create Mesh Colliders
			//if( createNewObjectsWithMeshColliders ){ go.AddComponent<MeshCollider>(); }

			// Register the creation of this last gameobject
			Undo.RegisterCreatedObjectUndo (go, "MeshKit (Combine)");

		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		//	MESH DIAGNOSTIC
		////////////////////////////////////////////////////////////////////////////////////////////////

		/*
		[MenuItem ("Window/MeshKit -> Mesh Diagnostic")]
		public static void  MeshDiagnostic () {
			GameObject go = Selection.activeGameObject;

			if( go != null && 
				go.GetComponent<SkinnedMeshRenderer>() != null && 
				go.GetComponent<SkinnedMeshRenderer>().sharedMesh != null
			){
				// Cache Variables 
				var smr = go.GetComponent<SkinnedMeshRenderer>();
				var mesh = go.GetComponent<SkinnedMeshRenderer>().sharedMesh;

				Debug.Log( 
					"MESHKIT -> Inspecting: " + mesh.name + "\n" +
					
					"\nTriangles: " + mesh.triangles.Length +
					"\nVertices: " + mesh.vertexCount + 
					"\nNormals: " + mesh.normals.Length + 
					"\nTangents: " + mesh.tangents.Length + 
					"\nUV1: " + mesh.uv.Length + 
					"\nUV2: " + mesh.uv2.Length + 
					"\nUV3: " + mesh.uv3.Length + 
					"\nUV4: " + mesh.uv4.Length + 
					"\nboneWeights: " + mesh.boneWeights.Length +
					"\nBindPoses: " + mesh.bindposes.Length +
					"\nSMR bones: " + smr.bones.Length + 
					"\nColors: " + mesh.colors.Length +
					"\nBlendshapes: " + mesh.blendShapeCount +
					"\n"

				);


			}

		}
		*/

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	NEW COMBINE MESH FUNCTION
		//	This is an internal / custom version of the CombineMeshes method that also uses 
		//	CombineInstance arrays. There doesn't appear to be any benefit in this version.
		//	NOTES:
		//	- Doesn't do colors yet.
		//	- Don't do bones, weights, etc.
		////////////////////////////////////////////////////////////////////////////////////////////////

		private static Mesh CombineMeshes( CombineInstance[] combineInstances ){

			// Setup the mesh
			Mesh newMesh = new Mesh();
			newMesh.Clear();

			Debug.LogWarning("MESHKIT COMBINE MESHES: Combining " + combineInstances.Length + " Meshes ...");

			// Calculate vertex count
			int vertexCount = 0;
			for( int i = 0; i < combineInstances.Length; i++ ){
				vertexCount += combineInstances[i].mesh.vertexCount;
			}

			// Helper variables
			int[] triangles = new int[0];	// <- this is created dynamically
			Vector3[] vertices = new Vector3[vertexCount];
			Vector3[] normals = new Vector3[vertexCount];
			Vector4[] tangents = new Vector4[vertexCount];
			Vector2[] uv = new Vector2[vertexCount];
			Vector2[] uv2 = new Vector2[vertexCount];
		
		// These features work on Unity 5 and up ...				
		#if !UNITY_3_5 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_3 && !UNITY_4_4 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7	
		
			Vector2[] uv3 = new Vector2[vertexCount];
			Vector2[] uv4 = new Vector2[vertexCount];
		
		#endif

		#if UNITY_2018_2_OR_NEWER

			Vector2[] uv5 = new Vector2[vertexCount];
			Vector2[] uv6 = new Vector2[vertexCount];
			Vector2[] uv7 = new Vector2[vertexCount];
			Vector2[] uv8 = new Vector2[vertexCount];

		#endif

			int offset;
	 
	 		// =============
			// ADD VERTICES
			// =============

			offset=0;
			foreach( CombineInstance combine in combineInstances ){
				if (combine.mesh){
					Copy(combine.mesh.vertexCount, combine.mesh.vertices, vertices, ref offset, combine.transform);			
				}
			}

			// =============
			// ADD NORMALS
			// =============		
	 
			offset=0;
			foreach( CombineInstance combine in combineInstances ){
				if (combine.mesh){
					Matrix4x4 invTranspose = combine.transform;
					invTranspose = invTranspose.inverse.transpose;
					CopyNormal(combine.mesh.vertexCount, combine.mesh.normals, normals, ref offset, invTranspose);
				}
			}

			// =============
			// ADD TANGENTS
			// =============

			offset=0;
			foreach( CombineInstance combine in combineInstances ){
				if (combine.mesh){
					Matrix4x4 invTranspose = combine.transform;
					invTranspose = invTranspose.inverse.transpose;
					CopyTangents(combine.mesh.vertexCount, combine.mesh.tangents, tangents, ref offset, invTranspose);
				}
			}

			// =============
			// ADD UV 1
			// =============
			
			offset=0;
			foreach( CombineInstance combine in combineInstances )
			{
				if (combine.mesh){
					Copy(combine.mesh.vertexCount, combine.mesh.uv, uv, ref offset);
				}
			}
	 
	 		// =============
			// ADD UV 2
			// =============

			offset=0;
			foreach( CombineInstance combine in combineInstances )
			{
				if (combine.mesh){
					Copy(combine.mesh.vertexCount, combine.mesh.uv2, uv2, ref offset);
				}
			}

		// These features work on Unity 5 and up ...				
		#if !UNITY_3_5 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_3 && !UNITY_4_4 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7	
		
			// =============
			// ADD UV 3
			// =============

			offset=0;
			foreach( CombineInstance combine in combineInstances )
			{
				if (combine.mesh){
					Copy(combine.mesh.vertexCount, combine.mesh.uv3, uv3, ref offset);
				}
			}

			// =============
			// ADD UV 4
			// =============

			offset=0;
			foreach( CombineInstance combine in combineInstances )
			{
				if (combine.mesh){
					Copy(combine.mesh.vertexCount, combine.mesh.uv4, uv4, ref offset);
				}
			}

		#endif	
		#if UNITY_2018_2_OR_NEWER

			// =============
			// ADD UV 5
			// =============
			
			offset=0;
			foreach( CombineInstance combine in combineInstances )
			{
				if (combine.mesh){
					Copy(combine.mesh.vertexCount, combine.mesh.uv5, uv5, ref offset);
				}
			}

			// =============
			// ADD UV 6
			// =============
			
			offset=0;
			foreach( CombineInstance combine in combineInstances )
			{
				if (combine.mesh){
					Copy(combine.mesh.vertexCount, combine.mesh.uv6, uv6, ref offset);
				}
			}

			// =============
			// ADD UV 7
			// =============
			
			offset=0;
			foreach( CombineInstance combine in combineInstances )
			{
				if (combine.mesh){
					Copy(combine.mesh.vertexCount, combine.mesh.uv7, uv7, ref offset);
				}
			}

			// =============
			// ADD UV 8
			// =============
			
			offset=0;
			foreach( CombineInstance combine in combineInstances )
			{
				if (combine.mesh){
					Copy(combine.mesh.vertexCount, combine.mesh.uv8, uv8, ref offset);
				}
			}

		#endif

			// =============
			// ADD TRIANGLES
			// =============

			// Loop through the combineInstances
			int vertexOffset = 0; // <- used to offset the references of the vertices
			for( int ciIndex = 0; ciIndex < combineInstances.Length; ciIndex++ ){
									
				// Cache the Combine Index
				CombineInstance ci	= combineInstances[ciIndex];		

				// Cache the triangles first
				int[] _tmpTriangles = ci.mesh.triangles;
				
				// Loop through the new set of triangles and increase the index 
				for( int i = 0; i < _tmpTriangles.Length; i++ ){
					_tmpTriangles[i] += vertexOffset;
				}

				// Add the updated triangles to the mesh
				triangles = (int[])Combine( triangles, _tmpTriangles );

				// update the offset
				vertexOffset += ci.mesh.vertexCount;

			}

			// Create the mesh
			newMesh.vertices = vertices;
			newMesh.triangles = triangles;
			newMesh.normals = normals;
			newMesh.tangents = tangents;
			newMesh.uv = uv;
			newMesh.uv2 = uv2;
		
		// These features work on Unity 5 and up ...				
		#if !UNITY_3_5 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_3 && !UNITY_4_4 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7	
			newMesh.uv3 = uv3;
			newMesh.uv4 = uv4;
		#endif

		#if UNITY_2018_2_OR_NEWER
			newMesh.uv5 = uv5;
			newMesh.uv6 = uv6;
			newMesh.uv7 = uv7;
			newMesh.uv8 = uv8;
		#endif

			// return the mesh
			return newMesh;
		}


		static void Copy (int vertexcount, Vector3[] src, Vector3[] dst, ref int offset, Matrix4x4 transform)
		{
			for (int i=0;i<src.Length;i++)
			{
				dst[i+offset] = transform.MultiplyPoint(src[i]);
			}
			offset += vertexcount;
		}
	 
		static void CopyBoneWei (int vertexcount, BoneWeight[] src, BoneWeight[] dst, ref int offset, Matrix4x4 transform)
		{
			for (int i=0;i<src.Length;i++)
				dst[i+offset] =src[i];
			offset += vertexcount;
		}
	 
		static void CopyNormal (int vertexcount, Vector3[] src, Vector3[] dst, ref int offset, Matrix4x4 transform)
		{
			for (int i=0;i<src.Length;i++)
				dst[i+offset] = transform.MultiplyVector(src[i]).normalized;
			offset += vertexcount;
		}
	 
		static void Copy (int vertexcount, Vector2[] src, Vector2[] dst, ref int offset)
		{
			for (int i=0;i<src.Length;i++)
				dst[i+offset] = src[i];
			offset += vertexcount;
		}
	 
		static void CopyTangents (int vertexcount, Vector4[] src, Vector4[] dst, ref int offset, Matrix4x4 transform)
		{
			for (int i=0;i<src.Length;i++)
			{
				Vector4 p4 = src[i];
				Vector3 p = new Vector3(p4.x, p4.y, p4.z);
				p = transform.MultiplyVector(p).normalized;
				dst[i+offset] = new Vector4(p.x, p.y, p.z, p4.w);
			}
	 
			offset += vertexcount;
		}


		// ==================================================================================================================
		//	COMBINE
		//	Combines two of the same types of Array[] together
		// ==================================================================================================================
		
		public static T[] Combine<T>( T[] a, T[] b ){
			T[] newArray = new T[ a.Length + b.Length ];
			a.CopyTo( newArray, 0 );
			b.CopyTo( newArray, a.Length );
			return newArray;
		}

	}
}
