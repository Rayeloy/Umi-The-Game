//
// 	UPDATED FOR MESHKIT v2.
//	NOTES:
//			- Much of this code has been modified to include extra features and better implementation into MeshKit.
//			- Undo should now be working for the core methods.
//			- This *should* still retain the ability to work during runtime.
//

#region License
/*
MIT License

Copyright(c) 2017-2018 Mattias Edlund

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion

using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;
using HellTap.MeshDecimator.Algorithms;

namespace HellTap.MeshDecimator.Unity
{
	using Mesh = UnityEngine.Mesh;

	#region Delegates
	/// <summary>
	/// A callback for LOD status reports.
	/// </summary>
	/// <param name="lodLevel">The LOD level.</param>
	/// <param name="iteration">The current iteration for this LOD level, starting at zero.</param>
	/// <param name="originalTris">The original count of triangles.</param>
	/// <param name="currentTris">The current count of triangles for this LOD level.</param>
	/// <param name="targetTris">The target count of triangles for this LOD level.</param>
	public delegate void LODStatusReportCallback(int lodLevel, int iteration, int originalTris, int currentTris, int targetTris);
	#endregion

	#region LOD Settings
	/// <summary>
	/// LOD Level Settings.
	/// </summary>
	[System.Serializable]
	public struct LODSettings
	{
		#region Fields
		
		[Header("LOD Distance")]
		[Range(0.01f, 100f)]
		/// <summary>
		/// The LOD Percentage. 1 = 100.
		/// </summary>
		[Tooltip("At what distance should this LOD be shown? 100 is used for the best quality mesh.")]
		public float lodDistancePercentage;	// <- Included For MeshKit.
		
		/// <summary>
		/// The LOD level quality between 0 and 1.
		/// </summary>
		[Header("Decimation")]
		[Range(0.01f, 1f)]
		[Tooltip("When decimating, a value of 0 will reduce mesh complexity as much as possible. 1 will preserve it.")]
		public float quality;

		/// <summary>
		/// If the meshes should be combined into one.
		/// </summary>
		[HideInInspector]
		[Tooltip("Combining Meshes should always be false in MeshKit.")]
		public bool combineMeshes;	// <- this is hidden and should always be false
		
		[Header("Renderers")]
		/// <summary>
		/// The LOD level skin quality.
		/// </summary>
		[Tooltip("The Skin Quality setting used in the Renderer.")]
		public SkinQuality skinQuality;
		/// <summary>
		/// If the LOD level receives shadows.
		/// </summary>
		[Tooltip("The Recieve Shadows setting used in the Renderer.")]
		public bool receiveShadows;
		/// <summary>
		/// The LOD level shadow casting mode.
		/// </summary>
		[Tooltip("The Shadow Casting setting used in the Renderer.")]
		public ShadowCastingMode shadowCasting;
		/// <summary>
		/// The LOD level motion vectors generation mode.
		/// </summary>
		[Tooltip("The Motion Vectors setting used in the Renderer.")]
		public MotionVectorGenerationMode motionVectors;
		/// <summary>
		/// If the LOD level uses skinned motion vectors.
		/// </summary>
		[Tooltip("The Skinned Motion Vectors setting used in the Renderer.")]
		public bool skinnedMotionVectors;

		/// <summary>
		/// The LOD level light probe usage.
		/// </summary>
		[Tooltip("The Light Probe Usage setting found in the Renderer.")]
		public LightProbeUsage lightProbeUsage;
		/// <summary>
		/// The LOD level reflection probe usage.
		/// </summary>
		[Tooltip("The Reflection Probe Usage setting found in the Renderer.")]
		public ReflectionProbeUsage reflectionProbeUsage;
		#endregion

		#region Constructors
		/// <summary>
		/// Creates new LOD Level Settings.
		/// </summary>
		/// <param name="quality">The LOD level quality.</param>
		public LODSettings( float quality, float lodDistancePercentage = 0.8f )
		{
			this.quality = quality;
			this.lodDistancePercentage = lodDistancePercentage;
			this.combineMeshes = false;
			this.skinQuality = SkinQuality.Auto;
			this.receiveShadows = true;
			this.shadowCasting = ShadowCastingMode.On;
			this.motionVectors = MotionVectorGenerationMode.Object;
			this.skinnedMotionVectors = true;

			this.lightProbeUsage = LightProbeUsage.BlendProbes;
			this.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
		}

		/// <summary>
		/// Creates new LOD Level Settings.
		/// </summary>
		/// <param name="quality">The quality.</param>
		/// <param name="skinQuality">The skin quality.</param>
		public LODSettings(float quality, float lodDistancePercentage, SkinQuality skinQuality)
		{
			this.quality = quality;
			this.lodDistancePercentage = lodDistancePercentage;
			this.combineMeshes = false;
			this.skinQuality = skinQuality;
			this.receiveShadows = true;
			this.shadowCasting = ShadowCastingMode.On;
			this.motionVectors = MotionVectorGenerationMode.Object;
			this.skinnedMotionVectors = true;

			this.lightProbeUsage = LightProbeUsage.BlendProbes;
			this.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
		}

		/// <summary>
		/// Creates new LOD Level Settings.
		/// </summary>
		/// <param name="quality">The quality.</param>
		/// <param name="skinQuality">The skin quality.</param>
		/// <param name="receiveShadows">If receiving shadows.</param>
		/// <param name="shadowCasting">The shadow casting mode.</param>
		public LODSettings(float quality, float lodDistancePercentage, SkinQuality skinQuality, bool receiveShadows, ShadowCastingMode shadowCasting)
		{
			this.quality = quality;
			this.lodDistancePercentage = lodDistancePercentage;
			this.combineMeshes = false;
			this.skinQuality = skinQuality;
			this.receiveShadows = receiveShadows;
			this.shadowCasting = shadowCasting;
			this.motionVectors = MotionVectorGenerationMode.Object;
			this.skinnedMotionVectors = true;

			this.lightProbeUsage = LightProbeUsage.BlendProbes;
			this.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
		}

		/// <summary>
		/// Creates new LOD Level Settings.
		/// </summary>
		/// <param name="quality">The quality.</param>
		/// <param name="lodDistancePercentage">The percentage where the LOD changes 1 = 100.</param>
		/// <param name="skinQuality">The skin quality.</param>
		/// <param name="receiveShadows">If receiving shadows.</param>
		/// <param name="shadowCasting">The shadow casting mode.</param>
		/// <param name="motionVectors">The motion vector generation mode.</param>
		/// <param name="skinnedMotionVectors">If motion vectors are skinned.</param>
		public LODSettings(float quality, float lodDistancePercentage, SkinQuality skinQuality, bool receiveShadows, ShadowCastingMode shadowCasting, MotionVectorGenerationMode motionVectors, bool skinnedMotionVectors)
		{
			this.quality = quality;
			this.lodDistancePercentage = lodDistancePercentage;
			this.combineMeshes = false;
			this.skinQuality = skinQuality;
			this.receiveShadows = receiveShadows;
			this.shadowCasting = shadowCasting;
			this.motionVectors = motionVectors;
			this.skinnedMotionVectors = skinnedMotionVectors;

			this.lightProbeUsage = LightProbeUsage.BlendProbes;
			this.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
		}

		/// <summary>
		/// Creates new LOD Level Settings.
		/// </summary>
		/// <param name="quality">The quality.</param>
		/// <param name="lodDistancePercentage">The percentage where the LOD changes 1 = 100.</param>
		/// <param name="skinQuality">The skin quality.</param>
		/// <param name="receiveShadows">If receiving shadows.</param>
		/// <param name="shadowCasting">The shadow casting mode.</param>
		/// <param name="motionVectors">The motion vector generation mode.</param>
		/// <param name="skinnedMotionVectors">If motion vectors are skinned.</param>
		/// <param name="lightProbeUsage">The light Probe usage in the renderer.</param>
		/// <param name="reflectionProbeUsage">The reflection Probe usage in the renderer.</param>
		public LODSettings(float quality, float lodDistancePercentage, SkinQuality skinQuality, bool receiveShadows, ShadowCastingMode shadowCasting, MotionVectorGenerationMode motionVectors, bool skinnedMotionVectors, LightProbeUsage lightProbeUsage, ReflectionProbeUsage reflectionProbeUsage )
		{
			this.quality = quality;
			this.lodDistancePercentage = lodDistancePercentage;
			this.combineMeshes = false;
			this.skinQuality = skinQuality;
			this.receiveShadows = receiveShadows;
			this.shadowCasting = shadowCasting;
			this.motionVectors = motionVectors;
			this.skinnedMotionVectors = skinnedMotionVectors;
			this.lightProbeUsage = lightProbeUsage;
			this.reflectionProbeUsage = reflectionProbeUsage;
		}
		#endregion
	}
	#endregion

	/// <summary>
	/// A LOD generator API.
	/// </summary>
	public static class LODGenerator
	{
		#region Consts
		private const string ParentGameObjectName = "_LOD_";
		#endregion

		#region Private Methods
		#region Combine Renderers
		private static Renderer[] CombineRenderers(MeshRenderer[] meshRenderers, SkinnedMeshRenderer[] skinnedRenderers)
		{
			int totalRendererCount = meshRenderers.Length + skinnedRenderers.Length;
			var newRenderers = new Renderer[totalRendererCount];
			System.Array.Copy(meshRenderers, 0, newRenderers, 0, meshRenderers.Length);
			System.Array.Copy(skinnedRenderers, 0, newRenderers, meshRenderers.Length, skinnedRenderers.Length);
			return newRenderers;
		}
		#endregion

		#region Generate Static LOD
		private static Mesh GenerateStaticLOD(Transform transform, MeshRenderer renderer, float quality, out Material[] materials, DecimationAlgorithm.StatusReportCallback statusCallback, bool preserveBorders = false, bool preserveSeams = false, bool preserveFoldovers = false )
		{
			var rendererTransform = renderer.transform;
			var meshFilter = renderer.GetComponent<MeshFilter>();
			var mesh = meshFilter.sharedMesh;
			var meshTransform = transform.worldToLocalMatrix * rendererTransform.localToWorldMatrix;
			materials = renderer.sharedMaterials;
			return MeshDecimatorUtility.DecimateMesh(mesh, meshTransform, quality, false, statusCallback, preserveBorders, preserveSeams, preserveFoldovers );
		}

		private static Mesh GenerateStaticLOD(Transform transform, MeshRenderer[] renderers, float quality, out Material[] materials, DecimationAlgorithm.StatusReportCallback statusCallback, bool preserveBorders = false, bool preserveSeams = false, bool preserveFoldovers = false)
		{
			if (renderers.Length == 1)
			{
				return GenerateStaticLOD(transform, renderers[0], quality, out materials, statusCallback, preserveBorders, preserveSeams, preserveFoldovers );
			}
			else
			{
				var sourceMeshes = new Mesh[renderers.Length];
				var transforms = new Matrix4x4[renderers.Length];
				var meshMaterials = new Material[renderers.Length][];
				for (int i = 0; i < renderers.Length; i++)
				{
					var renderer = renderers[i];
					var rendererTransform = renderer.transform;
					var meshFilter = renderer.GetComponent<MeshFilter>();
					sourceMeshes[i] = meshFilter.sharedMesh;
					transforms[i] = transform.worldToLocalMatrix * rendererTransform.localToWorldMatrix;
					meshMaterials[i] = renderer.sharedMaterials;
				}
				return MeshDecimatorUtility.DecimateMeshes(sourceMeshes, transforms, meshMaterials, quality, false, out materials, statusCallback, preserveBorders, preserveSeams, preserveFoldovers );
			}
		}
		#endregion

		#region Generate Skinned LOD
		private static Mesh GenerateSkinnedLOD(Transform transform, SkinnedMeshRenderer renderer, float quality, out Material[] materials, out Transform[] mergedBones, DecimationAlgorithm.StatusReportCallback statusCallback, bool preserveBorders = false, bool preserveSeams = false, bool preserveFoldovers = false )
		{
			var rendererTransform = renderer.transform;
			var mesh = renderer.sharedMesh;
			var meshTransform = transform.worldToLocalMatrix * rendererTransform.localToWorldMatrix;
			materials = renderer.sharedMaterials;
			mergedBones = renderer.bones;
			return MeshDecimatorUtility.DecimateMesh(mesh, meshTransform, quality, false, statusCallback, preserveBorders, preserveSeams, preserveFoldovers );
		}

		private static Mesh GenerateSkinnedLOD(Transform transform, SkinnedMeshRenderer[] renderers, float quality, out Material[] materials, out Transform[] mergedBones, DecimationAlgorithm.StatusReportCallback statusCallback, bool preserveBorders = false, bool preserveSeams = false, bool preserveFoldovers = false )
		{
			if (renderers.Length == 1)
			{
				return GenerateSkinnedLOD(transform, renderers[0], quality, out materials, out mergedBones, statusCallback, preserveBorders, preserveSeams, preserveFoldovers );
			}
			else
			{
				var sourceMeshes = new Mesh[renderers.Length];
				var transforms = new Matrix4x4[renderers.Length];
				var meshMaterials = new Material[renderers.Length][];
				var meshBones = new Transform[renderers.Length][];
				for (int i = 0; i < renderers.Length; i++)
				{
					var renderer = renderers[i];
					var rendererTransform = renderer.transform;
					sourceMeshes[i] = renderer.sharedMesh;
					transforms[i] = transform.worldToLocalMatrix * rendererTransform.localToWorldMatrix;
					meshMaterials[i] = renderer.sharedMaterials;
					meshBones[i] = renderer.bones;
				}
				return MeshDecimatorUtility.DecimateMeshes(sourceMeshes, transforms, meshMaterials, meshBones, quality, false, out materials, out mergedBones, statusCallback, preserveBorders, preserveSeams, preserveFoldovers );
			}
		}
		#endregion

		#region Find Root Bone
		private static Transform FindRootBone(Transform transform, Transform[] transforms)
		{
			Transform result = null;
			for (int i = 0; i < transforms.Length; i++)
			{
				if (transforms[i].parent == transform)
				{
					result = transforms[i];

					// If we found a transform directly under our root with more than one child, we assume it to be root
					if (transforms[i].childCount > 0)
						break;
				}
			}
			return result;
		}
		#endregion

		#region Setup LOD Renderer
		private static void SetupLODRenderer(Renderer renderer, LODSettings settings)
		{
			renderer.shadowCastingMode = settings.shadowCasting;
			renderer.receiveShadows = settings.receiveShadows;
			renderer.motionVectorGenerationMode = settings.motionVectors;

			renderer.lightProbeUsage = settings.lightProbeUsage;
			renderer.reflectionProbeUsage = settings.reflectionProbeUsage;

			SkinnedMeshRenderer skinnedRenderer = renderer as SkinnedMeshRenderer;
			if (skinnedRenderer != null)
			{
				skinnedRenderer.skinnedMotionVectors = settings.skinnedMotionVectors;
				skinnedRenderer.quality = settings.skinQuality;
			}
		}
		#endregion
		#endregion

		#region Public Methods
		#region Generate LODs
		/// <summary>
		/// Generates the LODs and sets up a LOD Group for the specified game object.
		/// </summary>
		/// <param name="gameObj">The game object to set up.</param>
		/// <param name="levels">The LOD levels.</param>
		/// <param name="statusCallback">The optional status report callback.</param>
		public static void GenerateLODs(GameObject gameObj, LODSettings[] levels, LODStatusReportCallback statusCallback = null, bool preserveBorders = false, bool preserveSeams = false, bool preserveFoldovers = false )
		{
			DestroyLODs(gameObj);

			var transform = gameObj.transform;
			var meshRenderers = gameObj.GetComponentsInChildren<MeshRenderer>();
			var skinnedRenderers = gameObj.GetComponentsInChildren<SkinnedMeshRenderer>();

			// Check if there's anything to do
			if (meshRenderers.Length == 0 && skinnedRenderers.Length == 0)
				return;

			var lodsParentObj = new GameObject(ParentGameObjectName);
			#if UNITY_EDITOR
				if( Application.isPlaying == false ){
					UnityEditor.Undo.RegisterCreatedObjectUndo (lodsParentObj, "Generate LODs");
				}
			#endif

			var lodsParent = lodsParentObj.transform;
			lodsParent.parent = transform;
			lodsParent.localPosition = Vector3.zero;
			lodsParent.localRotation = Quaternion.identity;
			lodsParent.localScale = Vector3.one;

			var lodGroup = gameObj.GetComponent<LODGroup>();
			if (lodGroup == null)
			{
				//lodGroup = gameObj.AddComponent<LODGroup>();
				
				#if UNITY_EDITOR
					if( Application.isPlaying == false ){
						UnityEditor.Undo.AddComponent<LODGroup>(gameObj);
						lodGroup = gameObj.GetComponent<LODGroup>();
					} else {
						lodGroup = gameObj.AddComponent<LODGroup>();
					}
				#else
					lodGroup = gameObj.AddComponent<LODGroup>();
				#endif
			}

			float screenRelativeTransitionHeight = 0.5f;
			LOD[] lodLevels = new LOD[levels.Length + 1];
			var firstLevelRenderers = CombineRenderers(meshRenderers, skinnedRenderers);
			lodLevels[0] = new LOD(screenRelativeTransitionHeight, firstLevelRenderers);
			screenRelativeTransitionHeight *= 0.5f;

			float minimumQuality = 1f;
			for (int levelIndex = 0; levelIndex < levels.Length; levelIndex++)
			{
				var level = levels[levelIndex];
				float quality = Mathf.Clamp(level.quality, 0.01f, minimumQuality);
				screenRelativeTransitionHeight *= quality * quality;
				GameObject lodObj = new GameObject(string.Format("Level{0}", levelIndex));
				#if UNITY_EDITOR
					if( Application.isPlaying == false ){
						UnityEditor.Undo.RegisterCreatedObjectUndo (lodObj, "Generate LODs");
					}
				#endif

				var lodParent = lodObj.transform;
				lodParent.parent = lodsParent;
				lodParent.localPosition = Vector3.zero;
				lodParent.localRotation = Quaternion.identity;
				lodParent.localScale = Vector3.one;

				DecimationAlgorithm.StatusReportCallback levelStatusCallback = null;
				if (statusCallback != null)
				{
					levelStatusCallback = (iteration, originalTris, currentTris, targetTris) =>
					{
						statusCallback.Invoke(levelIndex, iteration, originalTris, currentTris, targetTris);
					};
				}

				GameObject staticLodObj = lodObj;
				GameObject skinnedLodObj = lodObj;
				Transform staticLodParent = lodParent;
				Transform skinnedLodParent = lodParent;
				if (meshRenderers.Length > 0 && skinnedRenderers.Length > 0)
				{
					staticLodObj = new GameObject("Static", typeof(MeshFilter), typeof(MeshRenderer));
					#if UNITY_EDITOR
						if( Application.isPlaying == false ){
							UnityEditor.Undo.RegisterCreatedObjectUndo (staticLodObj, "Generate LODs");
						}
					#endif

					staticLodParent = staticLodObj.transform;
					staticLodParent.parent = lodParent;
					staticLodParent.localPosition = Vector3.zero;
					staticLodParent.localRotation = Quaternion.identity;
					staticLodParent.localScale = Vector3.one;

					skinnedLodObj = new GameObject("Skinned", typeof(SkinnedMeshRenderer));
					#if UNITY_EDITOR
						if( Application.isPlaying == false ){
							UnityEditor.Undo.RegisterCreatedObjectUndo (skinnedLodObj, "Generate LODs");
						}
					#endif

					skinnedLodParent = skinnedLodObj.transform;
					skinnedLodParent.parent = lodParent;
					skinnedLodParent.localPosition = Vector3.zero;
					skinnedLodParent.localRotation = Quaternion.identity;
					skinnedLodParent.localScale = Vector3.one;
				}

				Renderer[] staticLodRenderers = null;
				Renderer[] skinnedLodRenderers = null;
				if (meshRenderers.Length > 0)
				{
					if (level.combineMeshes)
					{
						Material[] materials;
						Mesh lodMesh = GenerateStaticLOD(transform, meshRenderers, quality, out materials, levelStatusCallback, preserveBorders, preserveSeams, preserveFoldovers );
						lodMesh.name = string.Format("{0}_static{1}", gameObj.name, levelIndex);
						var meshFilter = staticLodObj.AddComponent<MeshFilter>();
						meshFilter.sharedMesh = lodMesh;
						var lodRenderer = staticLodObj.AddComponent<MeshRenderer>();
						lodRenderer.sharedMaterials = materials;
						SetupLODRenderer(lodRenderer, level);
						staticLodRenderers = new Renderer[] { lodRenderer };
					}
					else
					{
						staticLodRenderers = new Renderer[meshRenderers.Length];

						Material[] materials;
						for (int rendererIndex = 0; rendererIndex < meshRenderers.Length; rendererIndex++)
						{
							var renderer = meshRenderers[rendererIndex];
							Mesh lodMesh = GenerateStaticLOD(transform, renderer, quality, out materials, levelStatusCallback, preserveBorders, preserveSeams, preserveFoldovers );
							lodMesh.name = string.Format("{0}_static{1}_{2}", gameObj.name, levelIndex, rendererIndex);

							var rendererLodObj = new GameObject(renderer.name, typeof(MeshFilter), typeof(MeshRenderer));
							rendererLodObj.transform.parent = staticLodParent;
							rendererLodObj.transform.localPosition = Vector3.zero;
							rendererLodObj.transform.localRotation = Quaternion.identity;
							rendererLodObj.transform.localScale = Vector3.one;
							var meshFilter = rendererLodObj.GetComponent<MeshFilter>();
							meshFilter.sharedMesh = lodMesh;
							var lodRenderer = rendererLodObj.GetComponent<MeshRenderer>();
							lodRenderer.sharedMaterials = materials;
							SetupLODRenderer(lodRenderer, level);
							staticLodRenderers[rendererIndex] = lodRenderer;
						}
					}
				}

				if (skinnedRenderers.Length > 0)
				{
					if (level.combineMeshes)
					{
						Transform[] bones;
						Material[] materials;
						Mesh lodMesh = GenerateSkinnedLOD(transform, skinnedRenderers, quality, out materials, out bones, levelStatusCallback, preserveBorders, preserveSeams, preserveFoldovers );
						lodMesh.name = string.Format("{0}_skinned{1}", gameObj.name, levelIndex);
						Transform rootBone = FindRootBone(transform, bones);

						var lodRenderer = skinnedLodObj.AddComponent<SkinnedMeshRenderer>();
						lodRenderer.sharedMesh = lodMesh;
						lodRenderer.sharedMaterials = materials;
						lodRenderer.rootBone = rootBone;
						lodRenderer.bones = bones;
						SetupLODRenderer(lodRenderer, level);
						skinnedLodRenderers = new Renderer[] { lodRenderer };
					}
					else
					{
						skinnedLodRenderers = new Renderer[skinnedRenderers.Length];

						Transform[] bones;
						Material[] materials;
						for (int rendererIndex = 0; rendererIndex < skinnedRenderers.Length; rendererIndex++)
						{
							var renderer = skinnedRenderers[rendererIndex];
							Mesh lodMesh = GenerateSkinnedLOD(transform, renderer, quality, out materials, out bones, levelStatusCallback, preserveBorders, preserveSeams, preserveFoldovers );
							lodMesh.name = string.Format("{0}_skinned{1}_{2}", gameObj.name, levelIndex, rendererIndex);
							Transform rootBone = FindRootBone(transform, bones);

							var rendererLodObj = new GameObject(renderer.name, typeof(SkinnedMeshRenderer));
							rendererLodObj.transform.parent = skinnedLodParent;
							rendererLodObj.transform.localPosition = Vector3.zero;
							rendererLodObj.transform.localRotation = Quaternion.identity;
							rendererLodObj.transform.localScale = Vector3.one;
							var lodRenderer = rendererLodObj.GetComponent<SkinnedMeshRenderer>();
							lodRenderer.sharedMesh = lodMesh;
							lodRenderer.sharedMaterials = materials;
							lodRenderer.rootBone = rootBone;
							lodRenderer.bones = bones;
							SetupLODRenderer(lodRenderer, level);
							skinnedLodRenderers[rendererIndex] = lodRenderer;
						}
					}
				}

				Renderer[] lodRenderers;
				if (staticLodRenderers != null && skinnedLodRenderers != null)
				{
					lodRenderers = staticLodRenderers.Concat<Renderer>(skinnedLodRenderers).ToArray();
				}
				else if (staticLodRenderers != null)
				{
					lodRenderers = staticLodRenderers;
				}
				else if (skinnedLodRenderers != null)
				{
					lodRenderers = skinnedLodRenderers;
				}
				else
				{
					lodRenderers = new Renderer[0];
				}

				minimumQuality = quality;
				lodLevels[levelIndex + 1] = new LOD(screenRelativeTransitionHeight, lodRenderers);
			}

			lodGroup.SetLODs(lodLevels);
		}
		#endregion

		#region Destroy LODs
		/// <summary>
		/// Destroys the generated LODs for the specified game object.
		/// </summary>
		/// <param name="gameObj">The game object to destroy LODs for.</param>
		public static void DestroyLODs(GameObject gameObj)
		{
			if (gameObj == null)
				throw new System.ArgumentNullException("gameObj");

			var targetTransform = gameObj.transform;
			var lodsParent = targetTransform.Find(ParentGameObjectName);
			if (lodsParent != null)
			{
				if (Application.isPlaying)
				{
					Object.Destroy(lodsParent.gameObject);
				}
				else
				{

					// ORIGINAL
					// Object.DestroyImmediate(lodsParent.gameObject);

					#if UNITY_EDITOR
						UnityEditor.Undo.DestroyObjectImmediate ( lodsParent.gameObject ); 
					#else
						Object.DestroyImmediate(lodsParent.gameObject);
					#endif
				}

				// Also destroy any LOD group on the game object, but only if we found generated LODs on it
				var lodGroup = gameObj.GetComponent<LODGroup>();
				if (lodGroup != null)
				{
					if (Application.isPlaying)
					{
						Object.Destroy(lodGroup);
					}
					else
					{
						//Object.DestroyImmediate(lodGroup);

						#if UNITY_EDITOR
							UnityEditor.Undo.DestroyObjectImmediate ( lodGroup ); 
						#else
							Object.DestroyImmediate(lodGroup);
						#endif
					}
				}
			}
		}
		#endregion
		#endregion
	}
}