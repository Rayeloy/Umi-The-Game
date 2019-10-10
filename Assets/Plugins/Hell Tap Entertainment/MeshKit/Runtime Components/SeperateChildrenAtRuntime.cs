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
	[AddComponentMenu("MeshKit/SeperateChildrenAtRuntime")]
	public class SeperateChildrenAtRuntime : MonoBehaviour {

		// Options
		[Tooltip("After seperating meshes, MeshKit strips unused vertices making each mesh highly optimized and memory efficient. Unfortunatly this can heavily increase processing time, especially with large meshes.")]
			public bool stripUnusedVertices = false;					// Strips unused vertices - optimized but can take a lot longer.
		[Tooltip("Only apply to GameObjects with active Renderer components.")]
		public bool onlyApplyToEnabledRenderers = true; 	// Only invert meshes with renderers that are enabled

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	START
		////////////////////////////////////////////////////////////////////////////////////////////////

		void Start(){ 
			
			MeshKit.SeparateMeshes( gameObject, onlyApplyToEnabledRenderers, stripUnusedVertices );
			Destroy(this); // Remove this component when finished.
		}

	}
}
