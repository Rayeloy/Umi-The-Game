////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MeshKit.cs
//
//	The runtime Manager of the MeshKit tools. 
//	It contains all the needed core functions for manipulating meshes at runtime as well as
//	an API.
//
//	© 2015 - 2018 Melli Georgiou.
//	Hell Tap Entertainment LTD
//
////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Mesh Decimator Helpers
using HellTap.MeshDecimator.Unity;
using HellTap.MeshDecimator.Algorithms;
using UnityEngine.Rendering;
using HellTap.MeshKit;

// Use HellTap Namespace
namespace HellTap.MeshKit {
	
	// Class
	public class MeshKit : MonoBehaviour {

		// DEBUG MODE
		static bool debug = true;				// Debug The Runtime Operations Of MeshKit.
		const int maxVertices = 65534;			// Unity's upper limit of vertices that can be combined

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	SINGLETON PATTERN
		//	Makes sure that a MeshKit manager is in the scene
		////////////////////////////////////////////////////////////////////////////////////////////////

		private static MeshKit _instance;
		public static MeshKit com {	// <- Here we can call MeshKit.com.FunctionName
			get {
				// If the instance for MeshKit hasn't yet been setup ...
				if( _instance == null ) {

					// If there is more than 1 MeshKit Manager in the scene, warn users
					if( FindObjectsOfType<MeshKit>().Length > 1 ){
						Debug.LogWarning("MESHKIT: There is more than 1 MeshKit Manager in this scene. You do not need to add this script as it is created dynamically. Please remove all MeshKit.cs components from your scene.");
					}

					// If there is no instance for MeshKit, see if there is one in the Scene ...
					_instance = FindObjectOfType<MeshKit>();

					// If there isn't any objects already in the scene, recreate it
					if( _instance == null ) {
						GameObject go = new GameObject ("MeshKit Manager");
						go.hideFlags = HideFlags.HideAndDontSave; // Makes Manager invisible! :)
						_instance = go.AddComponent<MeshKit>();
					}
				}
				
				// return com when found / created
				return _instance;
			}
		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	REBUILD MESH
		//	Allows a Mesh to be rebuilt with / without normals, tangents, etc.
		////////////////////////////////////////////////////////////////////////////////////////////////

		// Rebuild Mesh
		public static Mesh RebuildMesh( Mesh m, bool optionStripNormals, bool optionStripTangents, bool optionStripColors, bool optionStripUV2, bool optionStripUV3, bool optionStripUV4, bool optionStripUV5, bool optionStripUV6, bool optionStripUV7, bool optionStripUV8, bool optionRebuildNormals, bool optionRebuildTangents, float rebuildNormalsAngle = -1 ){

			// Make sure we can access this Mesh
			if( m != null ){

				// ==============================
				// COPY THE SHARED MESH DATA
				// ==============================

				// Copy the sharedMesh
				Mesh mesh = new Mesh();
				mesh.Clear();
				mesh.vertices = m.vertices;
				
				// UVs
				mesh.uv = m.uv;
				mesh.uv2 = m.uv2;						// Include UV2 so lightmapping works.
		// These features work on Unity 5 and up ...				
		#if !UNITY_3_5 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_3 && !UNITY_4_4 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7	
				mesh.uv3 = m.uv3;						// *
				mesh.uv4 = m.uv4;						// *
		#endif			

		// These features work on Unity 2018.2 and up ...
		#if UNITY_2018_2_OR_NEWER
				mesh.uv5 = m.uv5;						// *
				mesh.uv6 = m.uv6;						// *
				mesh.uv7 = m.uv7;						// *
				mesh.uv8 = m.uv8;						// *
		#endif	


				// Remove UV2 (Lightmap UVs)
				if( optionStripUV2 ){ mesh.uv2 = new Vector2[0]; }

		// These features work on Unity 5 and up ...
		#if !UNITY_3_5 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_3 && !UNITY_4_4 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7	

				// Remove UV3 and UV4
				if( optionStripUV3 ){ mesh.uv3 = new Vector2[0]; }
				if( optionStripUV4 ){ mesh.uv4 = new Vector2[0]; }

		#endif	

		// These features work in Unity 2018.2 and up
		#if UNITY_2018_2_OR_NEWER

				// Remove UV5, UV6, UV7 and UV8
				if( optionStripUV5 ){ mesh.uv5 = new Vector2[0]; }
				if( optionStripUV6 ){ mesh.uv6 = new Vector2[0]; }
				if( optionStripUV7 ){ mesh.uv7 = new Vector2[0]; }
				if( optionStripUV8 ){ mesh.uv8 = new Vector2[0]; }

		#endif

				// Do triangles after vertices and UVs
				mesh.triangles = m.triangles;

				// Colors
				if( optionStripColors ){ 
					mesh.colors32 = new Color32[0];
				} else {
					mesh.colors32 = m.colors32;
				}

				// Extras
				mesh.subMeshCount = m.subMeshCount;		// The new submeshes should only have 1 submesh.
				mesh.bindposes = m.bindposes;			// *
				mesh.boneWeights = m.boneWeights;		// *

				// Strip / copy normals
				if(optionStripNormals){ 
					mesh.normals = new Vector3[0]; 
				} else {
					mesh.normals = m.normals;
				}

				// Strip / copy tangents
				if(optionStripTangents){ 
					mesh.tangents = new Vector4[0]; 
				} else {
					mesh.tangents = m.tangents;
				}
				
				// Rebuild Normals
				if(optionRebuildNormals ){ 
					if( rebuildNormalsAngle < 0 ){
						mesh.RecalculateNormals(); 
					} else {
						mesh.RecalculateNormalsBasedOnAngleThreshold( rebuildNormalsAngle );
					}
				}

				// Rebuild Tangents
				if(optionRebuildTangents){ mesh = CreateTangents(mesh); }

				// return the new mesh
				return mesh;

			}

			// Return the same Mesh if something goes wrong ...
			return m;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	STRIP UNUSED VERTICES
		//	Rebuilds The Mesh, stripping unused vertices and preserving the existing mesh data.
		////////////////////////////////////////////////////////////////////////////////////////////////

		// Strip Unused Vertices From The New Mesh
		public static Mesh StripUnusedVertices( Mesh m, bool optimize ){
			return MeshKit.Strip(m, optimize, false, false, false, false, false, false, false, false, false, false, false, false, false );
		}

		// Strip Unused Vertices From The New Mesh - full options
		public static Mesh Strip( Mesh m, bool optimize, bool stripNormals, bool stripTangents, bool stripColors, bool stripUV, bool stripUV2, bool stripUV3, bool stripUV4, bool stripUV5, bool stripUV6, bool stripUV7, bool stripUV8, bool stripBoneWeights, bool stripBindPoses ){

			// Cache mesh variables
			int[] cachedTriangles = m.triangles;
			Vector3[] cachedVertices = m.vertices;
			Vector3[] cachedNormals = m.normals;
			Vector4[] cachedTangents = m.tangents;
			Color32[] cachedColors = m.colors32;
			BoneWeight[] cachedBoneWeights = m.boneWeights;
			Matrix4x4[] cachedBindposes = m.bindposes;
			Vector2[] cachedUV = m.uv;
			Vector2[] cachedUV2 = m.uv2;
			// These features work on Unity 5 and up ...				
			#if !UNITY_3_5 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_3 && !UNITY_4_4 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7	
				Vector2[] cachedUV3 = m.uv3;
				Vector2[] cachedUV4 = m.uv4;
			#endif

			// These features work on Unity 2018.2 and up ...
			#if UNITY_2018_2_OR_NEWER
				Vector2[] cachedUV5 = m.uv5;
				Vector2[] cachedUV6 = m.uv6;
				Vector2[] cachedUV7 = m.uv7;
				Vector2[] cachedUV8 = m.uv8;
			#endif	

			// Male sure this mesh has triangles and vertices ...
			if( m !=null && cachedTriangles.Length > 0 && cachedVertices.Length > 0 ){

				// Options
				bool useNormals = false;
				bool useTangents = false;
				bool useColors = false;
				bool useUV = false;
				bool useUV2 = false;
				bool useBoneWeights = false;
				bool useBindPoses = false;

			// These features work on Unity 5 and up ...				
			#if !UNITY_3_5 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_3 && !UNITY_4_4 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7	
				bool useUV3 = false;
				bool useUV4 = false;
			#endif	

			// These features work on Unity 2018.2 and up ...
			#if UNITY_2018_2_OR_NEWER
				bool useUV5 = false;
				bool useUV6 = false;
				bool useUV7 = false;
				bool useUV8 = false;
			#endif
				
				// Helper Lists
				Vector3[] newNormals = new Vector3[0];
				Vector4[] newTangents = new Vector4[0];
				Color32[] newColors = new Color32[0];
				BoneWeight[] newBoneWeights = new BoneWeight[0];
				Matrix4x4[] newBindposes = new Matrix4x4[0];
				Vector2[] newUV = new Vector2[0];
				Vector2[] newUV2 = new Vector2[0];

			// These features work on Unity 5 and up ...				
			#if !UNITY_3_5 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_3 && !UNITY_4_4 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7	
				Vector2[] newUV3 = new Vector2[0];
				Vector2[] newUV4 = new Vector2[0];
			#endif	

			// These features work on Unity 2018.2 and up ...
			#if UNITY_2018_2_OR_NEWER
				Vector2[] newUV5 = new Vector2[0];
				Vector2[] newUV6 = new Vector2[0];
				Vector2[] newUV7 = new Vector2[0];
				Vector2[] newUV8 = new Vector2[0];
			#endif
				
				
				// Check to see if Normals, Tangents, UVs, etc exist and are setup correctly
				// If we've sent some strip options, take this into account too.
				if( !stripNormals && cachedNormals.Length > 0 && cachedNormals.Length == cachedVertices.Length ){ useNormals = true; }
				if( !stripTangents && cachedTangents.Length > 0 && cachedTangents.Length == cachedVertices.Length ){ useTangents = true; }
				if( !stripColors && m.colors32.Length > 0 && m.colors32.Length == m.colors32.Length ){ useColors = true; }
				if( !stripBoneWeights && m.boneWeights.Length > 0 && m.boneWeights.Length == cachedVertices.Length ){ useBoneWeights = true; }
				if( !stripBindPoses && m.bindposes.Length > 0 && m.bindposes.Length == cachedVertices.Length ){ useBindPoses = true; }
				if( !stripUV && m.uv.Length > 0 && m.uv.Length == cachedVertices.Length ){ useUV = true; }
				if( !stripUV2 && m.uv2.Length > 0 && m.uv2.Length == cachedVertices.Length ){ useUV2 = true; }

			// These features work on Unity 5 and up ...				
			#if !UNITY_3_5 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_3 && !UNITY_4_4 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7	
				if( !stripUV3 && m.uv3.Length > 0 && m.uv3.Length == cachedVertices.Length ){ useUV3 = true; }
				if( !stripUV4 && m.uv4.Length > 0 && m.uv4.Length == cachedVertices.Length ){ useUV4 = true; }
			#endif	

			// These features work on Unity 2018.2 and up ...
			#if UNITY_2018_2_OR_NEWER
				if( !stripUV5 && m.uv5.Length > 0 && m.uv5.Length == cachedVertices.Length ){ useUV5 = true; }
				if( !stripUV6 && m.uv6.Length > 0 && m.uv6.Length == cachedVertices.Length ){ useUV6 = true; }
				if( !stripUV7 && m.uv7.Length > 0 && m.uv7.Length == cachedVertices.Length ){ useUV7 = true; }
				if( !stripUV8 && m.uv8.Length > 0 && m.uv8.Length == cachedVertices.Length ){ useUV8 = true; }
			#endif
				

				// ========================================
				//	CREATE AN ARRAY TO TRACK VERTICES
				// ========================================

				// Create a temporary builtin int list to find the needed vertices.
				// NOTE: This array uses the full number of triangles to make sure we have enough space, we use
				// howManyVertsAdded to keep track of how many we're actually using so we can trim it after!
				int howManyVertsAdded = 0;
				int[] tempVertIndex = new int[cachedTriangles.Length];

				// Create new Vertices Index List (we use this to determine which vertexes are needed, and we save their indexes)
				for( int i = 0; i < cachedTriangles.Length; i++ ){
					
					// Cache the triangle
					int theCachedTriangle = cachedTriangles[i];

					// We should check to see if this vertex already exists in the array ...
					if( howManyVertsAdded > 0){
						
						// Track entry
						bool foundTriangle = false;
				
						// Loop through the tempVertIndex using howManyVertsAdded as the max range
						for( int checkVert = 0; checkVert < howManyVertsAdded; checkVert++  ){
							// If this vert matches the cachedTriangle, set true and break the loop ...
							if( tempVertIndex[checkVert] == theCachedTriangle ){
								foundTriangle = true; 
								break;
							}
						}

						// If it doesn't exist, add it!
						if(foundTriangle==false){ 
							tempVertIndex[howManyVertsAdded] = theCachedTriangle;
							howManyVertsAdded++;
						}
						
					// If there are no entries in the index yet, add the first one!	
					} else {
						tempVertIndex[howManyVertsAdded] = theCachedTriangle;
						howManyVertsAdded++;
					}
				}

				// Create the final version of the index by trimming the unused length (we use howManyVertsAdded)
				int[] newVertsIndex = new int[howManyVertsAdded];
				for( int copyVert = 0; copyVert < howManyVertsAdded; copyVert++ ){
					newVertsIndex[copyVert] = tempVertIndex[copyVert];
				}
				
				// ===============================================
				//	REBUILD VERTICES, NORMALS, TANGENTS, & COLORS
				// ===============================================

				// Rebuild the normals, tangents, etc if they are enabled
				if(useNormals){ newNormals = new Vector3[howManyVertsAdded]; }
				if(useTangents){ newTangents = new Vector4[howManyVertsAdded]; }
				if(useColors){ newColors = new Color32[howManyVertsAdded]; }
				if(useBoneWeights){ newBoneWeights = new BoneWeight[howManyVertsAdded]; }
				if(useBindPoses){ newBindposes = new Matrix4x4[howManyVertsAdded]; }	
				if(useUV){ newUV = new Vector2[howManyVertsAdded]; }
				if(useUV2){ newUV2 = new Vector2[howManyVertsAdded]; }

			// These features work on Unity 5 and up ...				
			#if !UNITY_3_5 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_3 && !UNITY_4_4 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7	
				if(useUV3){ newUV3 = new Vector2[howManyVertsAdded]; }
				if(useUV4){ newUV4 = new Vector2[howManyVertsAdded]; }
			#endif

			// These features work on Unity 2018.2 and up ...
			#if UNITY_2018_2_OR_NEWER
				if(useUV5){ newUV5 = new Vector2[howManyVertsAdded]; }
				if(useUV6){ newUV6 = new Vector2[howManyVertsAdded]; }
				if(useUV7){ newUV7 = new Vector2[howManyVertsAdded]; }
				if(useUV8){ newUV8 = new Vector2[howManyVertsAdded]; }
			#endif
				


				// Create a new Vertex list and populate it with the original Vertex values figured out from the last step
				Vector3[] newVerts = new Vector3[howManyVertsAdded];
				for( int v = 0; v<howManyVertsAdded; v++ ){

					// Cache the newVertsIndex[v] value as we use it a lot here!
					int cachedNewVertsIndexV = newVertsIndex[v];

					// Set the new vertices using the new Vertex Index
					newVerts[v] = cachedVertices[ cachedNewVertsIndexV ];

					// Set the other options using the new Vertex Index too
					if(useNormals){ newNormals[v] = cachedNormals[ cachedNewVertsIndexV ]; }
					if(useTangents){ newTangents[v] = cachedTangents[ cachedNewVertsIndexV ]; }
					if(useColors){ newColors[v] = cachedColors[ cachedNewVertsIndexV ]; }
					if(useBoneWeights){ newBoneWeights[v] = cachedBoneWeights[ cachedNewVertsIndexV ]; }
					if(useBindPoses){ newBindposes[v] = cachedBindposes[ cachedNewVertsIndexV ]; }
					if(useUV ){ newUV[v] = cachedUV[  cachedNewVertsIndexV ]; }
					if(useUV2){ newUV2[v] = cachedUV2[ cachedNewVertsIndexV ]; }

			// These features work on Unity 5 and up ...				
			#if !UNITY_3_5 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_3 && !UNITY_4_4 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7	
					if(useUV3){ newUV3[v] = cachedUV3[ cachedNewVertsIndexV ]; }
					if(useUV4){ newUV4[v] = cachedUV4[ cachedNewVertsIndexV ]; }
			#endif

			// These features work on Unity 2018.2 and up ...
			#if UNITY_2018_2_OR_NEWER
					if(useUV5){ newUV5[v] = cachedUV5[ cachedNewVertsIndexV ]; }
					if(useUV6){ newUV6[v] = cachedUV6[ cachedNewVertsIndexV ]; }
					if(useUV7){ newUV7[v] = cachedUV7[ cachedNewVertsIndexV ]; }
					if(useUV8){ newUV8[v] = cachedUV8[ cachedNewVertsIndexV ]; }
			#endif
					
				}
				
				// ========================================
				//	CREATE NEW TRIANGLES
				// ========================================

				// Create new Triangles index
				int[] newTriangles = cachedTriangles;
				int tIndex = 0;
				// Loop through each triangle Index in the mesh triangles array ...
				foreach( int tri in cachedTriangles ){
					
					// Loop through the Vertex Index List
					for( int vi = 0; vi<howManyVertsAdded; vi++ ){	// vi = Vertex Index
						// If the current triangle index points to the same value as the current Vertex Index ...
						if( tri == newVertsIndex[vi] ){
							newTriangles[tIndex] = vi;	// We replace the original triangle index with the position 
						}
					}
					// Iterate Loop
					tIndex++;
					
				}

				// ========================================
				//	APPLY TO THE MESH
				// ========================================

				// Apply the changes
				m.Clear();
				m.vertices = newVerts;
				
				// Extras
				if(useNormals){ m.normals = newNormals; }
				if(useTangents){ m.tangents = newTangents; }
				if(useColors){ m.colors32 = newColors; }
				if(useUV){ m.uv = newUV; }
				if(useUV2){ m.uv2 = newUV2; }

			// These features work on Unity 5 and up ...				
			#if !UNITY_3_5 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_3 && !UNITY_4_4 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7	
				if(useUV3){ m.uv3 = newUV3; }
				if(useUV4){ m.uv4 = newUV4; }
			#endif	

			// These features work on Unity 2018.2 and up ...
			#if UNITY_2018_2_OR_NEWER
				if(useUV5){ m.uv5 = newUV5; }
				if(useUV6){ m.uv6 = newUV6; }
				if(useUV7){ m.uv7 = newUV7; }
				if(useUV8){ m.uv8 = newUV8; }
			#endif
				
				if(useBoneWeights){ m.boneWeights = newBoneWeights; }
				if(useBindPoses){ m.bindposes = newBindposes; }

				// Triangles and recaculate last!
				m.triangles = newTriangles;
				m.RecalculateBounds();

				#if !UNITY_5_5_OR_NEWER
					// Run Legacy optimization. This apparantly isn't needed anymore after Unity 5.5.
					if(optimize){ m.Optimize(); }
				#endif
			}

			// Return Mesh after it has been processed...
			return m;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	SPLIT MESH
		//	This is the runtime version of the Split Meshes Routine
		//	(It's quite different to the Editor version as it works directly with the Mesh itself)
		////////////////////////////////////////////////////////////////////////////////////////////////

	    public static Mesh[] SplitMesh( Mesh mesh, bool stripUnusedVertices ){

	    	// Make sure this Mesh is not null
	    	if( mesh != null ){

		     	// Make sure this mesh has more than 1 subMesh
		     	if( mesh.subMeshCount > 1 ){

		     		if(MeshKit.debug){ Debug.Log("MESHKIT: "+ mesh.name + " has " + mesh.subMeshCount + " submeshes." );}

		     		// Create a builtin Array based on the number of submeshes (this is where we will store them)
		     		Mesh[] returnSubMesh = new Mesh[mesh.subMeshCount];

		     		// Loop though the submeshes and recreate them ...
		     		for(int i = 0; i < mesh.subMeshCount; i++){

						// ==============================
						// CREATE THE NEW MESH
						// ==============================

		     			// Create a new Mesh based on the old one ...
						Mesh newMesh = new Mesh();
						newMesh.Clear();
						newMesh.vertices = mesh.vertices;
						newMesh.triangles = mesh.GetTriangles(i);
						
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

						// Rebuild this mesh with its Unused Vertices stripped
						newMesh = MeshKit.StripUnusedVertices( newMesh, true );	// Mesh, Optimize

						// Add the new Mesh to the list
						returnSubMesh[i] = newMesh;
						returnSubMesh[i].name = mesh.name +" - MeshKit Separated ["+i.ToString()+"]";
		     		}

		     		// Return the list of subMeshes when we're done ...
		     		return returnSubMesh;

		     	// This object only has 1 subMesh	
		     	} else {
		     		Debug.Log("MESHKIT: "+ mesh.name + " hasn't got any submeshes. This mesh will be skipped.");
		     	}
	     	}

	     	// Return null if something went wrong ...
	     	return null;
	    }

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	CREATE MESH TANGENTS
		//	Used for Rebuilding Double-Sided Meshes and more.
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static Mesh CreateTangents(Mesh mesh) {

			// Setup Mesh Data Arrays
			int[] triangles = mesh.triangles;
			Vector3[] vertices = mesh.vertices;
			Vector2[] uv = mesh.uv;
			Vector3[] normals = mesh.normals;

			// Skip creating Tangents if this mesh doesn't have UVs
			if( uv.Length == 0 ){
				Debug.LogWarning("MESHKIT: Tangents couldn't be created for a Mesh because it didn't have UVs! Skipping ...");
				return mesh;
			}
		 
			// Helper Variables
			int triangleCount = triangles.Length;
			int vertexCount = vertices.Length;
			Vector3[] tan1 = new Vector3[vertexCount];
			Vector3[] tan2 = new Vector3[vertexCount];
			Vector4[] tangents = new Vector4[vertexCount];
		 
		 	// Loop through the triangle count in steps of 3
			for (long a = 0; a < triangleCount; a += 3) {

				// Setup local loop variables
				long i1 = triangles[a + 0];
				long i2 = triangles[a + 1];
				long i3 = triangles[a + 2];
		 
				Vector3 v1 = vertices[i1];
				Vector3 v2 = vertices[i2];
				Vector3 v3 = vertices[i3];
		 
				Vector2 w1 = uv[i1];
				Vector2 w2 = uv[i2];
				Vector2 w3 = uv[i3];
		 
				float x1 = v2.x - v1.x;
				float x2 = v3.x - v1.x;
				float y1 = v2.y - v1.y;
				float y2 = v3.y - v1.y;
				float z1 = v2.z - v1.z;
				float z2 = v3.z - v1.z;
		 
				float s1 = w2.x - w1.x;
				float s2 = w3.x - w1.x;
				float t1 = w2.y - w1.y;
				float t2 = w3.y - w1.y;
		 
				float div = s1 * t2 - s2 * t1;
 				float r = div == 0.0f ? 0.0f : 1.0f / div;
		 
				Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
				Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);
		 
				tan1[i1] += sdir;
				tan1[i2] += sdir;
				tan1[i3] += sdir;
		 
				tan2[i1] += tdir;
				tan2[i2] += tdir;
				tan2[i3] += tdir;
			}
		 
		 	// Loop through vertex count
			for ( long a = 0; a < vertexCount; ++a ){

				Vector3 n = normals[a];
				Vector3 t = tan1[a];
		 
				//Vector3 tmp = (t - n * Vector3.Dot(n, t)).normalized;
				//tangents[a] = new Vector4(tmp.x, tmp.y, tmp.z);
				Vector3.OrthoNormalize(ref n, ref t);
				tangents[a].x = t.x;
				tangents[a].y = t.y;
				tangents[a].z = t.z;
		 
				tangents[a].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
			}
		 
		 	// Apply Tangents
			mesh.tangents = tangents;

			// Return the Mesh
			return mesh;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	INVERT MESH
		//	Turns a Mesh Inside-Out
		////////////////////////////////////////////////////////////////////////////////////////////////

		// Invert Mesh
		public static Mesh InvertMesh( Mesh mesh ){

			// Only Invert this Mesh if it doesn't have subMeshes
			if( mesh.subMeshCount == 1 ){

				// Setup helper variables
				int[] indices = mesh.triangles;
				int triangleCount = indices.Length / 3;

				// Inverting a mesh requires swapping the first 
				// and second triangles in each set of 3.
				for(var i = 0; i < triangleCount; i++){
					int tmp = indices[i*3];
					indices[i*3] = indices[i*3 + 1];
					indices[i*3 + 1] = tmp;
				}

				// Apply Triangles
				mesh.triangles = indices;

				// Flip vertex normals to correct lights
				Vector3[] normals = mesh.normals;
				for(var n = 0; n < normals.Length; n++){
				    normals[n] = -normals[n];
				}

				// Apply Normals
				mesh.normals = normals;

			}

			// Return Mesh
			return mesh;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	MAKE DOUBLE-SIDED MESH
		//	Makes the Mesh visible from both sides
		////////////////////////////////////////////////////////////////////////////////////////////////

		// Double Sided Test
		public static Mesh MakeDoubleSidedMesh( Mesh mesh ){

			// Only Invert this Mesh if it doesn't have subMeshes
			if( mesh.subMeshCount == 1 ){

				// =============================================================================
				//	SETUP THE CONFIGURATION OF THE MESH
				//	We need to figure out what this mesh has so we can create the mesh properly
				// =============================================================================

				// Figure out if this mesh actually has normals, tangents, etc.
				bool useNormals = false;
				bool useTangents = false;
				bool useColors = false;
				bool useBoneWeights = false;
			//	bool useBindPoses = false;
				bool useUV = false;
				bool useUV2 = false;

			// These features work on Unity 5 and up ...				
			#if !UNITY_3_5 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_3 && !UNITY_4_4 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7	
				bool useUV3 = false;
				bool useUV4 = false;
			#endif	

			#if UNITY_2018_2_OR_NEWER
				bool useUV5 = false;
				bool useUV6 = false;
				bool useUV7 = false;
				bool useUV8 = false;
			#endif		

				// Check to see if Normals, Tangents, UVs, etc exist and are setup correctly
				if( mesh.normals.Length > 0 && mesh.normals.Length == mesh.vertices.Length ){ useNormals = true; }
				if( mesh.tangents.Length > 0 && mesh.tangents.Length == mesh.vertices.Length ){ useTangents = true; }
				if( mesh.colors32.Length > 0 && mesh.colors32.Length == mesh.colors32.Length ){ useColors = true; }
				if( mesh.boneWeights.Length > 0 && mesh.boneWeights.Length == mesh.vertices.Length ){ useBoneWeights = true; }
			//	if( mesh.bindposes.Length > 0 && mesh.bindposes.Length == mesh.vertices.Length ){ useBindPoses = true; }
				if( mesh.uv.Length > 0 && mesh.uv.Length == mesh.vertices.Length ){ useUV = true; }
				if( mesh.uv2.Length > 0 && mesh.uv2.Length == mesh.vertices.Length ){ useUV2 = true; }

			// These features work on Unity 5 and up ...				
			#if !UNITY_3_5 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_3 && !UNITY_4_4 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7	
				if( mesh.uv3.Length > 0 && mesh.uv3.Length == mesh.vertices.Length ){ useUV3 = true; }
				if( mesh.uv4.Length > 0 && mesh.uv4.Length == mesh.vertices.Length ){ useUV4 = true; }
			#endif	

			#if UNITY_2018_2_OR_NEWER
				if( mesh.uv5.Length > 0 && mesh.uv5.Length == mesh.vertices.Length ){ useUV5 = true; }
				if( mesh.uv6.Length > 0 && mesh.uv6.Length == mesh.vertices.Length ){ useUV6 = true; }
				if( mesh.uv7.Length > 0 && mesh.uv7.Length == mesh.vertices.Length ){ useUV7 = true; }
				if( mesh.uv8.Length > 0 && mesh.uv8.Length == mesh.vertices.Length ){ useUV8 = true; }
			#endif	
				

				//Debug.Log( useNormals + " " +useTangents + " " +useColors + " " +useUV + " " +useUV2 + " " +useUV3 + " " +useUV4 );

				// Helper Variables
				Vector3[] vertices = mesh.vertices;
				Vector3[] normals = mesh.normals;
				Color32[] colors = mesh.colors32;
				BoneWeight[] boneWeights = mesh.boneWeights;
				Matrix4x4[] bindposes = mesh.bindposes;
				Vector2[] uv = mesh.uv;
				Vector2[] uv2 = mesh.uv2;

			// These features work on Unity 5 and up ...				
			#if !UNITY_3_5 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_3 && !UNITY_4_4 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7	
				Vector2[] uv3 = mesh.uv3;
				Vector2[] uv4 = mesh.uv4;
			#endif	

			#if UNITY_2018_2_OR_NEWER
				Vector2[] uv5 = mesh.uv5;
				Vector2[] uv6 = mesh.uv6;
				Vector2[] uv7 = mesh.uv7;
				Vector2[] uv8 = mesh.uv8;
			#endif
				
				// NOTE: With tangents, we rebuild it at the end if they were detected.

				// New Mesh Data
				int numOfVerts = vertices.Length;
				Vector3[] newVertices = new Vector3[numOfVerts*2];
				Vector3[] newNormals = new Vector3[0];
				Vector4[] newTangents = new Vector4[0];
				Color32[] newColors32 = new Color32[0];
				BoneWeight[] newBoneWeights = new BoneWeight[0];
			//	Matrix4x4[] newBindposes = new Matrix4x4[0];
				Vector2[] newUVs = new Vector2[0];
				Vector2[] newUV2 = new Vector2[0];

			// These features work on Unity 5 and up ...				
			#if !UNITY_3_5 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_3 && !UNITY_4_4 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7	
				Vector2[] newUV3 = new Vector2[0];
				Vector2[] newUV4 = new Vector2[0];
			#endif	

			#if UNITY_2018_2_OR_NEWER
				Vector2[] newUV5 = new Vector2[0];
				Vector2[] newUV6 = new Vector2[0];
				Vector2[] newUV7 = new Vector2[0];
				Vector2[] newUV8 = new Vector2[0];
			#endif
				

				// If this mesh has other features, set them up!
				if(useNormals){ newNormals = new Vector3[numOfVerts*2]; }
				// if(useTangents){ This is now rebuilt at the end of the script! }
				if(useColors){ newColors32 = new Color32[numOfVerts*2]; }
				if(useBoneWeights){ newBoneWeights = new BoneWeight[numOfVerts*2]; }
				//if(useBindPoses){ newBindposes = new Matrix4x4[numOfVerts*2]; }
				if(useUV){ newUVs = new Vector2[numOfVerts*2]; }
				if(useUV2){ newUV2 = new Vector2[numOfVerts*2]; }

			// These features work on Unity 5 and up ...				
			#if !UNITY_3_5 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_3 && !UNITY_4_4 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7	
				if(useUV3){ newUV3 = new Vector2[numOfVerts*2]; }
				if(useUV4){ newUV4 = new Vector2[numOfVerts*2]; }
			#endif	

			#if UNITY_2018_2_OR_NEWER
				if(useUV5){ newUV5 = new Vector2[numOfVerts*2]; }
				if(useUV6){ newUV6 = new Vector2[numOfVerts*2]; }
				if(useUV7){ newUV7 = new Vector2[numOfVerts*2]; }
				if(useUV8){ newUV8 = new Vector2[numOfVerts*2]; }
			#endif
				

				// New Mesh Data 
				int[] triangles = mesh.triangles;
				int numOfTris = triangles.Length;								
				int[] newTriangles = new int[numOfTris*2];		// Double the amount of triangles

				// =========================================================================
				//	COPY IN THE ORIGINAL MESH DATA
				//	We also setup the normals of the second (double-sided) half of the mesh.
				// =========================================================================

				// Loop through the number of original vertices ...
				for (int x=0; x< numOfVerts; x++){

					// Copy the original vertices and UVs into the new array
					newVertices[x] = newVertices[x+numOfVerts] = vertices[x];
					if(useUV){ newUVs[x] = newUVs[x+numOfVerts] = uv[x]; }
					if(useUV2){ newUV2[x] = newUV2[x+numOfVerts] = uv2[x]; }

			// These features work on Unity 5 and up ...				
			#if !UNITY_3_5 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_3 && !UNITY_4_4 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7
					if(useUV3){ newUV3[x] = newUV3[x+numOfVerts] = uv3[x]; }
					if(useUV4){ newUV4[x] = newUV4[x+numOfVerts] = uv4[x]; }
			#endif	

			#if UNITY_2018_2_OR_NEWER
					if(useUV5){ newUV5[x] = newUV5[x+numOfVerts] = uv5[x]; }
					if(useUV6){ newUV6[x] = newUV6[x+numOfVerts] = uv6[x]; }
					if(useUV7){ newUV7[x] = newUV7[x+numOfVerts] = uv7[x]; }
					if(useUV8){ newUV8[x] = newUV8[x+numOfVerts] = uv8[x]; }
			#endif	

					// Copy the original normals, colors, bones and poses... (NOTE Tangents are rebuilt at the end!)
					if(useNormals){ newNormals[x] = normals[x]; }
					if(useColors){ newColors32[x] = colors[x]; }
					if(useBoneWeights){ newBoneWeights[x] = boneWeights[x]; }
				//	if(useBindPoses){ newBindposes[x] = bindposes[x]; }


					// At this point we also setup the second half of the normals array
					// as we copy them into the second half, we invert them.
					// We can also populate colors, etc.
					if(useNormals){ newNormals[x+numOfVerts] = -normals[x]; }
					if(useColors){ newColors32[x+numOfVerts] = colors[x]; }
					if(useBoneWeights){ newBoneWeights[x+numOfVerts] = boneWeights[x]; }
				//	if(useBindPoses){ newBindposes[x+numOfVerts] = bindposes[x]; }
				}

				// =========================================================================
				//	CREATE THE SECOND (DOUBLE-SIDED) HALF OF MESH
				// =========================================================================

				// Loop through the number of original triangles, but increment in groups of 3.
				for (int i=0; i< numOfTris; i+=3){

					// Copy the original triangle
					newTriangles[i] = triangles[i];
					newTriangles[i+1] = triangles[i+1];
					newTriangles[i+2] = triangles[i+2];

					// Save the index of the new reversed triangle
					int j = i+numOfTris;

					// Here we pretty much apply the same as the Invert routine.
					// We swap the 2nd and 3rd triangle index.
					newTriangles[j] = triangles[i]+numOfVerts;
					newTriangles[j+2] = triangles[i+1]+numOfVerts;
					newTriangles[j+1] = triangles[i+2]+numOfVerts;
				}

				// Apply The Mesh ( Verts first, Triangles last!)
				mesh.vertices = newVertices;
				mesh.uv = newUVs;
				mesh.uv2 = newUV2;

			// These features work on Unity 5 and up ...				
			#if !UNITY_3_5 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_3 && !UNITY_4_4 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7	
				mesh.uv3 = newUV3;
				mesh.uv4 = newUV4;
			#endif	

			#if UNITY_2018_2_OR_NEWER
				mesh.uv5 = newUV5;
				mesh.uv6 = newUV6;
				mesh.uv7 = newUV7;
				mesh.uv8 = newUV8;
			#endif	

				mesh.colors32 = newColors32;
				mesh.normals = newNormals;
				mesh.tangents = newTangents;
				mesh.boneWeights = newBoneWeights;
				//mesh.bindposes = newBindposes;
				mesh.bindposes = bindposes;		// <- Here we just copy over the original bindPoses and this seems to work

				// Triangles are last
				mesh.triangles = newTriangles;

				// If we were originally using Tangents, we need to rebuild them so they work in both directions!
				if(useTangents){ mesh = MeshKit.CreateTangents(mesh); }

			}

			// Return Mesh
			return mesh;
		}

// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//	API FUNCTIONS
// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	INVERT MESH
		//	This is an overladed version of the direct InvertMesh function. This API function
		//	allows us to invert meshes at runtime on a specific GameObject or recursively on
		//	its children as well.
		////////////////////////////////////////////////////////////////////////////////////////////////

		// API SIDE FUNCTION - MeshKit.InvertMesh(a,b,c);
		public static void InvertMesh( GameObject go, bool recursive, bool optionUseMeshFilters, bool optionUseSkinnedMeshRenderers, bool enabledRenderersOnly ) {
			MeshKit.com.StartCoroutine( MeshKit.com.InvertMeshAtRuntime(go, recursive, optionUseMeshFilters, optionUseSkinnedMeshRenderers, enabledRenderersOnly) );
		}

		// ACTUAL FUNCTION
		public IEnumerator InvertMeshAtRuntime( GameObject go, bool recursive, bool optionUseMeshFilters, bool optionUseSkinnedMeshRenderers, bool enabledRenderersOnly ) {
		
			// ==================================
			//	APPLY TO THIS OBJECT ONLY
			// ==================================

			// Apply to this object only
			if( recursive == false ){

				// ------------------------
				//	MESH FILTERS
				// ------------------------

				// Helper
				bool meshFound = false;

				// Make sure this object has a MeshFilter and a mesh
				if( optionUseMeshFilters && go.GetComponent<MeshFilter>()!=null && go.GetComponent<MeshFilter>().sharedMesh != null && 
					(	enabledRenderersOnly == false ||
						enabledRenderersOnly && go.GetComponent<Renderer>() != null && go.GetComponent<Renderer>().enabled
					) 
				){
					
					// Cache the mesh
					MeshFilter mf = go.GetComponent<MeshFilter>();
					Mesh mesh = mf.mesh;

					// Run Process
					MeshKit.InvertMesh( mesh );
					meshFound = true;
				
				}

				// ------------------------
				//	SKINNED MESH RENDERERS
				// ------------------------

				// Make sure this object has a SkinnedMeshRenderer and a mesh
				if( optionUseSkinnedMeshRenderers && go.GetComponent<SkinnedMeshRenderer>()!=null && go.GetComponent<SkinnedMeshRenderer>().sharedMesh != null && 
					(	enabledRenderersOnly == false ||
						enabledRenderersOnly && go.GetComponent<SkinnedMeshRenderer>() != null && go.GetComponent<SkinnedMeshRenderer>().enabled
					) 
				){
					
					// Cache the mesh and set it up for the SkinnedMeshRenderer
					SkinnedMeshRenderer smr = go.GetComponent<SkinnedMeshRenderer>();
					Mesh mesh = (Mesh) Instantiate( smr.sharedMesh );
					smr.sharedMesh = mesh;

					// Run Process
					MeshKit.InvertMesh( mesh );
					meshFound = true;
				}

				// If this object doesn't have a mesh, show it in the console	
				if( meshFound == false ){ Debug.Log( "MESHKIT: The GameObject "+go.name+" does not have a Mesh."); }

			// ==================================
			//	APPLY TO THIS OBJECT AND CHILDREN
			// ==================================

			} else {

				// ------------------------
				//	MESH FILTERS
				// ------------------------

				// Get all the MeshFilters in this object and its children
				MeshFilter[] meshfilters = new MeshFilter[0];
				if( optionUseMeshFilters ){
					meshfilters = go.GetComponentsInChildren<MeshFilter>() as MeshFilter[];
				}

				// Loop through the MeshFilters
				if(meshfilters.Length > 0){
					foreach( MeshFilter mf2 in meshfilters ){
						if( mf2!=null && mf2.sharedMesh != null && 
							(	enabledRenderersOnly == false ||
								enabledRenderersOnly && mf2.gameObject.GetComponent<Renderer>() != null && mf2.gameObject.GetComponent<Renderer>().enabled
							)
						 ){
							
							// Cache the mesh
							Mesh mesh2 = mf2.mesh;

							// Run Process (one mesh per frame)
							MeshKit.InvertMesh( mesh2 );
							yield return 0;
						}
					}
				}

				// ------------------------
				//	SKINNED MESH RENDERERS
				// ------------------------

				// Get all the MeshFilters in this object and its children
				SkinnedMeshRenderer[] skinnedMeshRenderers = new SkinnedMeshRenderer[0];
				if( optionUseSkinnedMeshRenderers ){
					skinnedMeshRenderers = go.GetComponentsInChildren<SkinnedMeshRenderer>() as SkinnedMeshRenderer[];
				}

				// Loop through the MeshFilters
				if(skinnedMeshRenderers.Length > 0){
					foreach( SkinnedMeshRenderer smr2 in skinnedMeshRenderers ){
						if( smr2!=null && smr2.sharedMesh != null && 
							(	enabledRenderersOnly == false ||
								enabledRenderersOnly && smr2.gameObject.GetComponent<SkinnedMeshRenderer>() != null && smr2.gameObject.GetComponent<SkinnedMeshRenderer>().enabled
							)
						 ){

							// Cache the mesh and set it up for the SkinnedMeshRenderer
							Mesh mesh2 = (Mesh) Instantiate( smr2.sharedMesh );
							smr2.sharedMesh = mesh2;

							// Run Process (one mesh per frame)
							MeshKit.InvertMesh( mesh2 );
							yield return 0;
						}
					}
				}
			}
		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	MAKE DOUBLE SIDED
		//	This is an overladed version of the direct MakeDoubleSided function. This API function
		//	allows us to make double-sided meshes at runtime on a specific GameObject or recursively on
		//	its children as well.
		////////////////////////////////////////////////////////////////////////////////////////////////

		// API SIDE FUNCTION - MeshKit.MakeDoubleSided(a,b,c);
		public static void MakeDoubleSided( GameObject go, bool recursive, bool optionUseMeshFilters, bool optionUseSkinnedMeshRenderers, bool enabledRenderersOnly ) {
			MeshKit.com.StartCoroutine( MeshKit.com.MakeDoubleSidedAtRuntime(go, recursive, optionUseMeshFilters, optionUseSkinnedMeshRenderers, enabledRenderersOnly) );
		}

		// ACTUAL FUNCTION
		public IEnumerator MakeDoubleSidedAtRuntime( GameObject go, bool recursive, bool optionUseMeshFilters, bool optionUseSkinnedMeshRenderers, bool enabledRenderersOnly ) {
		
			// ==================================
			//	APPLY TO THIS OBJECT ONLY
			// ==================================

			// Apply to this object only
			if( recursive == false ){

				// ------------------------
				//	MESH FILTERS
				// ------------------------

				// Helper
				bool meshFound = false;

				// Make sure this object has a MeshFilter and a mesh
				if( optionUseMeshFilters && go.GetComponent<MeshFilter>()!=null && go.GetComponent<MeshFilter>().sharedMesh != null && 
					(	enabledRenderersOnly == false ||
						enabledRenderersOnly && go.GetComponent<Renderer>() != null && go.GetComponent<Renderer>().enabled
					) 
				){
					
					// Cache the mesh
					MeshFilter mf = go.GetComponent<MeshFilter>();
					Mesh mesh = mf.mesh;

					// Run Process
					MeshKit.MakeDoubleSidedMesh( mesh );
					meshFound = true;
				
				}

				// ------------------------
				//	SKINNED MESH RENDERERS
				// ------------------------

				// Make sure this object has a SkinnedMeshRenderer and a mesh
				if( optionUseSkinnedMeshRenderers && go.GetComponent<SkinnedMeshRenderer>()!=null && go.GetComponent<SkinnedMeshRenderer>().sharedMesh != null && 
					(	enabledRenderersOnly == false ||
						enabledRenderersOnly && go.GetComponent<SkinnedMeshRenderer>() != null && go.GetComponent<SkinnedMeshRenderer>().enabled
					) 
				){
					
					// Cache the mesh
					SkinnedMeshRenderer smr = go.GetComponent<SkinnedMeshRenderer>();
					Mesh mesh = (Mesh) Instantiate( smr.sharedMesh );
					smr.sharedMesh = mesh;

					// Run Process
					MeshKit.MakeDoubleSidedMesh( mesh );
					meshFound = true;
				
				}

				// If this object doesn't have a mesh, show it in the console	
				if( meshFound == false ){ Debug.Log( "MESHKIT: The GameObject "+go.name+" does not have a Mesh."); }

			// ==================================
			//	APPLY TO THIS OBJECT AND CHILDREN
			// ==================================

			} else {

				// ------------------------
				//	MESH FILTERS
				// ------------------------

				// Get all the MeshFilters in this object and its children
				MeshFilter[] meshfilters = new MeshFilter[0];
				if(optionUseMeshFilters){
					meshfilters = go.GetComponentsInChildren<MeshFilter>() as MeshFilter[];
				}

				// Loop through the MeshFilters
				if(meshfilters.Length > 0){
					foreach( MeshFilter mf2 in meshfilters ){
						if( mf2!=null && mf2.sharedMesh != null && 
							(	enabledRenderersOnly == false ||
								enabledRenderersOnly && mf2.gameObject.GetComponent<Renderer>() != null && mf2.gameObject.GetComponent<Renderer>().enabled
							)
						 ){
							
							// Cache the mesh
							Mesh mesh2 = mf2.mesh;

							// Run Process (one mesh per frame)
							MeshKit.MakeDoubleSidedMesh( mesh2 );
							yield return 0;
						}
					}
				}

				// ------------------------
				//	SKINNED MESH RENDERERS
				// ------------------------

				// Get all the MeshFilters in this object and its children
				SkinnedMeshRenderer[] skinnedMeshRenderers = new SkinnedMeshRenderer[0];
				if(optionUseSkinnedMeshRenderers){
					skinnedMeshRenderers = go.GetComponentsInChildren<SkinnedMeshRenderer>() as SkinnedMeshRenderer[];
				}

				// Loop through the MeshFilters
				if(skinnedMeshRenderers.Length > 0){
					foreach( SkinnedMeshRenderer smr2 in skinnedMeshRenderers ){
						if( smr2!=null && smr2.sharedMesh != null && 
							(	enabledRenderersOnly == false ||
								enabledRenderersOnly && smr2.gameObject.GetComponent<SkinnedMeshRenderer>() != null && smr2.gameObject.GetComponent<SkinnedMeshRenderer>().enabled
							)
						 ){
							
							// Cache the mesh and set it up for the SkinnedMeshRenderer
							Mesh mesh2 = (Mesh) Instantiate( smr2.sharedMesh );
							smr2.sharedMesh = mesh2;

							// Run Process (one mesh per frame)
							MeshKit.MakeDoubleSidedMesh( mesh2 );
							yield return 0;
						}
					}
				}
			}
		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	COMBINE CHILDREN
		//	This is the runtime version of the Combine Children function
		//	NOTE: Currently only supports MeshFilters
		////////////////////////////////////////////////////////////////////////////////////////////////

		// API SIDE FUNCTION - MeshKit.CombineChildren(args);
		public static void CombineChildren( GameObject go, bool optimizeMeshes, int createNewObjectsWithLayer, string createNewObjectsWithTag, bool enabledRenderersOnly, bool createNewObjectsWithMeshColliders, bool deleteSourceObjects, bool deleteObjectsWithDisabledRenderers, bool deleteEmptyObjects, int userMaxVertices = maxVertices
		){
			MeshKit.com.StartCoroutine( MeshKit.com.CombineChildrenAtRuntime(go, optimizeMeshes, createNewObjectsWithLayer, createNewObjectsWithTag, enabledRenderersOnly, createNewObjectsWithMeshColliders, deleteSourceObjects, deleteObjectsWithDisabledRenderers, deleteEmptyObjects, userMaxVertices) );
		}

		// ACTUAL FUNCTION
		public IEnumerator CombineChildrenAtRuntime( GameObject go, bool optimizeMeshes, int createNewObjectsWithLayer, string createNewObjectsWithTag, bool enabledRenderersOnly, bool createNewObjectsWithMeshColliders, bool deleteSourceObjects, bool deleteObjectsWithDisabledRenderers, bool deleteEmptyObjects, int userMaxVertices = maxVertices ){
			
			//Debug.Log( "deleteSourceObjects: " + deleteSourceObjects );
			//Debug.Log( "deleteObjectsWithDisabledRenderers: " + deleteObjectsWithDisabledRenderers );
			//Debug.Log( "deleteEmptyObjects: " + deleteEmptyObjects );

			// =====================
			//	MAIN ROUTINE
			// =====================

			// Make sure userMaxVertices doesn't exceed the actual maxVertices
			if(userMaxVertices > maxVertices ){ 
				Debug.Log("MESHKIT: Maximum vertices cannot be higher than " + maxVertices + ". Your settings have been changed.");
				userMaxVertices = maxVertices; 
			}

			// Prepare variables to store the new meshes, materials, etc.
			Matrix4x4 myTransform = go.transform.worldToLocalMatrix;
			Dictionary<Material, List<CombineInstance>> combines = new Dictionary<Material, List<CombineInstance>>();
			MeshRenderer[] meshRenderers = go.GetComponentsInChildren<MeshRenderer>(true);

			// Track Renderers That Are Disabled
			ArrayList renderersThatWereDisabled = new ArrayList();
			renderersThatWereDisabled.Clear();

			// Loop through the MeshRenderers inside of this group ...
			foreach (var meshRenderer in meshRenderers){

				// Only combine meshes that have a single subMesh
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

			// Loop through the MeshFilters inside of this group ...
			int howManyMeshCombinationsWereThere = 0;
			MeshFilter[] meshFilters = go.GetComponentsInChildren<MeshFilter>(true);
			MeshFilter[] compatibleMeshFilters = new MeshFilter[0];

			// Loop through the MeshFilters to see which ones are compatible
			foreach(var filter in meshFilters){

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
					combines[filter.GetComponent<Renderer>().sharedMaterial].Add(ci);
					renderersThatWereDisabled.Add(filter.GetComponent<Renderer>());
					filter.GetComponent<Renderer>().enabled = false;
					howManyMeshCombinationsWereThere++;
				}
			}

			// After we've sorted out MeshFilters, replace the original array with the updated one
			//Debug.Log( "Total MeshFilters: " + meshFilters.Length );
			//Debug.Log( "Compatible MeshFilters: " + compatibleMeshFilters.Length );
			meshFilters = compatibleMeshFilters;


			// Create The Combined Meshes only if there are more materials then meshes
			if(MeshKit.debug){ Debug.Log("MESHKIT: Mesh Combinations: "+ howManyMeshCombinationsWereThere); }
			if(MeshKit.debug){ Debug.Log("MESHKIT: Material Count: "+ combines.Keys.Count); }
			if( combines.Keys.Count < howManyMeshCombinationsWereThere ){

				// Loop through the materials in the "combine" list ...
				foreach(Material m in combines.Keys){

					// NOTE: We should try to scan the size of these meshes before combining them to make sure they are within the limit
					float totalVertsCount = 0f;
					foreach( CombineInstance countCI in combines[m].ToArray() ){
						totalVertsCount += countCI.mesh.vertexCount;
					}

					// If there are less than 64k (or user's settings) meshes, we can create the new GameObject normally ...
					if( totalVertsCount < userMaxVertices ){

						// Create a new GameObject based on the name of the material
						GameObject newGO = new GameObject("Combined " + m.name + "  ["+m.shader.name+"]");
						newGO.transform.parent = go.transform;
						newGO.transform.localPosition = Vector3.zero;
						newGO.transform.localRotation = Quaternion.identity;
						newGO.transform.localScale = Vector3.one;

						MeshFilter filter = newGO.AddComponent<MeshFilter>();
						filter.mesh.CombineMeshes(combines[m].ToArray(), true, true);
					 
						MeshRenderer renderer = newGO.AddComponent<MeshRenderer>();
						renderer.material = m;

						// Set Layer and Tag
						if(createNewObjectsWithLayer >= 0){ newGO.layer = createNewObjectsWithLayer; }
						if(createNewObjectsWithTag!=""){ newGO.tag = createNewObjectsWithTag;}

						// Create Mesh Colliders
						if( createNewObjectsWithMeshColliders ){ newGO.AddComponent<MeshCollider>(); }

					// Otherwise we need to break apart this CombinedInstance and create seperate ones within the limits.
					} else {

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
								GameObject newGO = new GameObject("Combined_" + m.name + "_"+arraysCreated.ToString() +"  ["+m.shader.name+"]");
								newGO.transform.parent = go.transform;
								newGO.transform.localPosition = Vector3.zero;
								newGO.transform.localRotation = Quaternion.identity;
								newGO.transform.localScale = Vector3.one;

								// Convert the CombineInstance into the builtin array and combine the meshes
								MeshFilter filter = newGO.AddComponent<MeshFilter>();
								CombineInstance[] newCombineArr = (CombineInstance[]) newCombineInstance.ToArray( typeof( CombineInstance ) );
								filter.mesh.CombineMeshes( newCombineArr, true, true );

								// Setup Renderer
								MeshRenderer renderer = newGO.AddComponent<MeshRenderer>();
								renderer.material = m;

								// Set Layer and Tag
								if(createNewObjectsWithLayer >= 0 ){ newGO.layer = createNewObjectsWithLayer; }
								if(createNewObjectsWithTag != string.Empty ){ newGO.tag = createNewObjectsWithTag;}

								// Create Mesh Colliders
								if( createNewObjectsWithMeshColliders ){ newGO.AddComponent<MeshCollider>(); }

								// Reset The array to build the next group
								currentVertsCount = 0f;
								newCombineInstance = new ArrayList(); 
								newCombineInstance.Clear();

								// Increment the array count
								arraysCreated++;
							}

							// ----------------------------------------------
							// ADD THE NEW MESH TO THE ARRAY IF THERES ROOM
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
									GameObject newGO2 = new GameObject("Combined_" + m.name + "_"+arraysCreated.ToString() +"  ["+m.shader.name+"]");
									newGO2.transform.parent = go.transform;
									newGO2.transform.localPosition = Vector3.zero;
									newGO2.transform.localRotation = Quaternion.identity;
									newGO2.transform.localScale = Vector3.one;

									// Convert the CombineInstance into the builtin array and combine the meshes
									MeshFilter filter = newGO2.AddComponent<MeshFilter>();
									CombineInstance[] newCombineArr2 = (CombineInstance[]) newCombineInstance.ToArray( typeof( CombineInstance ) );
									filter.mesh.CombineMeshes( newCombineArr2, true, true );

									// Setup Renderer
									MeshRenderer renderer = newGO2.AddComponent<MeshRenderer>();
									renderer.material = m;

									// Set Layer and Tag
									if(createNewObjectsWithLayer >= 0){ newGO2.layer = createNewObjectsWithLayer; }
									if(createNewObjectsWithTag != string.Empty){ newGO2.tag = createNewObjectsWithTag;}

									// Create Mesh Colliders
									if( createNewObjectsWithMeshColliders ){ newGO2.AddComponent<MeshCollider>(); }

								}
							}

							// ----------------------------------------------
							// IF A SINGLE MESH IS TOO BIG TO BE ADDED
							// ----------------------------------------------

							else if( countCI.mesh.vertexCount >= maxVertices ){
								// Show warnings for meshes that are too large
								Debug.Log("MESHKIT: MeshKit detected a Mesh called \"" + countCI.mesh.name + "\" with "+ countCI.mesh.vertexCount + " vertices using the material \""+ m.name + "\". This is beyond Unity's limitations and cannot be combined. This mesh was skipped.");
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

					// Loop through the original Renderers list ...
					foreach (var meshRenderer2 in meshRenderers){

						// Skip this meshRenderer if it has a MeshFilter with submeshes
						if ( meshRenderer2.gameObject.GetComponent<MeshFilter>() &&
							meshRenderer2.gameObject.GetComponent<MeshFilter>().sharedMesh != null &&
							meshRenderer2.gameObject.GetComponent<MeshFilter>().sharedMesh.subMeshCount > 1){
							continue;
						}

						// If this original object didn't have a collider, it is no longer needed and can be destroyed.
						else if( meshRenderer2.gameObject.GetComponent<Collider>() == null ){
							Destroy(meshRenderer2.gameObject);
						
						// Otherwise, destroy uneeded Components ...
						} else {

							// Otherwise, Destroy MeshRenderers
							if( meshRenderer2.gameObject.GetComponent<MeshRenderer>() != null ){
								Destroy(meshRenderer2.gameObject.GetComponent<MeshRenderer>());
							}

							// Destroy MeshFilters
							if( meshRenderer2.gameObject.GetComponent<MeshFilter>() != null ){
								Destroy(meshRenderer2.gameObject.GetComponent<MeshFilter>());
							}
						}
					}
				}

				// Delete Any object with disabled Renderers
				if( deleteObjectsWithDisabledRenderers ){
					Renderer[] theRenderers = go.GetComponentsInChildren<Renderer>(true) as Renderer[];
					foreach(Renderer r in theRenderers){
						if( r!=null && r.enabled == false && r.gameObject != null){
							Destroy(r.gameObject);
						}
					}
				}

				// If we should delete Empty Objects ...
				if( deleteEmptyObjects ){
					
					// Loop through all the transforms and destroy any blank objects
					bool foundEmptyObjects = false;			// We use this to help delete old objects
					while( foundEmptyObjects == false ){

						// Reset the flag on each iteration...
						foundEmptyObjects = false;

						// Loop through all the transforms and destroy any blank objects
						Transform[] theTransforms = go.GetComponentsInChildren<Transform>(true) as Transform[];

						foreach(Transform t in theTransforms){
							if(t.gameObject.GetComponents<Component>().Length == 1 && t.childCount == 0 ){
								Destroy( t.gameObject as GameObject );
								foundEmptyObjects = true;
							}
						}

						// Wait until next frame ...
						yield return 0;
					}
				}
				

			// If we aren't recreating more Meshes, restore the MeshRenderers	
			} else {
				if(MeshKit.debug){ Debug.Log("MESHKIT: No meshes require combining in this group ..."); }
				foreach( MeshRenderer mr in renderersThatWereDisabled ){
					if(mr!=null){ mr.enabled = true; }
				}
			}
		}

////////////// ->>>

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	LIST CONTAINS FUNCTION
		//	Checks to see if this combination of materials and Mesh already exist in the ArrayList
		//	If it does, we add the new mesh to the existing setup as an extra GameObject
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static bool ListContains( ArrayList list, Material[] key, Mesh originalMesh, MeshFilter mf ){
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
		//	SEPERATE MESHES AT RUNTIME
		////////////////////////////////////////////////////////////////////////////////////////////////
		
		// Class that helps organise the Separate Meshes Function
		public class BatchMeshes {
			public Material[] key;			// We use this as the identifier - this is the material array being used on each object.
			public Mesh originalMesh;		// The original Mesh of this gameObject
			public Mesh[] splitMeshes;		// When the MeshSplitter finishes building the meshes, it will return them here.
			public ArrayList gos;			// The list of GameObjects in the scene using this setup, we'll replace them after the batch is built.
		}

		// Main Function
	    public static void SeparateMeshes( GameObject go, bool onlyApplyToEnabledRenderers, bool stripUnusedVertices ){

	    	// Recreate the ArrayList
	    	ArrayList list = new ArrayList();
	    	list.Clear();

	    	// Loop through all the MeshFilters in the selected Object
			MeshFilter[] theMFs = go.GetComponentsInChildren<MeshFilter>();
			if(MeshKit.debug){ Debug.Log(theMFs.Length + " MeshFilters found for processing ..."); }
			int numberOfWorkableMFs = 0;
			foreach(MeshFilter mf in theMFs){

				// We need to find MeshFilters which have more than 1 Submesh:
				if( mf != null && mf.sharedMesh != null && mf.sharedMesh.subMeshCount > 1 ){

					// This Shared Mesh has more than 1 SubMesh, but we need to access its materials too ...
					if( mf.gameObject.GetComponent<MeshRenderer>() != null &&
						mf.gameObject.GetComponent<MeshRenderer>().sharedMaterials.Length > 1 &&
						// Make sure the renderer is enabled to help prevent double baking!
						(!onlyApplyToEnabledRenderers || onlyApplyToEnabledRenderers && mf.gameObject.GetComponent<MeshRenderer>().enabled == true)
					){
						// If this setup doesn't exist in the ArrayList, add it!
						if( !ListContains(list, mf.gameObject.GetComponent<MeshRenderer>().sharedMaterials, mf.sharedMesh, mf ) ){

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

			if(MeshKit.debug){ Debug.Log( "numberOfWorkableMFs: "+ numberOfWorkableMFs ); }
			if( numberOfWorkableMFs == 0){
				Debug.Log("MESHKIT: No objects require seperating into independant meshes.");
			}

			
			// SEND EACH BATCH TO BE PROCESSED
			if( list!=null && list.Count > 0 ){
				int id = 0;

				foreach( BatchMeshes bm2 in list ){
					/*
					if( bm2 != null && bm2.gos[0] != null && (bm2.gos[0] as GameObject).GetComponent<MeshFilter>() != null ){
						
						// Split The Mesh based on the first GameObject and then return the splitMeshes
						bm2.splitMeshes = MeshKit.SplitMesh( (bm2.gos[0] as GameObject).GetComponent<MeshFilter>().sharedMesh as Mesh );

						// Rebuild GameObjects With New Meshes
						if(bm2.splitMeshes.Length > 0 ){ RebuildSeparatedObjects(bm2); } 

						// increment ID
						id++; 	
					}
					*/

					// Make sure bm2 is valid ...
					if( bm2 != null ){

						// Cache the GameObject first
						GameObject bm2Go = bm2.gos[0] as GameObject;

						// If MeshFilter can be accessed ...
						if( bm2Go != null && bm2Go.GetComponent<MeshFilter>() != null ){

							// Cache MeshFilter
							MeshFilter bm2GoMF = bm2Go.GetComponent<MeshFilter>();

							// Split The Mesh based on the first GameObject and then return the splitMeshes
							bm2.splitMeshes = MeshKit.SplitMesh( bm2GoMF.sharedMesh as Mesh, stripUnusedVertices );

							// Rebuild GameObjects With New Meshes
							if(bm2.splitMeshes.Length > 0 ){ RebuildSeparatedObjects(bm2); } 

							// increment ID
							id++; 	
							
						}
					}
				}
			}

		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	REBUILD SEPERATED OBJECTS
		////////////////////////////////////////////////////////////////////////////////////////////////

	    public static void RebuildSeparatedObjects( BatchMeshes bm ){
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

	    					// Turn off the original MeshRenderer when we're done
	    					go.GetComponent<MeshRenderer>().enabled = false;	
	    					
	    				}
	    			}
	    		}
			}
	    }

	    ////////////////////////////////////////////////////////////////////////////////////////////////
		//	BATCH REBUILD MESH
		//	Allows a Mesh to be rebuilt with / without normals, tangents, etc.
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static void Rebuild( GameObject go, bool recursive, bool optionUseMeshFilters, bool optionUseSkinnedMeshRenderers,  bool optionStripNormals, bool optionStripTangents, bool optionStripColors, bool optionStripUV2, bool optionStripUV3, bool optionStripUV4, bool optionStripUV5, bool optionStripUV6, bool optionStripUV7, bool optionStripUV8, bool optionRebuildNormals, bool optionRebuildTangents, float rebuildNormalsAngle = -1 ){

			// Fix option configuration
			if(optionStripNormals){ optionStripTangents = true; } 		// Always strip tangents if we're stripping normals
			if(optionRebuildTangents ){ optionRebuildNormals = true; } 	// Always rebuild normals if there are none and we are trying to build tangents.

			// -------------
			//	MESHFILTERS
			// -------------

			// Find all the MeshFilters in this GameObject
			MeshFilter[] theMeshFilters = new MeshFilter[0];
			if( optionUseMeshFilters ){
				if( recursive ){
					theMeshFilters = go.GetComponentsInChildren<MeshFilter>() as MeshFilter[];
				} else {
					theMeshFilters = go.GetComponents<MeshFilter>() as MeshFilter[];
				}
			}

			// Loop through the MeshFilters
			foreach( MeshFilter m in theMeshFilters){
				
				// if the MeshFilter is valid, has a sharedMesh and doesnt have any subMeshes ...
				if( m != null && m.sharedMesh != null && m.sharedMesh.subMeshCount == 1 ){

					// Attempt to rebuild the mesh
					Mesh newMesh = RebuildMesh(m.sharedMesh, optionStripNormals, optionStripTangents, optionStripColors, optionStripUV2, optionStripUV3, optionStripUV4, optionStripUV5, optionStripUV6, optionStripUV7, optionStripUV8, optionRebuildNormals, optionRebuildTangents, rebuildNormalsAngle );

					// If the newMesh was crated succesfully ...
					if(newMesh!=null){
						
						// Apply it to the MeshFilter ...
						m.sharedMesh = newMesh;

					} else {
						Debug.Log("MESHKIT: Couldn't rebuild MeshFilter's Mesh on GameObject: "+ m.gameObject.name);
					}
				}
			}

			// ------------------------
			//	SKINNED MESH RENDERERS
			// ------------------------

			// Find all the MeshFilters in this GameObject
			SkinnedMeshRenderer[] theSkinnedMeshRenderers = new SkinnedMeshRenderer[0];
			if( optionUseSkinnedMeshRenderers ){
				if( recursive ){
					theSkinnedMeshRenderers = go.GetComponentsInChildren<SkinnedMeshRenderer>() as SkinnedMeshRenderer[];
				} else {
					theSkinnedMeshRenderers = go.GetComponents<SkinnedMeshRenderer>() as SkinnedMeshRenderer[];
				}
			}

			// Loop through the MeshFilters
			foreach( SkinnedMeshRenderer m in theSkinnedMeshRenderers){
				
				// if the MeshFilter is valid, has a sharedMesh and doesnt have any subMeshes ...
				if( m != null && m.sharedMesh != null && m.sharedMesh.subMeshCount == 1 ){

					// Attempt to rebuild the mesh
					Mesh newMesh = RebuildMesh(m.sharedMesh, optionStripNormals, optionStripTangents, optionStripColors, optionStripUV2, optionStripUV3, optionStripUV4, optionStripUV5, optionStripUV6, optionStripUV7, optionStripUV8, optionRebuildNormals, optionRebuildTangents, rebuildNormalsAngle );

					// If the newMesh was crated succesfully ...
					if(newMesh!=null){
						
						// Apply it to the MeshFilter ...
						m.sharedMesh = newMesh;

					} else {
						Debug.Log("MESHKIT: Couldn't rebuild SkinnedMeshRenderer's Mesh on GameObject: "+ m.gameObject.name);
					}
				}
			}
		}

// MESH KIT v2.0 ->

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	DECIMATE SKINNED MESH
		//	Decimates a mesh on a Skinned Mesh Renderer at runtime
		////////////////////////////////////////////////////////////////////////////////////////////////

		// Decimate A Skinned Mesh
		public static Mesh DecimateMesh( SkinnedMeshRenderer smr, float quality, bool recalculateNormals, bool preserveBorders = false, bool preserveSeams = false, bool preserveFoldovers = false ){
			if( smr != null && smr.sharedMesh != null ){

				// Helper variables
				var rendererTransform = smr.transform;
				var mesh = smr.sharedMesh;
				var meshTransform = smr.transform.worldToLocalMatrix * rendererTransform.localToWorldMatrix;

				// Keep quality in range
				quality = Mathf.Clamp01(quality);

				// Decimate the mesh
				Mesh decimatedMesh = MeshDecimatorUtility.DecimateMesh(mesh, meshTransform, quality, recalculateNormals, null, preserveBorders, preserveSeams, preserveFoldovers);
				
				// If the mesh was decimated successfully, return the mesh
				if( decimatedMesh != null ){

					smr.sharedMesh = decimatedMesh;
					return decimatedMesh;

				} else {

					// Warning Logs
					Debug.LogWarning("MESHKIT: A skinned mesh couldn't be decimated. Skipping ..." );
					return null;
				}	
			}

			// Warning Logs
			Debug.LogWarning("MESHKIT: SkinnedMeshRenderer or its shared mesh was null. Skipping ..." );

			// Otherwise, return null
			return null;
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		//	DECIMATE MESH FILTER MESH
		//	Decimates a mesh on a Mesh Filter at runtime
		////////////////////////////////////////////////////////////////////////////////////////////////

		// Decimate A MeshFilter Mesh
		public static Mesh DecimateMesh( MeshFilter mf, float quality, bool recalculateNormals, bool preserveBorders = false, bool preserveSeams = false, bool preserveFoldovers = false ){
			if( mf != null && mf.sharedMesh != null ){

				// Helper variables
				var rendererTransform = mf.transform;
				var mesh = mf.sharedMesh;
				var meshTransform = mf.transform.worldToLocalMatrix * rendererTransform.localToWorldMatrix;

				// Keep quality in range
				quality = Mathf.Clamp01(quality);

				// Decimate the mesh
				Mesh decimatedMesh = MeshDecimatorUtility.DecimateMesh(mesh, meshTransform, quality, recalculateNormals, null, preserveBorders, preserveSeams, preserveFoldovers);
				
				// If the mesh was decimated successfully, return the mesh
				if( decimatedMesh != null ){

					mf.sharedMesh = decimatedMesh;
					return decimatedMesh;

				} else {

					// Warning Logs
					Debug.LogWarning("MESH KIT DECIMATOR: A mesh couldn't be decimated. Skipping ..." );
					return null;
				}	
			}

			// Warning Logs
			Debug.LogWarning("MESH KIT DECIMATOR: MeshFilter or its shared mesh was null. Skipping ..." );

			// Otherwise, return null
			return null;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	DECIMATE MESH
		//	This is an overloaded version of the direct DecimateMesh function. This API function
		//	allows us to invert meshes at runtime on a specific GameObject or recursively on
		//	its children as well.
		////////////////////////////////////////////////////////////////////////////////////////////////

		// API SIDE FUNCTION - MeshKit.DecimateMesh(a,b,c);
		public static void DecimateMesh( GameObject go, bool recursive, bool optionUseMeshFilters, bool optionUseSkinnedMeshRenderers, bool enabledRenderersOnly, float quality, bool recalculateNormals, bool preserveBorders = false, bool preserveSeams = false, bool preserveFoldovers = false ) {
			MeshKit.com.StartCoroutine( MeshKit.com.DecimateMeshAtRuntime(go, recursive, optionUseMeshFilters, optionUseSkinnedMeshRenderers, enabledRenderersOnly, quality, recalculateNormals, preserveBorders, preserveSeams, preserveFoldovers) );
		}

		// ACTUAL FUNCTION
		public IEnumerator DecimateMeshAtRuntime( GameObject go, bool recursive, bool optionUseMeshFilters, bool optionUseSkinnedMeshRenderers, bool enabledRenderersOnly, float quality, bool recalculateNormals, bool preserveBorders = false, bool preserveSeams = false, bool preserveFoldovers = false ) {
		
			// ==================================
			//	APPLY TO THIS OBJECT ONLY
			// ==================================

			// Apply to this object only
			if( recursive == false ){

				// Make sure this object has a MeshFilter/SkinnedMeshRenderer and a mesh
				if(
					(
						optionUseMeshFilters && go.GetComponent<MeshFilter>()!=null && go.GetComponent<MeshFilter>().sharedMesh != null ||
						optionUseSkinnedMeshRenderers && go.GetComponent<SkinnedMeshRenderer>()!=null && go.GetComponent<SkinnedMeshRenderer>().sharedMesh
					
					) && (

						enabledRenderersOnly == false ||
						enabledRenderersOnly && go.GetComponent<Renderer>() != null && go.GetComponent<Renderer>().enabled
					) 
				){
					
					// Check for Skinned Mesh Renderer and make sure its valid
					if( optionUseSkinnedMeshRenderers && go.GetComponent<SkinnedMeshRenderer>() != null && 
						( enabledRenderersOnly == false || enabledRenderersOnly && go.GetComponent<SkinnedMeshRenderer>().enabled )
					){

						// Run Process
						MeshKit.DecimateMesh( 
							go.GetComponent<SkinnedMeshRenderer>(), quality, recalculateNormals, preserveBorders, preserveSeams, preserveFoldovers
						);
					
					// Check for Mesh Filter
					} else if( optionUseMeshFilters && go.GetComponent<MeshFilter>() != null && 
						( enabledRenderersOnly == false || enabledRenderersOnly && go.GetComponent<Renderer>().enabled )
					){

						// Run Process
						MeshKit.DecimateMesh( 
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

				// Get all the MeshFilters in this object and its children (if we're selecting mesh filters)
				MeshFilter[] meshfilters = new MeshFilter[0];
				if( optionUseMeshFilters  ){
					meshfilters = go.GetComponentsInChildren<MeshFilter>() as MeshFilter[];
				}

				// Loop through the MeshFilters
				if(meshfilters.Length > 0){
					foreach( MeshFilter mf in meshfilters ){
						if( mf!=null && mf.sharedMesh != null && 
							(	enabledRenderersOnly == false ||
								enabledRenderersOnly && mf.gameObject.GetComponent<Renderer>() != null && mf.gameObject.GetComponent<Renderer>().enabled
							)
						 ){

						 	// Run Process (one mesh per frame)
							MeshKit.DecimateMesh( 
								mf, quality, recalculateNormals, preserveBorders, preserveSeams, preserveFoldovers
							);
							
							// Wait 1 frame
							yield return 0;
						}
					}
				}

				// ------------------------
				//	SKINNED MESH RENDERERS
				// ------------------------

				// Get all the MeshFilters in this object and its children ( if we're selecting Skinned Mesh Renderers)
				SkinnedMeshRenderer[] skinnedMeshRenderers = new SkinnedMeshRenderer[0];
				if( optionUseSkinnedMeshRenderers  ){
					skinnedMeshRenderers = go.GetComponentsInChildren<SkinnedMeshRenderer>() as SkinnedMeshRenderer[];
				}

				// Loop through the MeshFilters
				if(skinnedMeshRenderers.Length > 0){
					foreach( SkinnedMeshRenderer smr in skinnedMeshRenderers ){
						if( smr!=null && smr.sharedMesh != null && 
							(	enabledRenderersOnly == false ||
								enabledRenderersOnly && smr.enabled == true
							)
						 ){

						 	// Run Process (one mesh per frame)
							MeshKit.DecimateMesh( 
								smr, quality, recalculateNormals, preserveBorders, preserveSeams, preserveFoldovers
							);
							
							// Wait 1 frame
							yield return 0;
						}
					}
				}
			}
		}	

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	AUTO-LOD CLASSES AND SUPPORT METHODS
		//	We map MeshKit.AutoLODSettings to LODGenerator.LODSettings
		////////////////////////////////////////////////////////////////////////////////////////////////

		// For ease of use, users can create AutoLODSettings to dynamically create AutoLODs.
		// This basically maps an easier to access class to the LODGenerator's LODSettings struct.
		[System.Serializable]
		public class AutoLODSettings {
			
			[Header("LOD Distance")]
			[Range(0.01f, 100f)]
			[Tooltip("At what distance should this LOD be shown? 100 is used for the best quality mesh.")]
			public float lodDistancePercentage = 50f;
			
			[Header("Decimation")]
			[Range(0.01f, 1f)]
			[Tooltip("When decimating, a value of 0 will reduce mesh complexity as much as possible. 1 will preserve it.")]
			public float quality = 0.8f;

			[Header("Renderers")]
			[Tooltip("The Skin Quality setting used in the Renderer.")]
			public SkinQuality skinQuality = SkinQuality.Auto;

			[Tooltip("The Recieve Shadows setting used in the Renderer.")]
			public bool receiveShadows = true;

			[Tooltip("The Shadow Casting setting used in the Renderer.")]
			public ShadowCastingMode shadowCasting = ShadowCastingMode.On;

			[Tooltip("The Motion Vectors setting used in the Renderer.")]
			public MotionVectorGenerationMode motionVectors = MotionVectorGenerationMode.Object;

			[Tooltip("The Skinned Motion Vectors setting used in the Renderer.")]
			public bool skinnedMotionVectors = true;

			[Tooltip("The Light Probe Usage setting found in the Renderer.")]
			public LightProbeUsage lightProbeUsage = LightProbeUsage.BlendProbes;

			[Tooltip("The Reflection Probe Usage setting found in the Renderer.")]
			public ReflectionProbeUsage reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;

			// Constructor => ( distance, quality )
			// NOTES: quality and distance is in a different order to LODSettings in LODGenerator
			public AutoLODSettings ( float lodDistancePercentageValue, float qualityValue = 0.8f ){
				
				this.lodDistancePercentage = lodDistancePercentageValue;
				this.quality = qualityValue;

				this.skinQuality = SkinQuality.Auto;
				this.receiveShadows = true;
				this.shadowCasting = ShadowCastingMode.On;
				this.motionVectors = MotionVectorGenerationMode.Object;
				this.skinnedMotionVectors = true;

				this.lightProbeUsage = LightProbeUsage.BlendProbes;
				this.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;

			}

			// Constructor => ( All values )
			// NOTES: quality and distance is in a different order to LODSettings in LODGenerator
			public AutoLODSettings( float lodDistancePercentage, float quality, SkinQuality skinQuality, bool receiveShadows, ShadowCastingMode shadowCasting, MotionVectorGenerationMode motionVectors, bool skinnedMotionVectors, LightProbeUsage lightProbeUsage = LightProbeUsage.BlendProbes, ReflectionProbeUsage reflectionProbeUsage = ReflectionProbeUsage.BlendProbes)
			{
				
				this.lodDistancePercentage = lodDistancePercentage;
				this.quality = quality;

				this.skinQuality = skinQuality;
				this.receiveShadows = receiveShadows;
				this.shadowCasting = shadowCasting;
				this.motionVectors = motionVectors;
				this.skinnedMotionVectors = skinnedMotionVectors;

				this.lightProbeUsage = lightProbeUsage;
				this.reflectionProbeUsage = reflectionProbeUsage;
			}

			// Convert the MeshKit.AutoLODSettings to LODGenerator.LODSettings
			public LODSettings ToLODSettings(){
				return new LODSettings( quality, lodDistancePercentage, skinQuality, receiveShadows, shadowCasting, motionVectors,  skinnedMotionVectors, lightProbeUsage, reflectionProbeUsage);
			}
		}

		// Convert the AutoLODSetttings[] array to a LODSettings[] array
		public static LODSettings[] AutoLODSetttingsToLODSettings( AutoLODSettings[] autoLODSettings ){

			// Return an empty array if there is something wrong with the LODSetup
			if( autoLODSettings == null || autoLODSettings.Length == 0 ){ return new LODSettings[0]; }
			
			// Otherwise, lets prepare it
			LODSettings[] lodSettings = new LODSettings[autoLODSettings.Length];
			for( int i = 0; i < lodSettings.Length; i++ ){
				lodSettings[i] = autoLODSettings[i].ToLODSettings();
			}

			// Return the lod settings
			return lodSettings;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	AUTO-LOD METHODS
		//	Creates LOD objects for a GameObject at runtime
		////////////////////////////////////////////////////////////////////////////////////////////////

		// AUTO LOD OVERLOAD 1
		// Create Auto LODs on a GameObject (Simple version) Examples:
		// AutoLOD( gameObject );  or AutoLOD( gameObject, false, false, false );
		public static void AutoLOD( GameObject go, bool preserveBorders = false, bool preserveSeams = false, bool preserveFoldovers = false ){

			// Recreate the needed arguments and send it to the main method below
			AutoLOD( go,
				new LODSettings[] {
					new LODSettings(0.8f, 50f, SkinQuality.Auto, true, ShadowCastingMode.On),
			 		new LODSettings(0.65f, 16f, SkinQuality.Bone2, true, ShadowCastingMode.Off, MotionVectorGenerationMode.Object, false),
					new LODSettings(0.4f, 7f, SkinQuality.Bone1, false, ShadowCastingMode.Off, MotionVectorGenerationMode.Object, false)
				},
				1f, preserveBorders, preserveSeams, preserveFoldovers );
		}

		// AUTO LOD OVERLOAD 2 ( using AutoLODSettings )
		// Create Auto LODs on a GameObject - Converts AutoLODSettings to LODSettings on the fly
		public static void AutoLOD( GameObject go, AutoLODSettings[] levels, float cullingDistance = 1f, bool preserveBorders = false, bool preserveSeams = false, bool preserveFoldovers = false ){

			// Recreate the needed arguments and send it to the main method below
			AutoLOD( go, AutoLODSetttingsToLODSettings( levels ), cullingDistance, preserveBorders, preserveSeams, preserveFoldovers );
		}

		// Create Auto LODs on a GameObject - Full method
		public static void AutoLOD( GameObject go, LODSettings[] levels, float cullingDistance = 1f, bool preserveBorders = false, bool preserveSeams = false, bool preserveFoldovers = false ){

			// =============================
			//	VERIFY THE SETUP
			// =============================

			// Cache all the LODGroups, MeshFilters and SkinnedMeshRenders on this GameObject
			LODGroup[] lodGroups = go.GetComponentsInChildren<LODGroup>() as LODGroup[];
			MeshFilter[] meshfilters = go.GetComponentsInChildren<MeshFilter>() as MeshFilter[];
			SkinnedMeshRenderer[] skinnedMeshRenderers = go.GetComponentsInChildren<SkinnedMeshRenderer>() as SkinnedMeshRenderer[];

			// levels argument isn't setup correctly
			if( levels == null || levels.Length == 0 ){
				Debug.LogWarning("MESHKIT: Cannot create LODs on GameObject " + go.name + " because no LODSettings were passed.");
				return;

			// Mixed MeshFilters and Mesh Renderers
			} else if( lodGroups.Length > 0  ){
				Debug.LogWarning("MESHKIT: Cannot create LODs on GameObject " + go.name + " because it already contains LOD Groups.");
				return;

			// Mixed MeshFilters and Mesh Renderers
			} else if( meshfilters.Length > 0 && skinnedMeshRenderers.Length > 0 ){
				Debug.LogWarning("MESHKIT: Cannot create LODs on GameObject " + go.name + " because it contains both Mesh Filters and Skinned Mesh Renderers.");
				return;

			// Multiple Skinned Mesh Renderers( multiple mesh filters seem to work fine )
			} else if ( skinnedMeshRenderers.Length > 1 ){
				Debug.LogWarning("MESHKIT: Cannot create LODs on GameObject " + go.name + " because it contains multiple Skinned Mesh Renderers.");
				return;
			
			// There is one Skinned Mesh Renderer but it is on a child object
			} else if ( meshfilters.Length == 0 && skinnedMeshRenderers.Length == 1 && go.GetComponent<SkinnedMeshRenderer>() == null ){
				Debug.LogWarning("MESHKIT: Cannot create LODs on GameObject " + go.name + " because it contains multiple Skinned Mesh Renderers.");
				return;
			}

			// ====================
			//	FIX LOD DISTANCES
			// ====================

			// Loop through the LOD Levels
			for( int i = 0; i < levels.Length; i++ ){
				// Make sure the LOD Distance is never greater than the previous LOD
				if( i > 0 && levels[i].lodDistancePercentage > levels[i-1].lodDistancePercentage){
					levels[i].lodDistancePercentage = levels[i-1].lodDistancePercentage - 0.01f;
					if( levels[i].lodDistancePercentage < 0f ){ levels[i].lodDistancePercentage = 0f; }
				}
			}

			// Make sure the Culling LOD Distance is never greater than the last LOD
			if( levels.Length >= 1 &&
				cullingDistance >= levels[levels.Length-1].lodDistancePercentage 
			){
				cullingDistance = levels[levels.Length-1].lodDistancePercentage - 0.01f;
				if( cullingDistance < 0f ){ cullingDistance = 0f; }
			}

			// =============================
			//	GENERATE THE LODs
			// =============================

			// If everything checks out, generate the LODs for this GameObject
			LODGenerator.GenerateLODs( go, levels, null, preserveBorders, preserveSeams, preserveFoldovers );
			

			// =============================
			//	UPDATE LODGROUP PERCENTAGES
			// =============================

			// Update LODGroup Percentages
			LODGroup lodGroup = go.GetComponent<LODGroup>();
			if (lodGroup != null ) {

				// Cache the current LODs from the LODGroup
				LOD[] lods = lodGroup.GetLODs() as LOD[];

				// Make sure that the LOD Lengths are valid ...
				if( lods.Length == levels.Length + 1 ){	// <- the LODGroup has an extra LOD at the end used for culling distances
					
					// Loop through the lods
					for( int i = 0; i < lods.Length; i++ ){

						// Update all LOD Percentages ( except for the last one, which is the cull value)
						if (i < lods.Length-1 ) {
							lods[i].screenRelativeTransitionHeight = levels[i].lodDistancePercentage * 0.01f;		
						}

						// We should add the cull value here.
						else if (i == lods.Length-1 ) {
							lods[i].screenRelativeTransitionHeight = cullingDistance * 0.01f;	
						}
					}

					// Now reset the LODS again
					lodGroup.SetLODs( lods );
				
				} else {
					Debug.Log( "MESHKIT: The LODGroup lengths don't match on GameObject: " + go.name + ". Couldn't update custom distances.");
				}

			}
		}

	}
}
