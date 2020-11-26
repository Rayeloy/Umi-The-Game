using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BadDog
{
    public enum BGVolumetricDebugMode
    {
        Normal,
        RadialBlur
    }

    [ExecuteInEditMode]
    public class BGVolumetricLighting : MonoBehaviour
    {
        [Header("Materials")]
        public Material volumetricLightingMaterial;

        [Header("Quality Params")]
        public int renderTextureSize = 512;
        [Range(1, 128)]
        public int sampleNum = 9;
        [Range(0.1f, 2)]
        public float sampleDensity = 0.25f;
        [Range(1, 4)]
        public int blurNum = 3;

        [Header("HDR")]
        public bool supportHDR = true;

        [Header("Debug")]
        public BGVolumetricDebugMode debugMode = BGVolumetricDebugMode.Normal;

        private Camera m_AttachedCamera;
        private BGPostProcessingBehavior m_PostProcessingBehavior;

        public static BGVolumetricLighting Instance = null;

        private void OnEnable()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            m_AttachedCamera = GetComponent<Camera>();

            m_PostProcessingBehavior = GetComponent<BGPostProcessingBehavior>();

            if (m_PostProcessingBehavior == null)
            {
                m_PostProcessingBehavior = gameObject.AddComponent<BGPostProcessingBehavior>();
            }

            m_AttachedCamera.depthTextureMode |= DepthTextureMode.Depth;
        }

        private void OnDisable()
        {
            if (m_PostProcessingBehavior != null)
            {
                m_PostProcessingBehavior.enabled = false;
            }
        }

        public bool ReadyToGo()
        {
            if (volumetricLightingMaterial == null)
            {
                return false;
            }

            if (m_AttachedCamera == null || m_PostProcessingBehavior == null)
            {
                return false;
            }

            return true;
        }

        private void Update()
        {
            if (!ReadyToGo())
            {
                m_PostProcessingBehavior.enabled = false;
                return;
            }

            List<BGMainLight> allLights = BGMainLight.GetAllLights();
            List<BGMainLight> inViewLights = new List<BGMainLight>();

            for(int i = 0; i < allLights.Count; i++)
            {
                BGMainLight mainLight = allLights[i];

                if(mainLight.UpdateViewPosition(m_AttachedCamera))
                {
                    inViewLights.Add(mainLight);
                }
            }

            if (inViewLights.Count <= 0)
            {
                m_PostProcessingBehavior.enabled = false;
            }
            else
            {
                m_PostProcessingBehavior.SetInViewLights(inViewLights);
                m_PostProcessingBehavior.enabled = true;
            }
        }
    }
}
