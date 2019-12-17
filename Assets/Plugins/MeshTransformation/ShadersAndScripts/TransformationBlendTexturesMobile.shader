

Shader "Transformation/BlendTexturesMobile" {
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _MainTex ("Base", 2D) = "white" {}
	_MainTex2 ("Base2", 2D) = "white" {}
    _SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
	_SpecTex ("SpecularTex (RGB) Gloss (A)", 2D) = "white" {}
	_SpecTex2 ("SpecularTex2 (RGB) Gloss2 (A)", 2D) = "white" {}
    _Gloss ("Gloss", Range (0.03, 1)) = 0.078125
	_TransScattering("Scaterring Falloff", Range( 1 , 10)) = 2
	_Transformation ("Transformation",Range (0,1)) = 0

    _BumpMap ("Normalmap", 2D) = "bump" {}
	_BumpMap2 ("Normalmap2", 2D) = "bump" {}
	_Scale("Scale", Float) = 1
	_PowersFresnel("PowersFresnel", Float) = 1
	 _SSS ("SSS", Color) = (1,1,1,1)
	 		[HideInInspector] _texcoord3( "", 2D ) = "white" {}
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
	 [Enum(UV0,0,UV1,1,UV2,2)] _UVs2Model ("UVs2Model", int) = 0
}

CGINCLUDE



sampler2D _MainTex;
sampler2D _SpecTex;
sampler2D _BumpMap;
sampler2D _MainTex2;
sampler2D _SpecTex2;
sampler2D _BumpMap2; 
fixed4 _Color;
fixed4 _SSS;
half _Gloss;
float _UVs2Model;
half _Transformation;
uniform half _TransScattering;
uniform float _Scale;
uniform float _PowersFresnel;

struct Input {
			float3 worldPos;
			float2 uv_texcoord;
			float2 uv2_texcoord2;
			float2 uv3_texcoord3;
			INTERNAL_DATA
			float3 worldNormal;
    float2 uv_MainTex;
		float2 uv_SpecTex;
		float2 uv_SpecTex2;
    float2 uv_BumpMap;
	float2 uv_BumpMap2;
};

void surf (Input IN, inout SurfaceOutput o) {

float2 uv2Model;

     if (_UVs2Model == 0) {
     uv2Model = IN.uv_texcoord;
	}
	if (_UVs2Model == 1) {
     uv2Model = IN.uv2_texcoord2;
	}
	if (_UVs2Model == 2) {
     uv2Model = IN.uv3_texcoord3;
	}




#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 
			float4 _lightColor = 0;
			#else 
			float4 _lightColor = _LightColor0;
			#endif 
			


    float3 _worldPos = IN.worldPos;
	float3 _worldViewDir = normalize( UnityWorldSpaceViewDir( _worldPos ) );
	float3 _worldNormal = WorldNormalVector( IN, float3( 0, 0, 1 ) );
	float fresnelNdotV1 = dot( _worldNormal, _worldViewDir );
	float fresnelNode1 = ( 0.0 + _Scale * pow( 1.0 - fresnelNdotV1, _PowersFresnel ) );
    fixed4 tex = lerp( tex2D(_MainTex, IN.uv_texcoord),tex2D(_MainTex2, float2(uv2Model.x,uv2Model.y)),_Transformation);
	float4 clampResult5 = clamp( ( fresnelNode1) , 0 , 1 );
	fixed4 spc = lerp ( tex2D(_SpecTex, IN.uv_texcoord),tex2D(_SpecTex2, float2(uv2Model.x,uv2Model.y)),_Transformation) * clampResult5; 
	
	
				#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 
			float3 _worldlightDir = 0;
			#else 
			float3 _worldlightDir = normalize( UnityWorldSpaceLightDir( _worldPos ) );
			#endif 

			half transVdotL = pow( saturate( dot( _worldViewDir, -_worldlightDir ) ), _TransScattering );

			half3 translucency = (transVdotL + _lightColor) * _SSS;

    o.Albedo = tex.rgb * _Color.rgb + translucency;
    o.Gloss = spc.a * _Gloss;
    o.Alpha = tex.a * _Color.a;
    o.Specular = spc.rgb;
	
    o.Normal = lerp ( UnpackNormal(tex2D(_BumpMap, IN.uv_texcoord)), UnpackNormal(tex2D(_BumpMap2, float2(uv2Model.x,uv2Model.y))),_Transformation);
}
ENDCG

SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 400

    CGPROGRAM
    #pragma surface surf BlinnPhong
    #pragma target 3.0
    ENDCG
}

SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 400

    CGPROGRAM
    #pragma surface surf BlinnPhong nodynlightmap
	#pragma target 3.0
    ENDCG
}

FallBack "Legacy Shaders/Specular"
}
