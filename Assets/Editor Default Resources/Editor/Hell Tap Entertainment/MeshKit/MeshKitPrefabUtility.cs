using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Use HellTap Namespace
namespace HellTap.MeshKit {

	public static class MeshKitPrefabUtility {

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	GET PREFAB OBJECT
		//	Retrieves the enclosing Prefab for any object contained within.
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static Object GetPrefabObject( Object targetObject ){

			// New Method
			#if UNITY_2018_4_OR_NEWER

				// Returns the PrefabInstance object for the outermost Prefab instance the provided object is part of.
				return PrefabUtility.GetPrefabInstanceHandle( targetObject );

			// Original Method
			#else

				return PrefabUtility.GetPrefabObject( targetObject );

			#endif
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		//	FIND ROOT GAMEOBJECT WITH SAME PARENT PREFAB
		//	Returns the topmost GameObject that has the same Prefab parent as target.
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static GameObject FindRootGameObjectWithSameParentPrefab( GameObject target ){

			// New Method
			#if UNITY_2018_4_OR_NEWER

				// Returns the GameObject that is the root of the outermost Prefab instance the object is part of.
				return PrefabUtility.GetOutermostPrefabInstanceRoot( target );

			// Original Method
			#else

				return PrefabUtility.FindRootGameObjectWithSameParentPrefab( target );

			#endif
		}

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	FIND VALID UPLOAD PREFAB INSTANCE ROOT
		//	Returns the root GameObject of the Prefab instance if that root Prefab instance is a parent of the Prefab.
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static GameObject FindValidUploadPrefabInstanceRoot( GameObject target ){

			// New Method
			#if UNITY_2018_4_OR_NEWER

				// Returns the GameObject that is the root of the outermost Prefab instance the object is part of.
				return PrefabUtility.GetOutermostPrefabInstanceRoot( target );

			// Original Method
			#else

				return PrefabUtility.FindValidUploadPrefabInstanceRoot( target );

			#endif
		}

	}

}




