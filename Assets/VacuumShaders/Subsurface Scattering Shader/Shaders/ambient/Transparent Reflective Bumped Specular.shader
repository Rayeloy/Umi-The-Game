Shader "Hidden/ambient/VacuumShaders/Subsurface Scattering/Transparent/Reflective/Bumped Specular" 
{
	Properties 
	{
		[V_MaterialTag]
		_V_MaterialTag("", float) = 0

		//Default Options
		[V_MaterialTitle]
		_V_MaterialTitle_Default("Default Options", float) = 0


		_Color ("Main Color", Color) = (1,1,1,1)
		_SpecColor ("Specular Color", Color) = (0.5,0.5,0.5,1)
		_Shininess ("Shininess", Range (0.01, 1)) = 0.078125
		_MainTex ("Base (RGB) Trans & RefStrGloss (A)", 2D) = "white" {} 
		_BumpSize("Bump Size", float) = 1
		_BumpMap ("Normalmap", 2D) = "bump" {}
		_ReflectColor ("Reflection Color", Color) = (1,1,1,0.5)
		_Cube ("Reflection Cubemap", Cube) = "_Skybox" { }

		
		//Default Options
		[V_MaterialTitle]
		_V_MaterialTitle_("Unity Advanced Rendering Options", float) = 0

		[HideInInspector] _TransMap ("",2D) = "white" {}
		[HideInInspector] _TransColor ("", color) = (1, 1, 1, 1)
		[HideInInspector] _TransDistortion ("",Range(0,0.5)) = 0.1
		[HideInInspector] _TransPower("",Range(1.0,16.0)) = 1.0
		[HideInInspector] _TransScale("", Float) = 1.0
		[HideInInspector] _TransBackfaceIntensity("", Float) = 0.15

		[HideInInspector] _TransDirectianalLightStrength("", Range(0.0, 1.0)) = 0.2
		[HideInInspector] _TransOtherLightsStrength("", Range(0.0, 1.0)) = 0.5
        [HideInInspector] _LightAttenuationStrength("", Float) = 2
        [HideInInspector] _NormalizeLightVector("", Float) = 1
		[HideInInspector] _V_SSS_Emission("", Float) = 0

		[HideInInspector] _V_SSS_Rim_Color("", color) = (1, 1, 1, 1)
		[HideInInspector] _V_SSS_Rim_Pow("", Range(0.5, 8.0)) = 2.0
	}

	SubShader 
	{
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "SSSType"="Legacy/PixelLit"}
		LOD 400
		
		Pass 
		{
			ZWrite On
			ColorMask 0
		}
		
		CGPROGRAM
		#pragma surface surf TransBlinnPhong vertex:vert addshadow nodynlightmap alpha:fade
		#pragma target 3.0

		
		#pragma shader_feature V_SSS_ADVANCED_TRANSLUSENCY_OFF V_SSS_ADVANCED_TRANSLUSENCY_ON
		#pragma shader_feature V_SSS_RIM_OFF V_SSS_RIM_ON

		#define V_SSS_SPECULAR
		#define V_SSS_BUMPED
		#define V_SSS_REFLECTIVE

		#include "../SSS.cginc"

		ENDCG
	}


	//SM2.0
	SubShader 
	{
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "SSSType"="Legacy/PixelLit"}
		LOD 400
		
		Pass 
		{
			ZWrite On
			ColorMask 0
		}
		
		CGPROGRAM
		#pragma surface surf TransBlinnPhong vertex:vert addshadow nodynlightmap alpha:fade

		
		#pragma shader_feature V_SSS_ADVANCED_TRANSLUSENCY_OFF V_SSS_ADVANCED_TRANSLUSENCY_ON
		#pragma shader_feature V_SSS_RIM_OFF V_SSS_RIM_ON

		#define V_SSS_REFLECTIVE

		#include "../SSS.cginc"

		ENDCG
	}

	FallBack "Legacy Shaders/Transparent/Diffuse"
	CustomEditor "SubsurfaceScatteringMaterial_Editor"
}
