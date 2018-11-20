Shader "VacuumShaders/Subsurface Scattering/VertexLit (Mobile)" 
{
	Properties 
	{
		[V_MaterialTag]
		_V_MaterialTag("", float) = 0 

		//Default Options
		[V_MaterialTitle]
		_V_MaterialTitle_Default("Default Options", float) = 0


		_Color ("Main Color", Color) = (1,1,1,1)		
		_MainTex ("Base (RGB)", 2D) = "white" {}
		

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
		Tags { "RenderType"="Opaque" "SSSType"="Legacy/OnePass_1"}
		LOD 200
		
		Pass  
		{
			Name "FORWARD"
			Tags { "LightMode" = "ForwardBase" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog

			#pragma multi_compile_fwdbase nodirlightmap 
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			
			
			#pragma shader_feature V_SSS_ADVANCED_TRANSLUSENCY_OFF V_SSS_ADVANCED_TRANSLUSENCY_ON
			
			#pragma shader_feature V_SSS_RIM_OFF V_SSS_RIM_ON

			#define V_SSS_VERTEXLIT
			#define V_SSS_LIGHTCOUNT_ONE

			#include "SSS.cginc"
			
			ENDCG

		} //Pass
	} //SubShader

	CustomEditor "SubsurfaceScatteringMaterial_Editor"

} //Shader
