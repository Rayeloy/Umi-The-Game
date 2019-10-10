////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MakeDoubleSidedAtRuntime.cs
//
//	Makes Double-Sided Meshes of all child objects at runtime
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
	[AddComponentMenu("MeshKit/Double-Sided Children At Runtime")]
	public class MakeDoubleSidedAtRuntime : MonoBehaviour {

		// Options
		[Tooltip("Attempt to make all child objects containing MeshFilters Double-Sided too.")]
		public bool applyToChildren = true;					// Apply to all child GameObjects
		[Tooltip("Make the meshes within MeshFilters Double-Sided")]
		public bool applyToMeshFilters = true;				// Apply to MeshFilter components
		[Tooltip("Make the meshes within Skinned Mesh Renderers Double-Sided")]
		public bool applyToSkinnedMeshRenderers = false;	// Apply to Skinned Mesh Renderers
		[Tooltip("Only apply to GameObjects with active Renderer components.")]
		public bool onlyApplyToEnabledRenderers = true; 	// Only invert meshes with renderers that are enabled

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	START
		////////////////////////////////////////////////////////////////////////////////////////////////

		void Start(){ 

			MeshKit.MakeDoubleSided(gameObject, applyToChildren, applyToMeshFilters, applyToSkinnedMeshRenderers, onlyApplyToEnabledRenderers );
			Destroy(this); // Remove this component when finished.
		}

	}
}
