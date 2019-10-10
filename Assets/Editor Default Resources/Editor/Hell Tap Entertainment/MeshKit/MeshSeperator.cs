////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MeshSeperator.cs
//
//	Tool that splits up a mesh by its submeshes, and then organises the new meshes into folders
//	based on the scene name.
//
//	© 2015 Melli Georgiou.
//	Hell Tap Entertainment LTD
//
////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using HellTap.MeshKit;

// Use HellTap Namespace
namespace HellTap.MeshKit {

	// Class
	public class MeshSeperator : EditorWindow {

		// Variables
		static Mesh mesh;
		static int[] subMeshTris;
		static public Mesh[] returnSubMesh;				// This is where we will store the new Meshes
		
	    ////////////////////////////////////////////////////////////////////////////////////////////////
		//	SPLIT MESH
		//	The Core Routine
		////////////////////////////////////////////////////////////////////////////////////////////////

	    public static Mesh[] SplitMesh( GameObject go, int id ){

	    	// NOTE: This GameObject should have already checked if it has a MeshFilter
	    	if( go != null && go.GetComponent<MeshFilter>() != null && go.GetComponent<MeshFilter>().sharedMesh != null ){

		    	MeshFilter mf = go.GetComponent<MeshFilter>();
		     	mesh = mf.sharedMesh;

		     	// Make sure this mesh has more than 1 subMesh
		     	if( mesh.subMeshCount > 1 ){

		     		if(MeshKitGUI.verbose){ Debug.Log("MESHKIT SEPERATOR: "+ mesh.name + " has " + mesh.subMeshCount + " submeshes." );}

		     		// Create a builtin Array based on the number of submeshes (this is where we will store them)
		     		returnSubMesh = new Mesh[mesh.subMeshCount];

		     		// Loop though the submeshes and recreate them ...
		     		for(int i = 0; i < mesh.subMeshCount; i++){
						subMeshTris = mesh.GetTriangles(i);
						if( CreateMeshAsset( i, subMeshTris, true, id) ){
							continue;
						// Mesh Creation encountered a problem, break the loop.
						} else {
							break;
						}
		     		}

		     		// Return the list of subMeshes when we're done ...
		     		return returnSubMesh;

		     	// This object only has 1 subMesh	
		     	} else {
		     		Debug.LogWarning("MESHKIT SEPERATOR: "+ mesh.name + " hasn't got any submeshes. This mesh will be skipped.", Selection.activeGameObject);
		     	}
	     	}

	     	// Return null if something went wrong ...
	     	return null;
	    }
	 
	 	////////////////////////////////////////////////////////////////////////////////////////////////
		//	CREATE MESH ASSET
		//	Creates a new mesh and saves it as an Asset, based on the scene.
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static bool CreateMeshAsset( int i, int[] triangles, bool optimize, int id){

			// ==============================
			// CREATE THE NEW MESH
			// ==============================

			Mesh newMesh = new Mesh();
			newMesh.Clear();
			newMesh.vertices = mesh.vertices;
			newMesh.triangles = triangles;
			
			newMesh.bindposes = mesh.bindposes;			// *
			newMesh.boneWeights = mesh.boneWeights;		// *
			newMesh.uv = mesh.uv;
			newMesh.uv2 = mesh.uv2;						// Include UV2 so lightmapping works.

		// These features work on Unity 5 and up ...				
		#if !UNITY_3_5 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_3 && !UNITY_4_4 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7	
			newMesh.uv3 = mesh.uv3;						// *
			newMesh.uv4 = mesh.uv4;						// *
		#endif	

		// These features work on Unity 2018.2 and up ...
		#if UNITY_2018_2_OR_NEWER
			newMesh.uv5 = mesh.uv5;						// *
			newMesh.uv6 = mesh.uv6;						// *
			newMesh.uv7 = mesh.uv7;						// *
			newMesh.uv8 = mesh.uv8;						// *
		#endif	
		
			newMesh.colors32 = mesh.colors32;
			newMesh.subMeshCount = 1;					// The new submeshes should only have 1 submesh.
			newMesh.normals = mesh.normals;
			newMesh.tangents = mesh.tangents;			// This adds tangents so normal maps work!
			
			// Verbose Messages
			if(MeshKitGUI.verbose){
				string debugTriangles = "";
				foreach( int tri in newMesh.triangles ){
					debugTriangles += tri.ToString()+"\n";
				}
				Debug.Log("TRIANGLE GROUP "+ i +" Length: " + newMesh.triangles.Length + " [ " + debugTriangles +"]");

				string debugVerts = "";
				foreach( Vector3 v in newMesh.vertices ){
					debugVerts += v.ToString()+"\n";
				}
				Debug.Log("VERTS GROUP "+ i +" Length: " + newMesh.vertices.Length + "  [ " + debugVerts + "]") ;


				string debugUV = "";
				foreach( Vector2 uv in newMesh.uv ){
					debugUV += uv.ToString()+"\n";
				}

				Debug.Log("UV GROUP "+ i +" Length: " + newMesh.uv.Length + "  [ " + debugUV + "]") ;

				if( newMesh.normals.Length > 0 && newMesh.normals.Length == newMesh.vertices.Length ){
					Debug.Log( "!!! SUBMESH " + i + " has normals with the same Length as vertices.");
				}
				if( newMesh.tangents.Length > 0 && newMesh.tangents.Length == newMesh.vertices.Length ){
					Debug.Log( "!!! SUBMESH " + i + " has tangents with the same Length as vertices.");
				}
				if( newMesh.uv.Length > 0 && newMesh.uv.Length == newMesh.vertices.Length ){
					Debug.Log( "!!! SUBMESH " + i + " has UV1 with the same Length as vertices.");
				}
			}

			// Strip Unused Vertices
			newMesh = MeshRebuilder.StripUnusedVertices( newMesh, true, i );	// Mesh, Optimize, index (for debug)

			// ==============================
			// SAVE MESH AS A NEW ASSET
			// ==============================

			// If the Support Folders exist ...
			if( MeshAssets.HaveSupportFoldersBeenCreated() ){

				// Create and return the Mesh
				newMesh.name.MakeFileSystemSafe(); // Added 2nd May 2015 - fix dodgy mesh names.
				Mesh m = MeshAssets.CreateMeshAsset( newMesh, MeshAssets.ProcessFileName(MeshAssets.seperatedMeshFolder, mesh.name, "Seperated["+i+"]", false) );

				// Try to load the Asset back as a mesh - check if it is valid!
				if(m!=null){

					// Add the new Mesh to the list
					returnSubMesh[i] = m;

					if(MeshKitGUI.verbose){ Debug.Log(mesh.name + "_"+ id.ToString("D4") + "_submesh["+i+"] Created Successfully!"); }	
					return true;
				}

			}
			
			// Return false if something went wrong ...
			return false;
		}
	}
}

