using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BadDog
{
    static class BGShaderIDs
    {
        internal static readonly int _MainLightViewPosition = Shader.PropertyToID("_MainLightViewPosition");
        internal static readonly int _DepthThreshold = Shader.PropertyToID("_DepthThreshold");
        internal static readonly int _LightingRadius = Shader.PropertyToID("_LightingRadius");
        internal static readonly int _LightingSampleWeight = Shader.PropertyToID("_LightingSampleWeight");
        internal static readonly int _LightingDecay = Shader.PropertyToID("_LightingDecay");
        internal static readonly int _LightingIntensity = Shader.PropertyToID("_LightingIntensity");
        internal static readonly int _LightingColor = Shader.PropertyToID("_LightingColor");
        internal static readonly int _SampleNum = Shader.PropertyToID("_SampleNum");
        internal static readonly int _SampleDensity = Shader.PropertyToID("_SampleDensity");
        internal static readonly int _VolumetricLightingTex = Shader.PropertyToID("_VolumetricLightingTex");
    }
}
