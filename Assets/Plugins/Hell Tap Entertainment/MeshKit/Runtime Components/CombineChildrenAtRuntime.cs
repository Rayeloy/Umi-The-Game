////////////////////////////////////////////////////////////////////////////////////////////////
//
//  CombineChildrenAtRuntime.cs
//
//	Combines Meshes of all children objects at runtime
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
	[AddComponentMenu("MeshKit/Combine Children At Runtime")]
	public class CombineChildrenAtRuntime : MonoBehaviour {

		// Add Seperation Option
		[Header("SubMesh Options   (this can take a while in large scenes)")]
			[Tooltip("This GameObject and it's children will be scanned for submeshes. If found, they will be broken apart and rebuilt before the combine process begins.\n\nNOTE: This should generally not be used in runtime builds as it is a very expensive operation which can take several minutes or more to complete!")]
			public bool seperateSubMeshesFirst = false;					// Seperates All SubMeshes into individual objects first.
			[Tooltip("After seperating meshes, MeshKit strips unused vertices making each mesh highly optimized and memory efficient. Unfortunatly this can heavily increase processing time, especially with large meshes.")]
			public bool stripUnusedVertices = false;					// Strips unused vertices - optimized but can take a lot longer.
			[Tooltip("Only Seperates SubMeshes which have Renderers that are enabled.")]
			public bool onlySeperateEnabledRenderers = true;			// Only Seperates SubMeshes which have Renderers that are enabled.

		// Combine Options
		[Header("Combine Options")]

			[Tooltip("Only GameObjects with their Renderer component enabled will be combined.")]
			[Range(16000, 65534)]
			public int maximumVerticesPerObject = 65534;				// The maximum number of vertices

			[Tooltip("Only GameObjects with their Renderer component enabled will be combined.")]
			public bool onlyCombineEnabledRenderers = true;				// Only combine enabled Renderers

			[Tooltip("Apply Unity's mesh optimization function to the combined Meshes.\n\nNOTE: This increases the time it takes to combine objects.")]
			public bool optimizeCombinedMeshes = false;					// Optimize Meshes during the combine stage.

			[Tooltip("Adds Mesh Colliders to the new combined objects. It's usually a good idea to check \"Delete Objects With Disabled Renderers\" when selecting this option.")]
			public bool createMeshCollidersOnNewObjects = false;					// Adds Mesh Colliders on combined objects

			[Tooltip("Use -1 to create new GameObjects using the Default layer. Alternatively, enter a layer index to use ( 0 - 31 ).")]
			public int createNewObjectsWithLayer = -1;					// Sets the newly created GameObjects to use this Layer [-1 ignores this]

			[Tooltip("Leave this blank to create untagged GameObjects or enter the name of the tag to set.")]
			public string createNewObjectsWithTag = "Untagged";			// Sets the newly created GameObjects to use this tag

		
		[Header("Cleanup Options")]
		//[Space(8)]
		
			[Tooltip("Destroys all GameObjects originally used to create the combined mesh (with the exception of those that have Colliders attached to them).")]
			public bool destroyOriginalObjects = true;					// Deletes the original objects and cleans up empty GameObjects

			[Tooltip("Destroys all GameObjects that are in this group with disabled Renderer components (This includes objects with active Colliders).")]
			public bool destroyObjectsWithDisabledRenderers = false;		// Keeps All Source Objects without Colliders

			[Tooltip("Destroys any empty GameObjects which do not have any components or children. ")]
			public bool destroyEmptyObjects = true;						// Scans the GameObjects in this group and deletes any empty GameObjects


		////////////////////////////////////////////////////////////////////////////////////////////////
		//	START
		//	We create the new Meshes at start
		////////////////////////////////////////////////////////////////////////////////////////////////

		void Start(){

			// Seperate The SubMeshes first
			if( seperateSubMeshesFirst ){
				#if UNITY_EDITOR
					Debug.LogWarning("MESHKIT: Seperating SubMeshes dynamically at runtime is a very expensive operation which can make your game slow down for several minutes or more depending on the complexity of the scene. Consider building seperated SubMeshes with the MeshKit Editor to avoid this processing time.", gameObject );
				#endif
				MeshKit.SeparateMeshes( gameObject, onlySeperateEnabledRenderers, stripUnusedVertices );
			}

			MeshKit.CombineChildren( gameObject, optimizeCombinedMeshes, createNewObjectsWithLayer, createNewObjectsWithTag, onlyCombineEnabledRenderers, createMeshCollidersOnNewObjects, destroyOriginalObjects, destroyObjectsWithDisabledRenderers, destroyEmptyObjects, maximumVerticesPerObject );
		}

	}
}
