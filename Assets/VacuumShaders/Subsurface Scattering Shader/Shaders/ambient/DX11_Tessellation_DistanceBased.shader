Shader "Hidden/ambient/VacuumShaders/Subsurface Scattering/Tessellation (DX11)/Distance Based"
{
	Properties 
	{
		[V_MaterialTag]
		_V_MaterialTag("", float) = 0

		//Default Options
		[V_MaterialTitle]
		_V_MaterialTitle_Default("Default Options", float) = 0


		_Color ("Main Color", Color) = (1,1,1,1)
		_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
		_Shininess ("Shininess", Range (0.03, 1)) = 0.078125
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_BumpMap ("Normalmap", 2D) = "bump" {}
		_BumpSize("Bump Size", float) = 1


		//Default Options
		[V_MaterialTitle]
		_V_MaterialTitle_DX11("Tessellation Options", float) = 0
		_V_SSS_Tessellation ("Tessellation", Range(1, 32)) = 4		
		_V_SSS_Displacement ("Displacement", float) = 0.3
		_V_SSS_DisplaceTex ("Displace Texture (R)", 2D) = "gray" {}					
		_V_SSS_Tessellation_MinDistance("Min Distance", float) = 10
		_V_SSS_Tessellation_MaxDistance("Max Distance", float) = 25
		
		
		//Default Options
		[V_MaterialTitle]
		_V_MaterialTitle_("Unity Advanced Rendering Options", float) = 0

		[HideInInspector] _TransMap ("",2D) = "white" {}
		[HideInInspector] _TransColor ("", color) = (1, 1, 1, 1)
		[HideInInspector] _TransDistortion ("",Range(0, 0.5)) = 0.1
		[HideInInspector] _TransPower("",Range(1.0, 16.0)) = 1.0
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
            Tags { "RenderType"="SSS_Tessellate_DB_Opaque" "SSSType"="PixelLit"}
            LOD 300
            
            CGPROGRAM
            #pragma surface surf TransBlinnPhong vertex:vert addshadow vertex:disp tessellate:tessCalc nolightmap nodynlightmap noinstancing
            #pragma target 5.0
            #include "Tessellation.cginc"

			
			#pragma shader_feature V_SSS_ADVANCED_TRANSLUSENCY_OFF V_SSS_ADVANCED_TRANSLUSENCY_ON
			#pragma shader_feature V_SSS_RIM_OFF V_SSS_RIM_ON
			 
			#define V_SSS_TESSELLATION
			#define V_SSS_TESSELLATION_DISTANCE_BASED
			#define V_SSS_SPECULAR
			#define V_SSS_BUMPED

			#include "../SSS.cginc" 
			
           
            ENDCG
        }

		CustomEditor "SubsurfaceScatteringMaterial_Editor"
        FallBack "VacuumShaders/Subsurface Scattering/Bumped Specular"
    }
