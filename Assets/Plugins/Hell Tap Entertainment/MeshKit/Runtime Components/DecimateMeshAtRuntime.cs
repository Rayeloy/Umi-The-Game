////////////////////////////////////////////////////////////////////////////////////////////////
//
//  DecimateMeshAtRuntime.cs
//
//	Decimates a mesh on a MeshFilter or SkinnedMeshRenderer at runtime.
//
//	© 2018 Melli Georgiou.
//	Hell Tap Entertainment LTD
//
////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HellTap.MeshKit;

// Use HellTap Namespace
namespace HellTap.MeshKit {

	// Class
	[DisallowMultipleComponent]
	[AddComponentMenu("MeshKit/Decimate Mesh At Runtime")]
	public class DecimateMeshAtRuntime : MonoBehaviour {

		// Options
		[Header("Selection")]
		[Tooltip("Attempt to decimate all child objects too.")]
		public bool applyToChildren = true;					// Apply to all child GameObjects
		[Tooltip("Decimate the meshes within MeshFilters")]
		public bool applyToMeshFilters = true;				// Apply to MeshFilter components
		[Tooltip("Decimate the meshes within Skinned Mesh Renderers")]
		public bool applyToSkinnedMeshRenderers = true;		// Apply to Skinned Mesh Renderers
		[Tooltip("Only apply to GameObjects with active Renderer components.")]
		public bool onlyApplyToEnabledRenderers = true; 	// Only invert meshes with renderers that are enabled

		[Header("Decimator")]
		[Tooltip("Set the quality of the decimation. 0 = No details, 1 = Full details.")]
		[Range(0f,1f)]
		public float decimatorQuality = 0.8f; 				// Decimation reduction
		[Tooltip("Recalculate the mesh's normals after it has been decimated.")]
		public bool recalculateNormals = false; 			// Recalculate the mesh's normals after it has been decimated
		
		[Header("Try these options if gaps appear in the Mesh")]
		[Tooltip("Preserve border vertices.")]
		public bool preserveBorders = false; 				// Preserve border vertices
		[Tooltip("Preserve seams on the Mesh.")]
		public bool preserveSeams = false; 					// Preserve seams on the Mesh
		[Tooltip("Preserve UV foldovers.")]
		public bool preserveFoldovers = false; 				// Preserve UV foldovers


		////////////////////////////////////////////////////////////////////////////////////////////////
		//	START
		////////////////////////////////////////////////////////////////////////////////////////////////

		void Start(){ 

			MeshKit.DecimateMesh(gameObject, applyToChildren, applyToMeshFilters, applyToSkinnedMeshRenderers, onlyApplyToEnabledRenderers, decimatorQuality, recalculateNormals, preserveBorders, preserveSeams, preserveFoldovers );
			Destroy(this); // Remove this component when finished.
		}

	}
}
