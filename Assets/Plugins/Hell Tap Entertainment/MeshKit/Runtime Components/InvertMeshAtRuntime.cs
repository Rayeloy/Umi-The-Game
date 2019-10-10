////////////////////////////////////////////////////////////////////////////////////////////////
//
//  InvertMeshAtRuntime.cs
//
//	Inverts Meshes of all child objects at runtime
//
//	© 2015 Melli Georgiou.
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
	[AddComponentMenu("MeshKit/Invert Children At Runtime")]
	public class InvertMeshAtRuntime : MonoBehaviour {

		// Options
		[Tooltip("Attempt to make all child objects containing MeshFilters Inverted too.")]
		public bool applyToChildren = true;					// Apply to all child GameObjects
		[Tooltip("Invert the meshes within MeshFilters")]
		public bool applyToMeshFilters = true;				// Apply to MeshFilter components
		[Tooltip("Invert the meshes within Skinned Mesh Renderers")]
		public bool applyToSkinnedMeshRenderers = false;	// Apply to Skinned Mesh Renderers
		[Tooltip("Only apply to GameObjects with active Renderer components.")]
		public bool onlyApplyToEnabledRenderers = true; 	// Only invert meshes with renderers that are enabled

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	START
		////////////////////////////////////////////////////////////////////////////////////////////////

		void Start(){ 

			MeshKit.InvertMesh( gameObject, applyToChildren, applyToMeshFilters, applyToSkinnedMeshRenderers, onlyApplyToEnabledRenderers );
			Destroy(this); // Remove this component when finished.
		}

	}
}
