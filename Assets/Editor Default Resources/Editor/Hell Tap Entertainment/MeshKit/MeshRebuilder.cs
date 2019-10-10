////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MeshRebuilder.cs
//
//	Tool that optimizes, strips and recreates a mesh based on the vertices that are being
//	referenced by the triangles. Can also perform operations like Invert, Make Double-Sided, etc.
//
//	© 2015 Melli Georgiou.
//	Hell Tap Entertainment LTD
//
////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using HellTap.MeshKit;

// Use HellTap Namespace
namespace HellTap.MeshKit {

	// Class
	public class MeshRebuilder : Editor {

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	STRIP UNUSED VERTICES
		//	Rebuilds The Mesh, stripping unused vertices and preserving the existing mesh data.
		////////////////////////////////////////////////////////////////////////////////////////////////

		// Strip Unused Vertices From The New Mesh
		public static Mesh StripUnusedVertices( Mesh m, bool optimize, int index  ){
			return Strip(m, optimize, index, false, false, false, false, false, false, false, false, false, false, false, false, false );
		}

		// Strip Unused Vertices From The New Mesh - full options
		public static Mesh Strip( Mesh m, bool optimize, int index, bool stripNormals, bool stripTangents, bool stripColors, bool stripUV,bool stripUV2, bool stripUV3, bool stripUV4, bool stripUV5, bool stripUV6, bool stripUV7, bool stripUV8, bool stripBoneWeights, bool stripBindPoses ){

			// Use to count total stripping time in Debug Mode
			double startTime = EditorApplication.timeSinceStartup; 

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

				// Debug
				if(MeshKitGUI.verbose){ Debug.Log("New Vertex Index Created For SubMesh: "+index + ". It totals: "+ newVertsIndex.Length ); }

				
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

				// DEBUG
				if(MeshKitGUI.verbose){
					string debugTriangles = "";
					foreach( int tri2 in newTriangles ){
						debugTriangles += tri2.ToString()+", ";
					}
					Debug.Log("B - TRIANGLE GROUP "+ index +" Length: " + newTriangles.Length +  " [ " + debugTriangles +"]");

					string debugVerts = "";
					foreach( Vector3 v in newVerts ){
						debugVerts += v.ToString()+"\n";
					}
					Debug.Log("B - VERTS GROUP "+ index +" Length: " + newVerts.Length + "  [ " + debugVerts + "]") ;

					string debugUV = "";
					foreach( Vector2 uv in newUV ){
						debugUV += uv.ToString()+"\n";
					}
					Debug.Log("B - UV GROUP "+ index +" Length: " + newUV.Length + "  [ " + debugUV + "]") ;

					Debug.Log("Total Strip Time: "+ (EditorApplication.timeSinceStartup - startTime) );
				}
				
			}

			// Return Mesh after it has been processed...
			return m;
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
				Debug.LogWarning("MESH REBUILDER: Tangents couldn't be created for a Mesh because it didn't have UVs! Skipping ...");
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
		//	Makes the Mesh inside-out
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
		//	IS MESH DOUBLE-SIDED?
		//	Scans a mesh to see if it is double sided
		//	NOTE: this may not always return true if the mesh has been altered in some other way.
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static bool IsMeshDoubleSided( Mesh m ){
			
			// If Mesh is not null and doesnt have any submeshes ...
			if( m != null && m.subMeshCount == 1 ){

				// Get Half the number of triangles and make sure it can be divided by 3
				int triCount = (int)(m.triangles.Length * 0.5);
				if( triCount % 3 == 0 ){

					// Cache the triangles
					int[] tris = m.triangles; 
					Vector3[] verts = m.vertices; 

					// Debug Info
					string debugTriangles = "";
					string debugVertices = "";
					if( MeshKitGUI.verbose ){
						for (int x=0; x< m.triangles.Length; x+=3){
							debugTriangles += tris[x].ToString() +", "+ tris[x+1].ToString() +", "+ tris[x+2].ToString() + "\n";
							debugVertices += verts[tris[x]].ToString() + ", " + verts[tris[x+1]].ToString() + ", "+verts[tris[x+2]].ToString() + "\n";
						}
					}

					// Loop through the number of original triangles, but increment in groups of 3.
					for (int i=0; i< triCount; i+=3){
						if( // If this is double-sided, it is likely it will have the same re-arranged triangles in the second half of the mesh
							!AreTwoTrianglesUsingTheSameVertices( 	verts[tris[i]], verts[tris[i+1]], verts[tris[i+2]], 
																	verts[tris[triCount+i]], verts[tris[triCount+i+1]], verts[tris[triCount+i+2]]  ) 
						){
							// Return false if we find a set of triangles that dont match.
							return false;
						}
					}

					// Debug Information
					if( MeshKitGUI.verbose ){
						Debug.Log(debugTriangles);
						Debug.Log(debugVertices);
					}

					// Return true if all the triangles match up	
					return true;
				}
			}

			// Return false if something goes wrong
			return false;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	ARE TWO TRIANGLES USING THE SAME VERTICES?
		//	Compares two sets of three Vector3's to see if a mesh is already double-sided.
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static bool AreTwoTrianglesUsingTheSameVertices( Vector3 a1, Vector3 a2, Vector3 a3, Vector3 b1, Vector3 b2, Vector3 b3 ){
			// Here we check if the vertices in this triangle exist in the second
			if( a1 != b1 && a1 != b2 && a1 != b3 ){ return false; }	// vertex 1 doesnt match any in the 2nd triangle.
			if( a2 != b1 && a2 != b2 && a2 != b3 ){ return false; }	// vertex 1 doesnt match any in the 2nd triangle.
			if( a3 != b1 && a3 != b2 && a3 != b3 ){ return false; }	// vertex 1 doesnt match any in the 2nd triangle.
			// return true if they all matched
			return true;
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		//	MAKE DOUBLE-SIDED MESH
		//	Makes the Mesh visible from both sides
		////////////////////////////////////////////////////////////////////////////////////////////////

		// Double Sided Test
		public static Mesh MakeDoubleSidedMesh( Mesh mesh ){
			return MeshRebuilder.MakeDoubleSidedMesh( mesh, false );
		}

		public static Mesh MakeDoubleSidedMesh( Mesh mesh, bool checkIfDoubleSidedFirst ){

			// Only Invert this Mesh if it doesn't have subMeshes
			if( mesh.subMeshCount == 1 ){

				// If this Mesh is already double-sided, skip it (return the same mesh back)
				if( checkIfDoubleSidedFirst && IsMeshDoubleSided(mesh) ){
					Debug.Log("MESHKIT: A Mesh seems to already be double-sided and was skipped ...");
					return mesh;
				}

				// =============================================================================
				//	SETUP THE CONFIGURATION OF THE MESH
				//	We need to figure out what this mesh has so we can create the mesh properly
				// =============================================================================

				// Figure out if this mesh actually has normals, tangents, etc.
				bool useNormals = false;
				bool useTangents = false;
				bool useColors = false;
				bool useUV = false;
				bool useUV2 = false;
				bool useBoneWeights = false;
			//	bool useBindPoses = false;

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
			//	if(useBindPoses){ newBindposes = new Matrix4x4[numOfVerts*2]; }
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


// ============================================================================================================================================
//	BATCH FUNCTIONS
// ============================================================================================================================================

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	BATCH INVERT MESH
		//	Turns a Mesh Inside-Out
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static void BatchInvertMesh( GameObject go, bool thisGameObjectOnly, bool optionUseMeshFilters, bool optionUseSkinnedMeshRenderers ){

			// Setup Progress
			float maxProgress = 0;
			float progress = 0;

			// Make sure we have created the support folders ...
			if( MeshAssets.HaveSupportFoldersBeenCreated() ){

				// ------------------------
				//	MESH FILTERS
				// ------------------------

				// Find all the MeshFilters in this GameObject
				MeshFilter[] theMeshFilters = new MeshFilter[0];
				if( optionUseMeshFilters ){
					if( thisGameObjectOnly == false ){
						theMeshFilters = go.GetComponentsInChildren<MeshFilter>() as MeshFilter[];
					} else {
						theMeshFilters = go.GetComponents<MeshFilter>() as MeshFilter[];
					}
				}
				
				
				// Make sure these Meshes do not have subMeshes before we begin
				foreach(var mf in theMeshFilters){
					if(	mf.sharedMesh != null && mf.gameObject.GetComponent<Renderer>() != null && mf.gameObject.GetComponent<Renderer>().enabled &&
						mf.sharedMesh.subMeshCount > 1
					){ 
						EditorUtility.DisplayDialog("Mesh Inverter", "One or more of these Mesh Filters were found to have SubMeshes. You should use the 'Seperate' tool before attempting to invert them.\n\nNone of your GameObjects have been changed.", "Okay");
						return; // Break out of the loop.
					}
				}

				// ------------------------
				//	SKINNED MESH RENDERERS
				// ------------------------

				// Find all the MeshFilters in this GameObject
				SkinnedMeshRenderer[] theSkinnedMeshRenderers = new SkinnedMeshRenderer[0];;
				if( optionUseSkinnedMeshRenderers ){
					if( thisGameObjectOnly == false ){
						theSkinnedMeshRenderers = go.GetComponentsInChildren<SkinnedMeshRenderer>() as SkinnedMeshRenderer[];
					} else {
						theSkinnedMeshRenderers = go.GetComponents<SkinnedMeshRenderer>() as SkinnedMeshRenderer[];
					}
				}
				
				
				// Make sure these Meshes do not have subMeshes before we begin
				foreach(var smr in theSkinnedMeshRenderers){
					if(	smr.sharedMesh != null && smr.gameObject.GetComponent<Renderer>() != null && smr.gameObject.GetComponent<Renderer>().enabled &&
						smr.sharedMesh.subMeshCount > 1
					){ 
						EditorUtility.DisplayDialog("Mesh Inverter", "One or more of these Skinned Mesh Renderers were found to have SubMeshes. These types of meshes cannot be inverted.\n\nNone of your GameObjects have been changed.", "Okay");
						return; // Break out of the loop.
					}
				}


				// Setup the Progress Bar
				maxProgress = theMeshFilters.Length + theSkinnedMeshRenderers.Length;

				// ------------------------
				//	MESH FILTERS
				// ------------------------

				// Loop through the meshes
				int id = 0; // Create an id to iterate with
				foreach( MeshFilter m in theMeshFilters){
					if(m.sharedMesh != null && m.gameObject.GetComponent<Renderer>() != null && m.gameObject.GetComponent<Renderer>().enabled ){

						// Show Progress Bar
						if( maxProgress > 0 ){
							EditorUtility.DisplayProgressBar("Inverting Mesh", "Inverting Mesh ( "+ id +" / "+ (theMeshFilters.Length + theSkinnedMeshRenderers.Length).ToString() + ")", progress / maxProgress);
						} else {
							EditorUtility.ClearProgressBar();
						}
						
						// Make an undo state for all MeshFilters
						Undo.RecordObject (m, "MeshKit (Invert Mesh)");

						// ==============================
						// COPY THE SHARED MESH DATA
						// ==============================

						Mesh mesh = new Mesh();
						mesh.Clear();
						mesh.vertices = m.sharedMesh.vertices;
						mesh.triangles = m.sharedMesh.triangles;
						
						mesh.bindposes = m.sharedMesh.bindposes;			// *
						mesh.boneWeights = m.sharedMesh.boneWeights;		// *
						mesh.uv = m.sharedMesh.uv;
						mesh.uv2 = m.sharedMesh.uv2;						// Include UV2 so lightmapping works.
			// These features work on Unity 5 and up ...				
			#if !UNITY_3_5 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_3 && !UNITY_4_4 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7	
						mesh.uv3 = m.sharedMesh.uv3;						// *
						mesh.uv4 = m.sharedMesh.uv4;						// *
			#endif

			#if UNITY_2018_2_OR_NEWER
						mesh.uv5 = m.sharedMesh.uv5;						// *
						mesh.uv6 = m.sharedMesh.uv6;						// *
						mesh.uv7 = m.sharedMesh.uv7;						// *
						mesh.uv8 = m.sharedMesh.uv8;						// *
			#endif				
						mesh.colors32 = m.sharedMesh.colors32;
						mesh.subMeshCount = m.sharedMesh.subMeshCount;		// The new submeshes should only have 1 submesh.
						mesh.normals = m.sharedMesh.normals;
						mesh.tangents = m.sharedMesh.tangents;				// This adds tangents so normal maps work!

						// Invert the Mesh, and then save it as a new file.
						mesh = InvertMesh(mesh);
						Mesh newMesh = MeshAssets.CreateMeshAsset( mesh, MeshAssets.ProcessFileName(MeshAssets.invertedMeshFolder, m.gameObject.name, "Inverted", false) );

						// If the newMesh was crated succesfully ...
						if(newMesh!=null){

							m.sharedMesh = newMesh;

						} else {
							Debug.Log("MESHKIT: Couldn't create inverted Mesh on GameObject: "+ m.gameObject.name);
						}

						// Increment ID ( for filename - so should be incremented here)
						id++;
					}

					// Increment Progress variable
					progress++;
				}

				// ------------------------
				//	SKINNED MESH RENDERERS
				// ------------------------

				// Loop through the meshes
				id = 0; // Re-use an id to iterate with
				foreach( SkinnedMeshRenderer m in theSkinnedMeshRenderers ){
					if(m.sharedMesh != null && m.gameObject.GetComponent<SkinnedMeshRenderer>() != null && m.gameObject.GetComponent<SkinnedMeshRenderer>().enabled ){

						// Show Progress Bar
						if( maxProgress > 0 ){
							EditorUtility.DisplayProgressBar("Inverting Mesh", "Inverting Mesh ( "+id +" / "+ (theMeshFilters.Length + theSkinnedMeshRenderers.Length).ToString() + ")", progress / maxProgress);
						} else {
							EditorUtility.ClearProgressBar();
						}
						
						// Make an undo state for all MeshFilters
						Undo.RecordObject (m, "MeshKit (Invert Mesh)");

						// ==============================
						// COPY THE SHARED MESH DATA
						// ==============================

						Mesh mesh = new Mesh();
						mesh.Clear();
						mesh.vertices = m.sharedMesh.vertices;
						mesh.triangles = m.sharedMesh.triangles;
						
						mesh.bindposes = m.sharedMesh.bindposes;			// *
						mesh.boneWeights = m.sharedMesh.boneWeights;		// *
						mesh.uv = m.sharedMesh.uv;
						mesh.uv2 = m.sharedMesh.uv2;						// Include UV2 so lightmapping works.
			// These features work on Unity 5 and up ...				
			#if !UNITY_3_5 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_3 && !UNITY_4_4 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7	
						mesh.uv3 = m.sharedMesh.uv3;						// *
						mesh.uv4 = m.sharedMesh.uv4;						// *
			#endif

			#if UNITY_2018_2_OR_NEWER
						mesh.uv5 = m.sharedMesh.uv5;						// *
						mesh.uv6 = m.sharedMesh.uv6;						// *
						mesh.uv7 = m.sharedMesh.uv7;						// *
						mesh.uv8 = m.sharedMesh.uv8;						// *
			#endif				
						mesh.colors32 = m.sharedMesh.colors32;
						mesh.subMeshCount = m.sharedMesh.subMeshCount;		// The new submeshes should only have 1 submesh.
						mesh.normals = m.sharedMesh.normals;
						mesh.tangents = m.sharedMesh.tangents;				// This adds tangents so normal maps work!

						// Invert the Mesh, and then save it as a new file.
						mesh = InvertMesh(mesh);
						Mesh newMesh = MeshAssets.CreateMeshAsset( mesh, MeshAssets.ProcessFileName(MeshAssets.invertedMeshFolder, m.gameObject.name, "Inverted", false) );

						// If the newMesh was crated succesfully ...
						if(newMesh!=null){

							m.sharedMesh = newMesh;

						} else {
							Debug.Log("MESHKIT: Couldn't create inverted Mesh on GameObject: "+ m.gameObject.name);
						}

						// Increment ID ( for filename - so should be incremented here)
						id++;
					}

					// Increment Progress variable
					progress++;
				}
			}

			// Stop Progress Bar
			maxProgress = 0;
			progress = 0;
			EditorUtility.ClearProgressBar();
		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	BATCH MAKE DOUBLE SIDED MESH
		//	Allows a Mesh to be viewed from both angles
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static void BatchDoubleSidedMesh( GameObject go, bool thisGameObjectOnly, bool optionUseMeshFilters, bool optionUseSkinnedMeshRenderers ){

			// Setup Progress
			float maxProgress = 0;
			float progress = 0;

			// Make sure we have created the support folders ...
			if( MeshAssets.HaveSupportFoldersBeenCreated() ){

				// ------------------------
				//	MESH FILTERS
				// ------------------------

				// Find all the MeshFilters in this GameObject
				MeshFilter[] theMeshFilters = new MeshFilter[0];
				if( optionUseMeshFilters ){
					if( thisGameObjectOnly == false ){
						theMeshFilters = go.GetComponentsInChildren<MeshFilter>() as MeshFilter[];
					} else {
						theMeshFilters = go.GetComponents<MeshFilter>() as MeshFilter[];
					}
				}
				
				// Make sure these Meshes do not have subMeshes before we begin
				foreach(var mf in theMeshFilters){
					if(	mf.sharedMesh != null && mf.gameObject.GetComponent<Renderer>() != null && mf.gameObject.GetComponent<Renderer>().enabled &&
						mf.sharedMesh.subMeshCount > 1
					){ 
						EditorUtility.DisplayDialog("Double-Sided Mesh", "One or more of these Mesh Filters were found to have SubMeshes. You should use the 'Seperate' tool before attempting to make them double-sided.\n\nNone of your GameObjects have been changed.", "Okay");
						return; // Break out of the loop.
					}
				}

				// ------------------------
				//	SKINNED MESH RENDERERS
				// ------------------------

				// Find all the MeshFilters in this GameObject
				SkinnedMeshRenderer[] theSkinnedMeshRenderers = new SkinnedMeshRenderer[0];
				if( optionUseSkinnedMeshRenderers ){
					if( thisGameObjectOnly == false ){
						theSkinnedMeshRenderers = go.GetComponentsInChildren<SkinnedMeshRenderer>() as SkinnedMeshRenderer[];
					} else {
						theSkinnedMeshRenderers = go.GetComponents<SkinnedMeshRenderer>() as SkinnedMeshRenderer[];
					}
				}
				
				// Make sure these Meshes do not have subMeshes before we begin
				foreach(var smr in theSkinnedMeshRenderers){
					if(	smr.sharedMesh != null && smr.gameObject.GetComponent<SkinnedMeshRenderer>() != null && smr.gameObject.GetComponent<SkinnedMeshRenderer>().enabled &&
						smr.sharedMesh.subMeshCount > 1
					){ 
						EditorUtility.DisplayDialog("Double-Sided Mesh", "One or more of these Skinned Mesh Renderers were found to have SubMeshes. These types of meshes cannot be made double-sided.\n\nNone of your GameObjects have been changed.", "Okay");
						return; // Break out of the loop.
					}
				}

				// Setup the Progress Bar
				maxProgress = theMeshFilters.Length + theSkinnedMeshRenderers.Length;

				// ------------------------
				//	MESH FILTERS
				// ------------------------

				// Loop through the meshes
				int id = 0; // Create an id to iterate with
				foreach( MeshFilter m in theMeshFilters){
					if(m.sharedMesh != null && m.gameObject.GetComponent<Renderer>() != null && m.gameObject.GetComponent<Renderer>().enabled ){

						// Show Progress Bar
						if( maxProgress > 0 ){
							EditorUtility.DisplayProgressBar("Double-Sided Mesh", "Double-Siding Mesh ( "+id +" / "+ (theMeshFilters.Length + theSkinnedMeshRenderers.Length).ToString() + ")", progress / maxProgress);
						} else {
							EditorUtility.ClearProgressBar();
						}
						
						// Make an undo state for all MeshFilters
						Undo.RecordObject (m, "MeshKit (Double-Sided Mesh)");

						// ==============================
						// COPY THE SHARED MESH DATA
						// ==============================

						Mesh mesh = new Mesh();
						mesh.Clear();
						mesh.vertices = m.sharedMesh.vertices;
						mesh.triangles = m.sharedMesh.triangles;
						
						mesh.bindposes = m.sharedMesh.bindposes;			// *
						mesh.boneWeights = m.sharedMesh.boneWeights;		// *
						mesh.uv = m.sharedMesh.uv;
						mesh.uv2 = m.sharedMesh.uv2;						// Include UV2 so lightmapping works.
			// These features work on Unity 5 and up ...				
			#if !UNITY_3_5 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_3 && !UNITY_4_4 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7	
						mesh.uv3 = m.sharedMesh.uv3;						// *
						mesh.uv4 = m.sharedMesh.uv4;						// *
			#endif

			#if UNITY_2018_2_OR_NEWER
						mesh.uv5 = m.sharedMesh.uv5;						// *
						mesh.uv6 = m.sharedMesh.uv6;						// *
						mesh.uv7 = m.sharedMesh.uv7;						// *
						mesh.uv8 = m.sharedMesh.uv8;						// *
			#endif

						mesh.colors32 = m.sharedMesh.colors32;
						mesh.subMeshCount = m.sharedMesh.subMeshCount;		// The new submeshes should only have 1 submesh.
						mesh.normals = m.sharedMesh.normals;
						mesh.tangents = m.sharedMesh.tangents;				// This adds tangents so normal maps work!

						// Make mesh double-sided, and then save it as a new file.
						mesh = MakeDoubleSidedMesh(mesh, true);
						Mesh newMesh = MeshAssets.CreateMeshAsset( mesh, MeshAssets.ProcessFileName(MeshAssets.doubleSidedMeshFolder, m.gameObject.name, "DoubleSided", false) );

						// If the newMesh was crated succesfully ...
						if(newMesh!=null){

							m.sharedMesh = newMesh;

						} else {
							Debug.Log("MESHKIT: Couldn't create double-sided Mesh on GameObject: "+ m.gameObject.name);
						}

						// Increment ID ( for filename - so should be incremented here)
						id++;
					}

					// Increment Progress variable
					progress++;
				}

				// ------------------------
				//	SKINNED MESH RENDERERS
				// ------------------------

				// Loop through the meshes
				id = 0; // Re-use the id to iterate with
				foreach( SkinnedMeshRenderer m in theSkinnedMeshRenderers ){
					if(m.sharedMesh != null && m.gameObject.GetComponent<SkinnedMeshRenderer>() != null && m.gameObject.GetComponent<SkinnedMeshRenderer>().enabled ){

						// Show Progress Bar
						if( maxProgress > 0 ){
							EditorUtility.DisplayProgressBar("Double-Sided Mesh", "Double-Siding Mesh ( "+id +" / "+ (theMeshFilters.Length + theSkinnedMeshRenderers.Length).ToString() + ")", progress / maxProgress);
						} else {
							EditorUtility.ClearProgressBar();
						}
						
						// Make an undo state for all MeshFilters
						Undo.RecordObject (m, "MeshKit (Double-Sided Mesh)");

						// ==============================
						// COPY THE SHARED MESH DATA
						// ==============================

						Mesh mesh = new Mesh();
						mesh.Clear();
						mesh.vertices = m.sharedMesh.vertices;
						mesh.triangles = m.sharedMesh.triangles;
						
						mesh.bindposes = m.sharedMesh.bindposes;			// *
						mesh.boneWeights = m.sharedMesh.boneWeights;		// *
						mesh.uv = m.sharedMesh.uv;
						mesh.uv2 = m.sharedMesh.uv2;						// Include UV2 so lightmapping works.
			// These features work on Unity 5 and up ...				
			#if !UNITY_3_5 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_3 && !UNITY_4_4 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7	
						mesh.uv3 = m.sharedMesh.uv3;						// *
						mesh.uv4 = m.sharedMesh.uv4;						// *
			#endif

			#if UNITY_2018_2_OR_NEWER
						mesh.uv5 = m.sharedMesh.uv5;						// *
						mesh.uv6 = m.sharedMesh.uv6;						// *
						mesh.uv7 = m.sharedMesh.uv7;						// *
						mesh.uv8 = m.sharedMesh.uv8;						// *
			#endif

						mesh.colors32 = m.sharedMesh.colors32;
						mesh.subMeshCount = m.sharedMesh.subMeshCount;		// The new submeshes should only have 1 submesh.
						mesh.normals = m.sharedMesh.normals;
						mesh.tangents = m.sharedMesh.tangents;				// This adds tangents so normal maps work!

						// Make mesh double-sided, and then save it as a new file.
						mesh = MakeDoubleSidedMesh(mesh, true);
						Mesh newMesh = MeshAssets.CreateMeshAsset( mesh, MeshAssets.ProcessFileName(MeshAssets.doubleSidedMeshFolder, m.gameObject.name, "DoubleSided", false) );

						// If the newMesh was crated succesfully ...
						if(newMesh!=null){

							m.sharedMesh = newMesh;

						} else {
							Debug.Log("MESHKIT: Couldn't create double-sided Mesh on GameObject: "+ m.gameObject.name);
						}

						// Increment ID ( for filename - so should be incremented here)
						id++;
					}

					// Increment Progress variable
					progress++;
				}
			}

			// Stop Progress Bar
			maxProgress = 0;
			progress = 0;
			EditorUtility.ClearProgressBar();
		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	BATCH REBUILD MESH
		//	Allows a Mesh to be rebuilt with / without normals, tangents, etc.
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static void BatchRebuildMesh( GameObject go, bool thisGameObjectOnly, bool optionUseMeshFilters, bool optionUseSkinnedMeshRenderers, bool optionStripNormals, bool optionStripTangents, bool optionStripColors, bool optionStripLightmapUVs, bool optionStripUV3, bool optionStripUV4, bool optionStripUV5, bool optionStripUV6, bool optionStripUV7, bool optionStripUV8, bool optionRebuildNormals, bool optionRebuildTangents, bool optionRebuildLightmapUVs, float rebuildNormalsAngle = -1 ){

			// Fix option configuration
			if(optionStripNormals){ optionStripTangents = true; } 		// Always strip tangents if we're stripping normals
			if(optionRebuildTangents ){ optionRebuildNormals = true; } 	// Always rebuild normals if there are none and we are trying to build tangents.

			// Setup Progress
			float maxProgress = 0;
			float progress = 0;

			// Make sure we have created the support folders ...
			if( MeshAssets.HaveSupportFoldersBeenCreated() ){

				// Find all the MeshFilters in this GameObject
				MeshFilter[] theMeshFilters = new MeshFilter[0];
				if( optionUseMeshFilters ){
					if( thisGameObjectOnly == false ){
						theMeshFilters = go.GetComponentsInChildren<MeshFilter>() as MeshFilter[];
					} else {
						theMeshFilters = go.GetComponents<MeshFilter>() as MeshFilter[];
					}
				}

				// Find all the SkinnedMeshRenderers in this GameObject
				SkinnedMeshRenderer[] theSkinnedMeshRenderers = new SkinnedMeshRenderer[0];
				if( optionUseSkinnedMeshRenderers ){
					if( thisGameObjectOnly == false ){
						theSkinnedMeshRenderers = go.GetComponentsInChildren<SkinnedMeshRenderer>() as SkinnedMeshRenderer[];
					} else {
						theSkinnedMeshRenderers = go.GetComponents<SkinnedMeshRenderer>() as SkinnedMeshRenderer[];
					}
				}

				// Setup the Progress Bar
				maxProgress = theMeshFilters.Length + theSkinnedMeshRenderers.Length;
				
				// Make sure these Meshes do not have subMeshes before we begin
				//foreach(var mf in theMeshFilters){
				MeshFilter mf = null;
				for ( int i = 0; i < theMeshFilters.Length; i++ ){
					mf = theMeshFilters[i];
					if(	mf != null && mf.sharedMesh != null && mf.gameObject.GetComponent<Renderer>() != null && mf.gameObject.GetComponent<Renderer>().enabled &&
						mf.sharedMesh.subMeshCount > 1
					){ 
						EditorUtility.DisplayDialog("Rebuild Mesh", "One or more of these Mesh Filters were found to have SubMeshes. You should use the 'Seperate' tool before attempting to rebuid them.\n\nNone of your GameObjects have been changed.", "Okay");
						return; // Break out of the loop.
					}
				}

				// Also Check the Skinned Mesh Renderers for submeshes
				SkinnedMeshRenderer smr = null;
				for ( int i = 0; i < theSkinnedMeshRenderers.Length; i++ ){
					smr = theSkinnedMeshRenderers[i];
					if(	smr != null && smr.sharedMesh != null && smr.gameObject.GetComponent<SkinnedMeshRenderer>() != null && smr.gameObject.GetComponent<SkinnedMeshRenderer>().enabled &&
						smr.sharedMesh.subMeshCount > 1
					){ 
						EditorUtility.DisplayDialog("Rebuild Mesh", "One or more Skinned Mesh Renderers were found to have SubMeshes. These types of meshes cannot be rebuilt.\n\nNone of your GameObjects have been changed.", "Okay");
						return; // Break out of the loop.
					}
				}

				// Loop through the Mesh Filter meshes
				int id = 0; // Create an id to iterate with
				foreach( MeshFilter m in theMeshFilters){
					if(m.sharedMesh != null && m.gameObject.GetComponent<Renderer>() != null && m.gameObject.GetComponent<Renderer>().enabled ){

						// Show Progress Bar
						if( maxProgress > 0 ){
							EditorUtility.DisplayProgressBar("Rebuild Mesh", "Rebuilding Mesh ( "+id +" / "+ theMeshFilters.Length + ")", progress / maxProgress);
						} else {
							EditorUtility.ClearProgressBar();
						}
						
						// Make an undo state for all MeshFilters
						Undo.RecordObject (m, "MeshKit (Rebuild Mesh)");

						// ==============================
						// COPY THE SHARED MESH DATA
						// ==============================

						// Copy the sharedMesh
						Mesh mesh = new Mesh();
						mesh.Clear();
						mesh.vertices = m.sharedMesh.vertices;
						
						// UVs
						mesh.uv = m.sharedMesh.uv;
						mesh.uv2 = m.sharedMesh.uv2;						// Include UV2 so lightmapping works.
			// These features work on Unity 5 and up ...				
			#if !UNITY_3_5 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_3 && !UNITY_4_4 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7	
						mesh.uv3 = m.sharedMesh.uv3;						// *
						mesh.uv4 = m.sharedMesh.uv4;						// *
			#endif	

			// These features work in Unity 2018.2 and up
			#if UNITY_2018_2_OR_NEWER
						mesh.uv5 = m.sharedMesh.uv5;						// *
						mesh.uv6 = m.sharedMesh.uv6;						// *
						mesh.uv7 = m.sharedMesh.uv7;						// *
						mesh.uv8 = m.sharedMesh.uv8;						// *
			#endif		

						// Remove UV2 (Lightmap UVs)
						if( optionStripLightmapUVs ){ mesh.uv2 = new Vector2[0]; }

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
						mesh.triangles = m.sharedMesh.triangles;

						// Colors
						if( optionStripColors ){ 
							mesh.colors32 = new Color32[0];
						} else {
							mesh.colors32 = m.sharedMesh.colors32;
						}

						// Extras
						mesh.subMeshCount = m.sharedMesh.subMeshCount;		// The new submeshes should only have 1 submesh.
						mesh.bindposes = m.sharedMesh.bindposes;			// *
						mesh.boneWeights = m.sharedMesh.boneWeights;		// *

						// Strip or copy original normals and tangents
						if(optionStripNormals){ 
							mesh.normals = new Vector3[0]; 
						} else {
							mesh.normals = m.sharedMesh.normals;
						}

						if(optionStripTangents){ 
							mesh.tangents = new Vector4[0]; 
						} else {
							mesh.tangents = m.sharedMesh.tangents;				// This adds tangents so normal maps work!
						}
						
						// Rebuild Normals
						if(optionRebuildNormals ){ 

							if( rebuildNormalsAngle < 0 ){
								if(MeshKitGUI.verbose){ Debug.Log("MESHKIT: Recalculating Normals ..."); }
								mesh.RecalculateNormals(); 
							} else {
								if(MeshKitGUI.verbose){ Debug.Log("MESHKIT: Recalculating Normals (Based On Angle Threshold)..."); }
								mesh.RecalculateNormalsBasedOnAngleThreshold( rebuildNormalsAngle );
							}
						}

						// Rebuild Tangents
						if(optionRebuildTangents){ 
							if(MeshKitGUI.verbose){ Debug.Log("MESHKIT: Creating Tangents ..."); }
							mesh = CreateTangents(mesh); 
						}

						// Rebuild Lightmap UVs
						if( optionRebuildLightmapUVs ){
							if(MeshKitGUI.verbose){ Debug.Log("MESHKIT: Recreating Lightmap UVs ..."); }
							Unwrapping.GenerateSecondaryUVSet(mesh); 
						}

						// Try to create the asset
						Mesh newMesh = MeshAssets.CreateMeshAsset( mesh, MeshAssets.ProcessFileName(MeshAssets.rebuiltMeshFolder, m.gameObject.name, "Rebuilt", false) );

						// If the newMesh was crated succesfully ...
						if(newMesh!=null){
							
							m.sharedMesh = newMesh;

						} else {
							Debug.Log("MESHKIT: Couldn't rebuild Mesh on GameObject: "+ m.gameObject.name);
						}

						// Increment ID ( for filename - so should be incremented here)
						id++;
					}

					// Increment Progress variable
					progress++;
				}

				// Loop through the Skinned Mesh Renderers
				id = 0; // Reset the id to iterate with
				foreach( SkinnedMeshRenderer m in theSkinnedMeshRenderers ){
					if( m != null && m.sharedMesh != null && m.gameObject.GetComponent<SkinnedMeshRenderer>() != null && m.gameObject.GetComponent<SkinnedMeshRenderer>().enabled ){

						// Show Progress Bar
						if( maxProgress > 0 ){
							EditorUtility.DisplayProgressBar("Rebuild Mesh", "Rebuilding Mesh ( "+id +" / "+ theMeshFilters.Length + ")", progress / maxProgress);
						} else {
							EditorUtility.ClearProgressBar();
						}
						
						// Make an undo state for all MeshFilters
						Undo.RecordObject (m, "MeshKit (Rebuild Mesh)");

						// ==============================
						// COPY THE SHARED MESH DATA
						// ==============================

						// Copy the sharedMesh
						Mesh mesh = new Mesh();
						mesh.Clear();
						mesh.vertices = m.sharedMesh.vertices;
						
						// UVs
						mesh.uv = m.sharedMesh.uv;
						mesh.uv2 = m.sharedMesh.uv2;						// Include UV2 so lightmapping works.
			// These features work on Unity 5 and up ...				
			#if !UNITY_3_5 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_3 && !UNITY_4_4 && !UNITY_4_5 && !UNITY_4_6 && !UNITY_4_7	
						mesh.uv3 = m.sharedMesh.uv3;						// *
						mesh.uv4 = m.sharedMesh.uv4;						// *
			#endif	

			// These features work in Unity 2018.2 and up
			#if UNITY_2018_2_OR_NEWER
						mesh.uv5 = m.sharedMesh.uv5;						// *
						mesh.uv6 = m.sharedMesh.uv6;						// *
						mesh.uv7 = m.sharedMesh.uv7;						// *
						mesh.uv8 = m.sharedMesh.uv8;						// *
			#endif		

						// Remove UV2 (Lightmap UVs)
						if( optionStripLightmapUVs ){ mesh.uv2 = new Vector2[0]; }

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
						mesh.triangles = m.sharedMesh.triangles;

						// Colors
						if( optionStripColors ){ 
							mesh.colors32 = new Color32[0];
						} else {
							mesh.colors32 = m.sharedMesh.colors32;
						}

						// Extras
						mesh.subMeshCount = m.sharedMesh.subMeshCount;		// The new submeshes should only have 1 submesh.
						mesh.bindposes = m.sharedMesh.bindposes;			// *
						mesh.boneWeights = m.sharedMesh.boneWeights;		// *


						// Strip or copy original normals and tangents
						if(optionStripNormals){ 
							mesh.normals = new Vector3[0]; 
						} else {
							mesh.normals = m.sharedMesh.normals;
						}

						if(optionStripTangents){ 
							mesh.tangents = new Vector4[0]; 
						} else {
							mesh.tangents = m.sharedMesh.tangents;				// This adds tangents so normal maps work!
						}
						
						// Rebuild Normals
						if(optionRebuildNormals ){ 

							if( rebuildNormalsAngle < 0 ){
								if(MeshKitGUI.verbose){ Debug.Log("MESHKIT: Recalculating Normals ..."); }
								mesh.RecalculateNormals(); 
							} else {
								if(MeshKitGUI.verbose){ Debug.Log("MESHKIT: Recalculating Normals (Based On Angle Threshold)..."); }
								mesh.RecalculateNormalsBasedOnAngleThreshold( rebuildNormalsAngle );
							}
						}

						// Rebuild Tangents
						if(optionRebuildTangents){ 
							if(MeshKitGUI.verbose){ Debug.Log("MESHKIT: Creating Tangents ..."); }
							mesh = CreateTangents(mesh); 
						}

						// Rebuild Lightmap UVs
						if( optionRebuildLightmapUVs ){
						//	if(MeshKitGUI.verbose){ Debug.Log("MESHKIT: Recreating Lightmap UVs ..."); }
						//	Unwrapping.GenerateSecondaryUVSet(mesh); 
							Debug.LogWarning("MESHKIT: Cannot recreate Lightmap UVs on Skinned Mesh: " + m.gameObject.name );
						}

						// Try to create the asset
						Mesh newMesh = MeshAssets.CreateMeshAsset( mesh, MeshAssets.ProcessFileName(MeshAssets.rebuiltMeshFolder, m.gameObject.name, "Rebuilt", false) );

						// If the newMesh was crated succesfully ...
						if(newMesh!=null){
							
							m.sharedMesh = newMesh;

						} else {
							Debug.Log("MESHKIT: Couldn't rebuild Mesh on GameObject: "+ m.gameObject.name);
						}

						// Increment ID ( for filename - so should be incremented here)
						id++;
					}

					// Increment Progress variable
					progress++;
				}
			}

			// Stop Progress Bar
			maxProgress = 0;
			progress = 0;
			EditorUtility.ClearProgressBar();
		}
	}
}
