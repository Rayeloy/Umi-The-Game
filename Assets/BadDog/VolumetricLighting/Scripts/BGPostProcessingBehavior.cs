using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BadDog
{
    [ExecuteInEditMode]
    public class BGPostProcessingBehavior : MonoBehaviour
    {
        private List<BGMainLight> m_InViewLights;

        private void PrepareOneLight(BGMainLight mainLight, BGVolumetricLighting volumetricLighting, Material material)
        {
            material.SetVector(BGShaderIDs._MainLightViewPosition, mainLight.GetViewPosition());
            material.SetFloat(BGShaderIDs._DepthThreshold, mainLight.depthThreshold);
            material.SetFloat(BGShaderIDs._LightingRadius, mainLight.lightingRadius);
            material.SetFloat(BGShaderIDs._LightingSampleWeight, mainLight.lightingSampleWeight);
            material.SetFloat(BGShaderIDs._LightingDecay, mainLight.lightingDecay);
            material.SetFloat(BGShaderIDs._LightingIntensity, mainLight.lightingIntensity);
            material.SetColor(BGShaderIDs._LightingColor, mainLight.lightingColor);

            material.SetInt(BGShaderIDs._SampleNum, volumetricLighting.sampleNum);
            material.SetFloat(BGShaderIDs._SampleDensity, volumetricLighting.sampleDensity);
        }

        private RenderTextureFormat GetRenderTextureFormat(BGVolumetricLighting volumetricLighting)
        {
            if (volumetricLighting.supportHDR)
            {
                return RenderTextureFormat.DefaultHDR;
            }
            else
            {
                return RenderTextureFormat.Default;
            }
        }

        private void RenderOneLight(BGMainLight mainLight, BGVolumetricLighting volumetricLighting, Material material, RenderTexture originSourceRT, RenderTexture currentSourceRT, RenderTexture destRT)
        {
            PrepareOneLight(mainLight, volumetricLighting, material);

            // create RT
            int rtSize = volumetricLighting.renderTextureSize;
            RenderTexture tempRT1 = RenderTexture.GetTemporary(rtSize, rtSize, 0, GetRenderTextureFormat(volumetricLighting));
            RenderTexture tempRT2 = RenderTexture.GetTemporary(rtSize, rtSize, 0, GetRenderTextureFormat(volumetricLighting));

            // prefilter
            Graphics.Blit(originSourceRT, tempRT1, material, 0);

            RenderTexture lastRT = null;

            // radial blur
            for (int i = 0; i < volumetricLighting.blurNum; i++)
            {
                Graphics.Blit(tempRT1, tempRT2, material, 1);
                lastRT = tempRT2;

                RenderTexture temp = tempRT1;
                tempRT1 = tempRT2;
                tempRT2 = temp;
            }

            material.SetTexture(BGShaderIDs._VolumetricLightingTex, lastRT);

            // final composite
            Graphics.Blit(currentSourceRT, destRT, material, 2);

            RenderTexture.ReleaseTemporary(tempRT1);
            RenderTexture.ReleaseTemporary(tempRT2);
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            BGVolumetricLighting volumetricLighting = BGVolumetricLighting.Instance;
            Material material = volumetricLighting.volumetricLightingMaterial;

            int lightCount = m_InViewLights.Count;

            if (lightCount <= 0)
            {
                Graphics.Blit(source, destination);
                return;
            }

            RenderTexture beginRT = source;

#if UNITY_EDITOR
            if(volumetricLighting.debugMode == BGVolumetricDebugMode.RadialBlur)
            {
                beginRT = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
                Graphics.Blit(source, beginRT, material, 3);
            }
#endif

            if (lightCount == 1)
            {
                RenderOneLight(m_InViewLights[0], volumetricLighting, material, source, beginRT, destination);
            }
            else if (lightCount == 2)
            {
                RenderTexture tempDestRT = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);

                RenderOneLight(m_InViewLights[0], volumetricLighting, material, source, beginRT, tempDestRT);
                RenderOneLight(m_InViewLights[1], volumetricLighting, material, source, tempDestRT, destination);

                RenderTexture.ReleaseTemporary(tempDestRT);
            }
            else
            {
                RenderTexture tempDestRT1 = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
                RenderTexture tempDestRT2 = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);

                RenderOneLight(m_InViewLights[0], volumetricLighting, material, source, beginRT, tempDestRT1);

                RenderTexture lastSourceRT = tempDestRT1;
                RenderTexture lastDestRT = tempDestRT2;

                for (int i = 1; i < m_InViewLights.Count - 1; i++)
                {
                    RenderOneLight(m_InViewLights[i], volumetricLighting, material, source, lastSourceRT, lastDestRT);

                    RenderTexture temp = lastSourceRT;
                    lastSourceRT = lastDestRT;
                    lastDestRT = temp;
                }

                RenderOneLight(m_InViewLights[m_InViewLights.Count - 1], volumetricLighting, material, source, lastSourceRT, destination);

                RenderTexture.ReleaseTemporary(tempDestRT1);
                RenderTexture.ReleaseTemporary(tempDestRT2);
            }

#if UNITY_EDITOR
            if(volumetricLighting.debugMode == BGVolumetricDebugMode.RadialBlur)
            {
                if (beginRT != source)
                {
                    RenderTexture.ReleaseTemporary(beginRT);
                }
            }
#endif
        }

        public void SetInViewLights(List<BGMainLight> inViewLights)
        {
            m_InViewLights = inViewLights;
        }
    }
}
