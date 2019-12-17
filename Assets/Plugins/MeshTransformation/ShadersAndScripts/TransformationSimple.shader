Shader "Transformation/Simple"
{
	Properties
	{
	[HideInInspector] _Cutoff( "Mask Clip Value", Float ) = 0.5
			_Albedo("Albedo", 2D) = "white" {}
		_Albedo2("Albedo2", 2D) = "white" {}
		_Color_("Color", Color) = (0.5,0.5,0.5,1)
		_NormalMap("NormalMap", 2D) = "bump" {}
		_NormalMap2("NormalMap2", 2D) = "bump" {}
		_SpecularRGBGlossA("Specular(RGB)Gloss(A)", 2D) = "white" {}
		_SpecularRGBGlossA2("Specular(RGB)Gloss(A)2", 2D) = "white" {}
		_SpecularColorGlossA("SpecularColor\Gloss(A)", Color) = (0,0,0,0)
		_ScaleFresnel("ScaleFresnel", Float) = 0
		_PowerFresnel("PowerFresnel", Float) = 0
		_AO2("AO2", 2D) = "white" {}
		_AO("AO", 2D) = "white" {}
		__scale("scale", Float) = 0
		_Transformation("Transformation", Range( 0 , 1)) = 0
		_Noise("Noise", 2D) = "white" {}
		_Holes("Holes", Float) = 0
		[Toggle]_uv0("uv0", Float) = 0
		[Toggle]_uv1("uv1", Float) = 0
		[Toggle]_uv2("uv2", Float) = 0
		[Toggle]_uv3("uv3", Float) = 0
		[Toggle]_uv0model2("uv0model2", Float) = 0
		[Toggle]_uv1model2("uv1model2", Float) = 0
		[Toggle]_uv2model2("uv2model2", Float) = 0
		[Toggle]_uv3model2("uv3model2", Float) = 0



		[HideInInspector] _texcoord3( "", 2D ) = "white" {}
		[HideInInspector] _texcoord4( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif

						 struct appdata__{
               float4 vertex : POSITION;
    float4 tangent : TANGENT;
    float3 normal : NORMAL;
    float4 texcoord : TEXCOORD0;
    float4 texcoord1 : TEXCOORD1;
	float4 texcoord2 : TEXCOORD2;
	float4 texcoord3 : TEXCOORD3;
    fixed4 color : COLOR;
	uint id : SV_VertexID;
            
         };

		struct Input
		{
			float2 uv_texcoord;
			float2 uv2_texcoord2;
			float2 uv3_texcoord3;
			float2 uv4_texcoord4;
			float3 worldPos;
			INTERNAL_DATA
			float3 worldNormal;
		};

		uniform sampler2D _NormalMap;
		uniform float4 _NormalMap_ST;
				uniform float _uv3;
		uniform float _uv2;
		uniform float _uv1;
		uniform float _uv0;
		uniform sampler2D _NormalMap2;
		uniform float _uv3model2;
		uniform float _uv2model2;
		uniform float _uv1model2;
		uniform float _uv0model2;
		uniform sampler2D _Albedo;
		uniform sampler2D _Albedo2;
		uniform float4 _Albedo_ST;
		uniform float4 _Color_;
		uniform sampler2D _SpecularRGBGlossA;
		uniform sampler2D _SpecularRGBGlossA2;
		uniform float4 _SpecularRGBGlossA_ST;
		uniform float4 _SpecularColorGlossA;
		uniform float _ScaleFresnel;
		uniform float _PowerFresnel;
		uniform sampler2D _AO;
		uniform sampler2D _AO2;
		uniform float4 _AO_ST;
		uniform float _Transformation;
		uniform sampler2D _Noise;
		uniform float4 _Noise_ST;
		uniform float _Holes;
		uniform float _Cutoff = 0.5;
		uniform float __scale;
		float3 RigPos;
		float3 posObj;
		uint id_;
		 #ifdef SHADER_API_D3D11
		StructuredBuffer<float3> _vert_;
		StructuredBuffer<float3> _normal_;
		#endif

		void vertexDataFunc( inout appdata__ v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertex3Pos = v.vertex.xyz;
			float3 vvvv = 0;
			float3 nn = 0;
			float3 delta_;
			delta_ = posObj;
			delta_ = RigPos - delta_;
			#ifdef SHADER_API_D3D11
			vvvv =  _vert_[v.id] - delta_;
			nn = _normal_[v.id];
			#endif
			float3 vvvvv = lerp(0,vvvv,__scale);
			float3 lerpResult86 = lerp( ase_vertex3Pos ,vvvvv, _Transformation);
			v.vertex.xyz = lerpResult86;

			float3 ase_vertexNormal = v.normal.xyz;
			float3 lerpResult169 = lerp( ase_vertexNormal , nn , _Transformation);
			v.normal = lerpResult169;
		}

		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			float2 uv_133 = lerp(lerp(lerp(lerp(i.uv_texcoord,i.uv_texcoord,_uv0),i.uv2_texcoord2,_uv1),i.uv3_texcoord3,_uv2),i.uv4_texcoord4,_uv3);
			float2 uv_234 = lerp(lerp(lerp(lerp(i.uv_texcoord,i.uv_texcoord,_uv0model2),i.uv2_texcoord2,_uv1model2),i.uv3_texcoord3,_uv2model2),i.uv4_texcoord4,_uv3model2);
			float Trans48 = _Transformation;
			float3 lerpResult54 = lerp( UnpackNormal( tex2D( _NormalMap, uv_133 ) ) , UnpackNormal( tex2D( _NormalMap2, uv_234 ) ) , Trans48);
			o.Normal = lerpResult54;
			float4 lerpResult46 = lerp( tex2D( _Albedo, uv_133 ) , tex2D( _Albedo2, uv_234 ) , Trans48);
			float4 blendOpSrc3 = lerpResult46;
			float4 blendOpDest3 = _Color_;
			o.Albedo = ( saturate( (( blendOpDest3 > 0.5 ) ? ( 1.0 - ( 1.0 - 2.0 * ( blendOpDest3 - 0.5 ) ) * ( 1.0 - blendOpSrc3 ) ) : ( 2.0 * blendOpDest3 * blendOpSrc3 ) ) )).rgb;
			float4 tex2DNode5 = tex2D( _SpecularRGBGlossA, uv_133 );
			float4 lerpResult50 = lerp( tex2DNode5 , tex2D( _SpecularRGBGlossA2, uv_234 ) , Trans48);
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float fresnelNDotV8 = dot( WorldNormalVector( i , lerpResult54 ), ase_worldViewDir );
			float fresnelNode8 = ( 0.0 + _ScaleFresnel * pow( 1.0 - fresnelNDotV8, _PowerFresnel ) );
			o.Specular = ( lerpResult50 * _SpecularColorGlossA * clamp( fresnelNode8,0,1) ).rgb;
			o.Smoothness = ( tex2DNode5.a * _SpecularColorGlossA.a );
			float lerpResult52 = lerp( tex2D( _AO, uv_133 ).r , tex2D( _AO2, uv_234 ).r , Trans48);
			o.Occlusion = lerpResult52;
			o.Alpha = 1;
			float4 temp_cast_2 = (1.0).xxxx;
			float2 uv_Noise = i.uv_texcoord * _Noise_ST.xy + _Noise_ST.zw;

			_Holes = lerp (0,_Holes,_Transformation);
			float3 lerpResult18 = lerp( temp_cast_2 , tex2D( _Noise, uv_Noise ) , _Holes);
			#if UNITY_COLORSPACE_GAMMA
            lerpResult18 = GammaToLinearSpace(lerpResult18);
            #endif
			clip( lerpResult18.r - _Cutoff );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardSpecular keepalpha fullforwardshadows vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert( appdata__ v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				fixed3 worldNormal = UnityObjectToWorldNormal( v.normal );
				fixed3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				fixed3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			fixed4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				fixed3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandardSpecular o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandardSpecular, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
}