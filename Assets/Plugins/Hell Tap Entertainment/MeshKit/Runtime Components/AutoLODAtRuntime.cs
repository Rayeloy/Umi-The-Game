////////////////////////////////////////////////////////////////////////////////////////////////
//
//  AutoLODAtRuntime.cs
//
//	Dynamically decimates meshes and creates an LODGroup at runtime.
//
//	© 2018 Melli Georgiou.
//	Hell Tap Entertainment LTD
//
////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using HellTap.MeshKit;

// Use HellTap Namespace
namespace HellTap.MeshKit {

	// Class
	[ExecuteInEditMode]				// <- this runtime component also runs in edit mode to clamp lod distances
	[DisallowMultipleComponent]
	[AddComponentMenu("MeshKit/AutoLOD At Runtime")]
	public class AutoLODAtRuntime : MonoBehaviour {

		// LOD Settings
		[Header("LOD Settings")]
		[Tooltip("These options will be used to setup the LOD Group. The group at the top of the list will be closest to the camera.")]
		public MeshKit.AutoLODSettings[] levels = new MeshKit.AutoLODSettings[] {
			 new MeshKit.AutoLODSettings( 50f, 0.8f ),
			 new MeshKit.AutoLODSettings( 16f, 0.65f, SkinQuality.Bone2, true, ShadowCastingMode.Off, MotionVectorGenerationMode.Object, false),
			 new MeshKit.AutoLODSettings( 7f, 0.4f, SkinQuality.Bone1, false, ShadowCastingMode.Off, MotionVectorGenerationMode.Object, false)
		};

		[Header("LOD Culling Distance")]
		[Tooltip("At what distance should this LOD Group be hidden?")]
		[Range(0f, 100f)]
		public float cullingDistancePercentage = 1;

		// Decimation Options
		[Header("Decimation Options")]
		[Tooltip("If there are gaps showing up in the mesh, you can try to stop the decimator from removing borders. This will affect the decimator's ability to reduce complexity.")]
		public bool preserveBorders = false;
		[Tooltip("If there are gaps showing up in the mesh, you can try to stop the decimator from removing seams. This will affect the decimator's ability to reduce complexity.")]
		public bool preserveSeams = false;
		[Tooltip("If there are gaps showing up in the mesh, you can try to stop the decimator from removing UV foldovers. This will affect the decimator's ability to reduce complexity.")]
		public bool preserveFoldovers = false;

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	START
		////////////////////////////////////////////////////////////////////////////////////////////////

		void Start(){ 

			// Stops the code running in edit mode
			if( Application.isPlaying == true ){

				// Do Auto LOD
				MeshKit.AutoLOD( gameObject, levels, cullingDistancePercentage, preserveBorders, preserveSeams, preserveFoldovers );
				
				// Remove this component when finished.
				Destroy(this); 

			} 
		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	UPDATE (EDIT MODE ONLY)
		//	Helps the user by keeping the LOD Distance percentages setup correctly
		////////////////////////////////////////////////////////////////////////////////////////////////

		#if UNITY_EDITOR
			void Update(){

				// This code only runs in the Editor during Edit mode
				if( Application.isPlaying == false ){
				
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
						cullingDistancePercentage >= levels[levels.Length-1].lodDistancePercentage 
					){
						cullingDistancePercentage = levels[levels.Length-1].lodDistancePercentage - 0.01f;
						if( cullingDistancePercentage < 0f ){ cullingDistancePercentage = 0f; }
					}
				
				}
			}
		#endif 

	}
}
