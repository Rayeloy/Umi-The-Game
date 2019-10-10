////////////////////////////////////////////////////////////////////////////////////////////////
//
//  BatchMeshSeperator.cs
//
//	Organizes SubMeshes in the scene so they can be broken down.
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

	// Classes (Available to all Editors using the HellTap namespace)
	public class BatchMeshes {
		public Material[] key;						// We use this as the identifier - this is the material array being used on each object.
		public Mesh originalMesh;					// The original Mesh of this gameObject
		public Mesh[] splitMeshes;					// When the MeshSplitter finishes building the meshes, it will return them here.
		public ArrayList gos;						// The list of GameObjects in the scene using this setup, we'll replace them after the batch is built.
	}

	// Class
	public class BatchMeshSeperator : EditorWindow {

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	VARIABLES
		////////////////////////////////////////////////////////////////////////////////////////////////

		// Helper Variables
		public static ArrayList list;					// This is an ArrayList of individual HTEBatchMeshes classes
		public static float progress = 0;
	    public static float maxProgress = 0;
		
		////////////////////////////////////////////////////////////////////////////////////////////////
		//	LIST CONTAINS
		//	Checks to see if this combination of materials and Mesh already exist in the ArrayList
		//	If it does, we add the new mesh to the existing setup as an extra GameObject
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static bool ListContains( Material[] key, Mesh originalMesh, MeshFilter mf ){
			// Loop through the list and see if this setup exists ...
			if( list != null && list.Count > 0 ){
				foreach( BatchMeshes bm in list ){
					// Make sure there is the same number of materials, and the same mesh ...
					if( bm.key.Length == key.Length && bm.originalMesh == originalMesh){	
						bool mismatchDetected = false;
						// Because we can't compare the materials using the array alone, we need to check them individually ...
						for( int i = 0; i < bm.key.Length; i++ ){
							if( bm.key[i] != key[i] ){
								mismatchDetected = true;
								break;	// A Material didn't match, so let's skip this entry ...
							}
						}
						// If no mismatches were detected, everything checks out - add this gameobject to the list and return true
						if(mismatchDetected == false ){ 
							bm.gos.Add(mf.gameObject);
							return true; 
						}
					}
				}
			}
			// If we didn't find it, return false
			return false;
		}

	    ////////////////////////////////////////////////////////////////////////////////////////////////
		//	PREPARE OBJECTS
		////////////////////////////////////////////////////////////////////////////////////////////////

	    public static void PrepareObjects( GameObject go, bool thisGameObjectOnly ){

	    	// Make sure that the scene is saved and the support folders have been created
	    	if( MeshAssets.HaveSupportFoldersBeenCreated() == true ){

		    	// Recreate the ArrayList
		    	list = new ArrayList();
		    	list.Clear();

		    	// Loop through all the MeshFilters in the selected Object 
				MeshFilter[] theMFs;
				if( thisGameObjectOnly == false ){
					theMFs = go.GetComponentsInChildren<MeshFilter>();
				// or just use this object depending on options
				} else {
					theMFs = go.GetComponents<MeshFilter>();
				}

				if(MeshKitGUI.verbose){ Debug.Log(theMFs.Length + " MeshFilters found for processing ..."); }
				int numberOfWorkableMFs = 0;
				foreach(MeshFilter mf in theMFs){

					// We need to find MeshFilters which have more than 1 Submesh:
					if( mf != null && mf.sharedMesh != null && mf.sharedMesh.subMeshCount > 1 ){

						// This Shared Mesh has more than 1 SubMesh, but we need to access its materials too ...
						if( mf.gameObject.GetComponent<MeshRenderer>() != null &&
							mf.gameObject.GetComponent<MeshRenderer>().sharedMaterials.Length > 1 &&
							mf.gameObject.GetComponent<MeshRenderer>().enabled == true	// Make sure the renderer is enabled to help prevent double baking!
						){
							// If this setup doesn't exist in the ArrayList, add it!
							if( !ListContains(mf.gameObject.GetComponent<MeshRenderer>().sharedMaterials, mf.sharedMesh, mf ) ){

								// Create a new BatchMesh Setup
								BatchMeshes bm = new BatchMeshes();
								bm.key = mf.gameObject.GetComponent<MeshRenderer>().sharedMaterials;
								bm.originalMesh = mf.sharedMesh;

								// Recreate a new GameObject ArrayList
								bm.gos = new ArrayList();
		    					bm.gos.Clear();
		    					bm.gos.Add(mf.gameObject);	// Add the first gameObject to the setup!

		    					// Add the new BatchMesh setup to the list
								list.Add(bm);

								// Increase the number of workable MFs
								numberOfWorkableMFs++;
							}
						}
					}
				}

				if(MeshKitGUI.verbose){ Debug.Log( "numberOfWorkableMFs: "+ numberOfWorkableMFs ); }
				if( numberOfWorkableMFs == 0){
					Debug.Log("MESHKIT: No objects require converting to independant meshes.");
				}

				// SEND EACH BATCH TO BE PROCESSED
				if( list!=null && list.Count > 0 ){
					int id = 0;

					// Setup Progress Bar
					maxProgress = list.Count;
					progress = 0;

					foreach( BatchMeshes bm2 in list ){
						if( bm2 != null && bm2.gos[0] != null ){

							// Show Progress Bar
							if( maxProgress > 0 ){
								EditorUtility.DisplayProgressBar("MeshKit Seperator", "Rebuilding Meshes ...", progress / maxProgress);
							} else {
								EditorUtility.ClearProgressBar();
							}

							// Split The Mesh based on the first GameObject and then return the splitMeshes
							bm2.splitMeshes = MeshSeperator.SplitMesh( bm2.gos[0] as GameObject, id );

							// Rebuild GameObjects With New Meshes
							if(bm2.splitMeshes.Length > 0 ){ RebuildSeperatedObjects(bm2); } 

							// Add 1 to progress ...
							progress += 1;

							// increment ID
							id++; 
						}
					}

					// Stop Progress Bar
					maxProgress = 0;
					progress = 0;
					EditorUtility.ClearProgressBar();
				}

				// Tell user that no submeshes were detected here!
				if( numberOfWorkableMFs == 0){
					EditorUtility.DisplayDialog("MeshKit Seperator", "No submeshes detected in this group.", "Okay");
				}
		    }
		}

	    ////////////////////////////////////////////////////////////////////////////////////////////////
		//	REBUILD SEPERATED OBJECTS
		////////////////////////////////////////////////////////////////////////////////////////////////

	    public static void RebuildSeperatedObjects( BatchMeshes bm ){
	    	if(bm!=null && bm.key.Length > 0 && bm.splitMeshes.Length > 0 && bm.gos.Count > 0 ){	// Make sure the BatchMesh file is valid

	    		// Convert bm.gos into a GameObject[] built in array.
	    		GameObject[] bmGameObjectBuiltInArray = (GameObject[]) bm.gos.ToArray( typeof( GameObject ) );
	    		
	    		foreach( GameObject go in bmGameObjectBuiltInArray ){
	    			// Loop through each of the Split Meshes (SubMeshes as Mesh)
	    			for( int i = 0; i < bm.splitMeshes.Length; i++ ){
	    				if( bm.splitMeshes[i] != null ){	// Make sure this Mesh is valid ...

	    					// Create a new GameObject to represent the new SubMesh
	    					GameObject newGo = new GameObject( bm.splitMeshes[i].name );
	    					newGo.tag = go.tag;
	    					newGo.layer = go.layer;

	    					newGo.transform.position = go.transform.position;
	    					newGo.transform.rotation = go.transform.rotation;
	    					newGo.transform.parent = go.transform.parent;	// We do this to get an accurate scale first!
	    					newGo.transform.localScale = go.transform.localScale;
	    					newGo.transform.parent = go.transform;
	    					
	    					// Recreate Mesh Filter
	    					MeshFilter mf = newGo.AddComponent<MeshFilter>();
	    					mf.sharedMesh = bm.splitMeshes[i];

	    					// Recreate Mesh Renderer
	    					MeshRenderer mr = newGo.AddComponent<MeshRenderer>();
	    					mr.receiveShadows = go.GetComponent<MeshRenderer>().receiveShadows;

	    		// These features work on Unity 5 and up ...				
				#if !UNITY_3_5 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_3 && !UNITY_4_4 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7
	    				
	    					mr.probeAnchor = go.GetComponent<MeshRenderer>().probeAnchor;
	    					mr.reflectionProbeUsage = go.GetComponent<MeshRenderer>().reflectionProbeUsage;
	    					mr.shadowCastingMode = go.GetComponent<MeshRenderer>().shadowCastingMode;
	    		#endif				

	    					// Set the material if it exists ...
	    					if(i < bm.key.Length){ mr.sharedMaterial = bm.key[i]; } // Key out of range fix?
	    					// If there are less materials than submeshes try to use a workaround
	    					else { 
	    						// See if we can access the previous material
	    						if(i > 0 && bm.key[i-1] != null ){
	    							mr.sharedMaterial = bm.key[i-1];
	    							Debug.LogWarning( "MESHKIT: The MeshRenderer for \"" + newGo.name  + "\" has been setup with less materials than submeshes and now has missing textures. MeshKit has tried to help by using the material of the previous submesh. To fix this you should increase the number of materials in the original MeshRenderer to match the number of submeshes.\n", newGo ); 

	    						// Otherwise let the user know that the material was missing	
	    						} else {
	    						//	mr.sharedMaterial = new Material("MESHKIT - Material was missing");
	    							mr.sharedMaterial = new Material( Shader.Find("Diffuse") ); // You cannot create new materials in 5.1
	    							Debug.LogWarning( "MESHKIT: The MeshRenderer for \"" + newGo.name  + "\" has been setup with less materials than submeshes and now has missing textures - to fix this you should increase the number of materials in the original MeshRenderer to match the number of submeshes.\n", newGo );
	    						}
	    					}

	    					// Set Light Probes
	    					#if UNITY_5_4_OR_NEWER
	    						// Old way of setting Light Probes
	    						mr.lightProbeUsage = go.GetComponent<MeshRenderer>().lightProbeUsage;

	    					#else
	    						// Old way of setting Light Probes
	    						mr.useLightProbes = go.GetComponent<MeshRenderer>().useLightProbes;

	    					#endif

	    					// Register this new GameObject with the undo system
	    					Undo.RegisterCreatedObjectUndo (newGo, "MeshKit (Seperate Submeshes)");

	    					// Turn off the original MeshRenderer when we're done
	    					Undo.RecordObject (go.GetComponent<MeshRenderer>(), "MeshKit (Seperate Submeshes)");
	    					go.GetComponent<MeshRenderer>().enabled = false;	
	    					

	    				}
	    			}
	    		}
			}
	    }
	}
}
