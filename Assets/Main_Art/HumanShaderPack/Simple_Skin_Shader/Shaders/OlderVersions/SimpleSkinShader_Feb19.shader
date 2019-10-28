// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "RRF_HumanShaders/Skin Shaders/RnD/SimpleSkinShader_Feb19"
{
	Properties
	{
		_BaseSSSGlow("BaseSSSGlow", Range( 0 , 1)) = 0
		_BaseSSS_Lerp("Base-SSS_Lerp", Range( 0 , 1)) = 0.58506
		_Shininess("Shininess", Range( 0.01 , 1)) = 0.1
		_ScatterNoiseScale("ScatterNoiseScale", Range( 0 , 2)) = 2
		_ScatterColor("ScatterColor", Color) = (0.9264706,0.8583477,0.8583477,0)
		_Light_Bias("Light_Bias", Range( 0 , 1)) = 0
		_Light_Scale("Light_Scale", Range( 0 , 10)) = 0
		_LightScatter("LightScatter", Range( 0 , 1)) = 0.25
		_DirectScatter("DirectScatter", Range( 0 , 5)) = 0
		_BackScatter("BackScatter", Range( 0 , 5)) = 0
		_ScatterAdd("ScatterAdd", Range( -1 , 1)) = 0
		_ShadowPower("ShadowPower", Range( 0 , 1)) = 0.6
		_Fuzz("Fuzz", Range( 0 , 3)) = 0
		_FuzzColor("FuzzColor", Color) = (0.5147059,0.5071367,0.5071367,0)
		_SSS_BaseColorWeightA("SSS_BaseColor-Weight(A)", Color) = (0.8970588,0.5474697,0.5474697,1)
		_SSS_ColorWeightA("SSS_Color-Weight(A)", 2D) = "white" {}
		_Albedo("Albedo", 2D) = "white" {}
		_AlbedoColor("AlbedoColor", Color) = (1,1,1,0)
		_NormalMap("NormalMap", 2D) = "bump" {}
		_NormalBumpScale("NormalBumpScale", Range( 0 , 2)) = 1
		_Curvature("Curvature", 2D) = "white" {}
		_Curvature_Power("Curvature_Power", Range( 0.1 , 4)) = 1
		_MetalGloss("Metal-Gloss", 2D) = "white" {}
		_SmoothnessAdjust("SmoothnessAdjust", Range( -1 , 1)) = 0
		_MicroNormalWeightR("MicroNormalWeight(R)", 2D) = "white" {}
		_MicroNormal("MicroNormal", 2D) = "bump" {}
		_MicroNormalPower("MicroNormalPower", Range( 0 , 3)) = 2
		_MicroNormalTiling("MicroNormalTiling", Range( 0.1 , 50)) = 15
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "UnityStandardUtils.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float3 worldNormal;
			INTERNAL_DATA
			float2 uv_texcoord;
			float3 worldPos;
		};

		struct SurfaceOutputCustomLightingCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		uniform float4 _AlbedoColor;
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform float _DirectScatter;
		uniform float _ScatterAdd;
		uniform sampler2D _Curvature;
		uniform float4 _Curvature_ST;
		uniform float _Curvature_Power;
		uniform float4 _SSS_BaseColorWeightA;
		uniform sampler2D _SSS_ColorWeightA;
		uniform float4 _SSS_ColorWeightA_ST;
		uniform float _BaseSSS_Lerp;
		uniform float4 _FuzzColor;
		uniform float _Fuzz;
		uniform sampler2D _MetalGloss;
		uniform float4 _MetalGloss_ST;
		uniform float _SmoothnessAdjust;
		uniform float _NormalBumpScale;
		uniform sampler2D _NormalMap;
		uniform float4 _NormalMap_ST;
		uniform float _Shininess;
		uniform float _MicroNormalPower;
		uniform sampler2D _MicroNormal;
		uniform float _MicroNormalTiling;
		uniform sampler2D _MicroNormalWeightR;
		uniform float4 _MicroNormalWeightR_ST;
		uniform float _BaseSSSGlow;
		uniform float _BackScatter;
		uniform float _ShadowPower;
		uniform float4 _ScatterColor;
		uniform float _ScatterNoiseScale;
		uniform float _Light_Bias;
		uniform float _Light_Scale;
		uniform float _LightScatter;


		float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }

		float snoise( float2 v )
		{
			const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
			float2 i = floor( v + dot( v, C.yy ) );
			float2 x0 = v - i + dot( i, C.xx );
			float2 i1;
			i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
			float4 x12 = x0.xyxy + C.xxzz;
			x12.xy -= i1;
			i = mod2D289( i );
			float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
			float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
			m = m * m;
			m = m * m;
			float3 x = 2.0 * frac( p * C.www ) - 1.0;
			float3 h = abs( x ) - 0.5;
			float3 ox = floor( x + 0.5 );
			float3 a0 = x - ox;
			m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
			float3 g;
			g.x = a0.x * x0.x + h.x * x0.y;
			g.yz = a0.yz * x12.xz + h.yz * x12.yw;
			return 130.0 * dot( m, g );
		}


		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			#ifdef UNITY_PASS_FORWARDBASE
			float ase_lightAtten = data.atten;
			if( _LightColor0.a == 0)
			ase_lightAtten = 0;
			#else
			float3 ase_lightAttenRGB = gi.light.color / ( ( _LightColor0.rgb ) + 0.000001 );
			float ase_lightAtten = max( max( ase_lightAttenRGB.r, ase_lightAttenRGB.g ), ase_lightAttenRGB.b );
			#endif
			#if defined(HANDLE_SHADOWS_BLENDING_IN_GI)
			half bakedAtten = UnitySampleBakedOcclusion(data.lightmapUV.xy, data.worldPos);
			float zDist = dot(_WorldSpaceCameraPos - data.worldPos, UNITY_MATRIX_V[2].xyz);
			float fadeDist = UnityComputeShadowFadeDistance(data.worldPos, zDist);
			ase_lightAtten = UnityMixRealtimeAndBakedShadows(data.atten, bakedAtten, UnityComputeShadowFade(fadeDist));
			#endif
			SurfaceOutputStandard s105 = (SurfaceOutputStandard ) 0;
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			float4 tex2DNode38 = tex2D( _Albedo, uv_Albedo );
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float3 objToWorldDir29 = mul( unity_ObjectToWorld, float4( ase_vertex3Pos, 0 ) ).xyz;
			float3 normalizeResult30 = normalize( objToWorldDir29 );
			float3 ase_worldPos = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float dotResult3 = dot( normalizeResult30 , ase_worldlightDir );
			float clampResult57 = clamp( ( dotResult3 + _ScatterAdd ) , 0.0 , 1.0 );
			float2 uv_Curvature = i.uv_texcoord * _Curvature_ST.xy + _Curvature_ST.zw;
			float4 temp_cast_0 = (_Curvature_Power).xxxx;
			float4 lerpResult79 = lerp( float4( 0,0,0,0 ) , pow( tex2D( _Curvature, uv_Curvature ) , temp_cast_0 ) , (0.0 + (_Curvature_Power - 0.0) * (1.0 - 0.0) / (4.0 - 0.0)));
			float4 clampResult62 = clamp( lerpResult79 , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
			float2 uv_SSS_ColorWeightA = i.uv_texcoord * _SSS_ColorWeightA_ST.xy + _SSS_ColorWeightA_ST.zw;
			float4 tex2DNode24 = tex2D( _SSS_ColorWeightA, uv_SSS_ColorWeightA );
			float4 temp_output_26_0 = ( _SSS_BaseColorWeightA * tex2DNode24 );
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float dotResult93 = dot( ase_worldlightDir , ase_worldViewDir );
			float clampResult104 = clamp( ( ( dotResult93 + 0.98 ) * 10.0 ) , 0.0 , 1.0 );
			float4 lerpResult130 = lerp( tex2DNode38 , tex2DNode24 , _BaseSSS_Lerp);
			float4 clampResult49 = clamp( ( ( _DirectScatter * clampResult57 * clampResult62 * temp_output_26_0 ) + ( clampResult104 * ( _AlbedoColor * lerpResult130 ) ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float fresnelNdotV31 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode31 = ( 0.0 + _Fuzz * pow( 1.0 - fresnelNdotV31, 3.0 ) );
			float4 lerpResult74 = lerp( clampResult49 , _FuzzColor , fresnelNode31);
			float2 uv_MetalGloss = i.uv_texcoord * _MetalGloss_ST.xy + _MetalGloss_ST.zw;
			float4 tex2DNode63 = tex2D( _MetalGloss, uv_MetalGloss );
			float lerpResult73 = lerp( tex2DNode63.a , 0.0 , fresnelNode31);
			float clampResult88 = clamp( ( lerpResult73 + _SmoothnessAdjust ) , 0.0 , 1.0 );
			float4 temp_cast_1 = (clampResult88).xxxx;
			float4 temp_output_43_0_g9 = temp_cast_1;
			float3 normalizeResult4_g10 = normalize( ( ase_worldViewDir + ase_worldlightDir ) );
			float2 uv_NormalMap = i.uv_texcoord * _NormalMap_ST.xy + _NormalMap_ST.zw;
			float3 tex2DNode45 = UnpackScaleNormal( tex2D( _NormalMap, uv_NormalMap ), _NormalBumpScale );
			float3 normalizeResult64_g9 = normalize( (WorldNormalVector( i , tex2DNode45 )) );
			float dotResult19_g9 = dot( normalizeResult4_g10 , normalizeResult64_g9 );
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			float3 temp_output_40_0_g9 = ( ase_lightColor.rgb * ase_lightAtten );
			float dotResult14_g9 = dot( normalizeResult64_g9 , ase_worldlightDir );
			UnityGI gi34_g9 = gi;
			float3 diffNorm34_g9 = normalizeResult64_g9;
			gi34_g9 = UnityGI_Base( data, 1, diffNorm34_g9 );
			float3 indirectDiffuse34_g9 = gi34_g9.indirect.diffuse + diffNorm34_g9 * 0.0001;
			float4 temp_output_42_0_g9 = lerpResult74;
			float4 lerpResult159 = lerp( ( lerpResult74 + float4( ( ( (temp_output_43_0_g9).rgb * (temp_output_43_0_g9).a * pow( max( dotResult19_g9 , 0.0 ) , ( _Shininess * 128.0 ) ) * temp_output_40_0_g9 ) + ( ( ( temp_output_40_0_g9 * max( dotResult14_g9 , 0.0 ) ) + indirectDiffuse34_g9 ) * (temp_output_42_0_g9).rgb ) ) , 0.0 ) ) , tex2DNode38 , 0.5);
			float4 lerpResult165 = lerp( ( _AlbedoColor * tex2DNode38 ) , lerpResult159 , _BaseSSS_Lerp);
			s105.Albedo = lerpResult165.rgb;
			float2 temp_cast_4 = (_MicroNormalTiling).xx;
			float2 uv_TexCoord171 = i.uv_texcoord * temp_cast_4;
			float2 uv_MicroNormalWeightR = i.uv_texcoord * _MicroNormalWeightR_ST.xy + _MicroNormalWeightR_ST.zw;
			float3 lerpResult173 = lerp( tex2DNode45 , BlendNormals( UnpackScaleNormal( tex2D( _MicroNormal, uv_TexCoord171 ), _MicroNormalPower ) , tex2DNode45 ) , tex2D( _MicroNormalWeightR, uv_MicroNormalWeightR ).r);
			s105.Normal = WorldNormalVector( i , lerpResult173 );
			float temp_output_2_0_g5 = _ShadowPower;
			float temp_output_3_0_g5 = ( 1.0 - temp_output_2_0_g5 );
			float3 appendResult7_g5 = (float3(temp_output_3_0_g5 , temp_output_3_0_g5 , temp_output_3_0_g5));
			float3 temp_output_126_0 = ( ( ( clampResult62 + ( ase_lightAtten * ase_lightColor ) ).rgb * temp_output_2_0_g5 ) + appendResult7_g5 );
			float3 Scatter128 = temp_output_126_0;
			float4 clampResult56 = clamp( ( temp_output_26_0 * ( _SSS_BaseColorWeightA.a * tex2DNode24.a ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
			float2 temp_cast_8 = (( _ScatterNoiseScale * 1000.0 )).xx;
			float2 uv_TexCoord147 = i.uv_texcoord * temp_cast_8;
			float simplePerlin2D148 = snoise( uv_TexCoord147 );
			float2 appendResult152 = (float2(simplePerlin2D148 , simplePerlin2D148));
			float2 clampResult158 = clamp( appendResult152 , float2( 0.5,0.5 ) , float2( 1,0 ) );
			float2 temp_output_140_0 = ( ( clampResult158 + _Light_Bias ) * _Light_Scale );
			float3 objToWorldDir134 = mul( unity_ObjectToWorld, float4( ase_vertex3Pos, 0 ) ).xyz;
			float3 normalizeResult136 = normalize( objToWorldDir134 );
			float dotResult137 = dot( ase_worldlightDir , normalizeResult136 );
			float clampResult163 = clamp( dotResult137 , 0.0 , 1.0 );
			float4 lerpResult144 = lerp( ( ( ( _BaseSSSGlow * lerpResult130 ) + ( ( float4( ( ( _BackScatter * ( 1.0 - clampResult104 ) ) * Scatter128 * Scatter128 * Scatter128 ) , 0.0 ) * clampResult56 ) * ase_lightColor.a ) ) * float4( temp_output_126_0 , 0.0 ) ) , ( _ScatterColor * float4( temp_output_140_0, 0.0 , 0.0 ) * clampResult163 ) , _LightScatter);
			s105.Emission = lerpResult144.rgb;
			s105.Metallic = tex2DNode63.r;
			s105.Smoothness = clampResult88;
			s105.Occlusion = 1.0;

			data.light = gi.light;

			UnityGI gi105 = gi;
			#ifdef UNITY_PASS_FORWARDBASE
			Unity_GlossyEnvironmentData g105 = UnityGlossyEnvironmentSetup( s105.Smoothness, data.worldViewDir, s105.Normal, float3(0,0,0));
			gi105 = UnityGlobalIllumination( data, s105.Occlusion, s105.Normal, g105 );
			#endif

			float3 surfResult105 = LightingStandard ( s105, viewDir, gi105 ).rgb;
			surfResult105 += s105.Emission;

			#ifdef UNITY_PASS_FORWARDADD//105
			surfResult105 -= s105.Emission;
			#endif//105
			c.rgb = surfResult105;
			c.a = 1;
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			o.Normal = float3(0,0,1);
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardCustomLighting keepalpha fullforwardshadows 

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
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
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
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputCustomLightingCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputCustomLightingCustom, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	//CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16204
21;253;1272;773;-2365.88;1757.043;1;True;False
Node;AmplifyShaderEditor.CommentaryNode;78;-918.687,-1657.276;Float;False;889.334;398.333;Curvature;5;61;7;60;79;62;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;77;-938.4332,-1232.552;Float;False;1108.382;385.6998;SimpleSSS;7;2;28;29;30;68;3;69;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;61;-906.7864,-1396.689;Float;False;Property;_Curvature_Power;Curvature_Power;24;0;Create;True;0;0;False;0;1;1.34;0.1;4;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;7;-886.368,-1605.669;Float;True;Property;_Curvature;Curvature;23;0;Create;True;0;0;False;0;None;36bdb461d755436469d897ba5bdcd44f;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PosVertexDataNode;28;-888.4332,-1179.816;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;60;-568.4681,-1613.79;Float;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;76;-965.8088,-828.8407;Float;False;597.4731;458.7154;Albedo;4;38;40;41;92;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TransformDirectionNode;29;-607.2095,-1182.552;Float;False;Object;World;False;Fast;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TFHCRemapNode;80;-571.4392,-1408.739;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;4;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;109;1493.733,-100.218;Float;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.LerpOp;79;-327.5943,-1514.224;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LightAttenuation;106;1471.626,-216.0878;Float;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;2;-893.6961,-990.7667;Float;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;92;-574.5845,-585.1484;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;75;-941.9908,-337.8607;Float;False;1004.829;503.8087;Main SSS Map;5;24;25;27;26;23;;1,1,1,1;0;0
Node;AmplifyShaderEditor.NormalizeNode;30;-325.0698,-1169.971;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ClampOpNode;62;-172.4582,-1513.124;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;68;-597.0049,-976.291;Float;False;Property;_ScatterAdd;ScatterAdd;13;0;Create;True;0;0;False;0;0;0.03;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;93;-349.9818,-788.248;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;108;1733.305,-166.297;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;38;-915.8088,-600.1253;Float;True;Property;_Albedo;Albedo;19;0;Create;True;0;0;False;0;None;0260083bbedd7c6429e6919fc5564e85;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;24;-887.7233,-64.05204;Float;True;Property;_SSS_ColorWeightA;SSS_Color-Weight(A);18;0;Create;True;0;0;False;0;None;c3e7538a6c896864d8bc0989e452ddaa;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DotProductOpNode;3;-119.7678,-1146.372;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;131;-325.18,-432.0236;Float;False;Property;_BaseSSS_Lerp;Base-SSS_Lerp;1;0;Create;True;0;0;False;0;0.58506;0.444;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;122;1956.685,-268.4408;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;123;1700.837,-54.80024;Float;False;Property;_ShadowPower;ShadowPower;14;0;Create;True;0;0;False;0;0.6;0.791;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;69;15.94911,-1059.531;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;25;-891.9908,-287.8607;Float;False;Property;_SSS_BaseColorWeightA;SSS_BaseColor-Weight(A);17;0;Create;True;0;0;False;0;0.8970588,0.5474697,0.5474697,1;1,0.08088237,0.08088237,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;85;39.89126,-1806.763;Float;False;1649.776;553.4731;Control;8;71;42;54;55;56;53;127;129;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;41;-910.8441,-778.8407;Float;False;Property;_AlbedoColor;AlbedoColor;20;0;Create;True;0;0;False;0;1,1,1,0;0.8308824,0.7606029,0.7575692,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;130;442.5629,-479.0949;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;102;-195.0309,-767.8531;Float;False;ConstantBiasScale;-1;;2;63208df05c83e8e49a48ffbdce2e43a0;0;3;3;FLOAT;0;False;1;FLOAT;0.98;False;2;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;86;772.435,-443.9589;Float;False;674.7306;340.6641;Fuzz;3;89;31;33;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;167;-1632.532,650.1257;Float;False;Property;_ScatterNoiseScale;ScatterNoiseScale;6;0;Create;True;0;0;False;0;2;2;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;42;89.89128,-1645.714;Float;False;Property;_DirectScatter;DirectScatter;11;0;Create;True;0;0;False;0;0;5;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;126;2098.151,-218.5042;Float;False;Lerp White To;-1;;5;047d7c189c36a62438973bad9d37b1c2;0;2;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ClampOpNode;104;103.4699,-800.4407;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;146;-1105.176,297.4633;Float;False;1591.749;616.5276;MicroScatter;5;152;151;148;147;158;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;-402.8018,-262.4104;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;87;764.8707,-814.0339;Float;False;1021.492;310.8159;Normal;3;45;46;63;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;33;814.0356,-369.9691;Float;False;Property;_Fuzz;Fuzz;15;0;Create;True;0;0;False;0;0;0;0;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;168;-1291.595,649.8776;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;1000;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;57;210.213,-1064.721;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;-523.0641,-741.1591;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;132;563.0756,148.6573;Float;False;1435.461;680.1272;ScatterLight;11;141;140;139;138;137;136;135;134;133;145;163;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;53;96.26378,-1756.763;Float;False;Property;_BackScatter;BackScatter;12;0;Create;True;0;0;False;0;0;0.61;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;43;597.1438,-1226.49;Float;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;147;-1093.53,552.036;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;2000,2000;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FresnelNode;31;1145.918,-393.9589;Float;False;Standard;WorldNormal;ViewDir;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;3;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;95;241.0678,-669.2186;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;94;249.5262,-874.0358;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;63;1463.761,-691.4241;Float;True;Property;_MetalGloss;Metal-Gloss;25;0;Create;True;0;0;False;0;None;a049867252a39ad4185eaca07e577617;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;-422.4095,-9.845176;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;128;2578.492,-229.501;Float;False;Scatter;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;-106.1614,-135.8701;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;129;648.6948,-1575.3;Float;False;128;Scatter;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;44;976.8253,-1027.785;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;73;1823.824,-523.4587;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;54;545.054,-1728.234;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;83;1500.945,-320.6255;Float;False;Property;_SmoothnessAdjust;SmoothnessAdjust;26;0;Create;True;0;0;False;0;0;0.05;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;148;-825.5602,547.0882;Float;False;Simplex2D;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;133;613.0756,433.2955;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;56;855.6223,-1409.29;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TransformDirectionNode;134;879.8236,390.0749;Float;False;Object;World;False;Fast;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ClampOpNode;49;1393.87,-929.8268;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;46;814.8707,-721.9401;Float;False;Property;_NormalBumpScale;NormalBumpScale;22;0;Create;True;0;0;False;0;1;0.91;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;151;-557.3419,562.8593;Float;False;FLOAT;1;0;FLOAT;0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.ColorNode;89;822.6454,-285.4754;Float;False;Property;_FuzzColor;FuzzColor;16;0;Create;True;0;0;False;0;0.5147059,0.5071367,0.5071367,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;84;2026.561,-503.6128;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;127;850.1414,-1665.033;Float;False;4;4;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;71;1387.667,-1433.63;Float;False;Property;_BaseSSSGlow;BaseSSSGlow;0;0;Create;True;0;0;False;0;0;0.237;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;58;1129.132,-1138.361;Float;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.NormalizeNode;136;1150.419,422.3747;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;45;1133.145,-764.0339;Float;True;Property;_NormalMap;NormalMap;21;0;Create;True;0;0;False;0;None;e2479a6af2d8c4b4b8479c74b02a5982;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;152;-218.573,559.0026;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;74;1719.877,-901.6283;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0.8014706,0.7174218,0.6718209,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;88;2169.057,-606.3607;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;1112.855,-1620.776;Float;False;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;135;623.2177,275.94;Float;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;172;2692.942,-1509.973;Float;False;Property;_MicroNormalTiling;MicroNormalTiling;31;0;Create;True;0;0;False;0;15;13.4;0.1;50;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;139;788.4786,742.5335;Float;False;Property;_Light_Scale;Light_Scale;9;0;Create;True;0;0;False;0;0;2.19;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;59;1517.202,-1218.469;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;72;1799.137,-1221.052;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;138;826.1392,652.511;Float;False;Property;_Light_Bias;Light_Bias;8;0;Create;True;0;0;False;0;0;0.223;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;158;10.58585,532.0358;Float;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT2;1,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DotProductOpNode;137;1382.247,284.2278;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;175;2805.967,-1348.888;Float;False;Property;_MicroNormalPower;MicroNormalPower;30;0;Create;True;0;0;False;0;2;1.34;0;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;120;2326.539,-714.1979;Float;False;Blinn-Phong Light;2;;9;cf814dba44d007a4e958d2ddd5813da6;0;3;42;COLOR;0,0,0,0;False;52;FLOAT3;0,0,0;False;43;COLOR;0,0,0,0;False;2;FLOAT3;0;FLOAT;57
Node;AmplifyShaderEditor.TextureCoordinatesNode;171;3090.21,-1511.339;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;163;1662.126,350.3398;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;141;1592,166.7121;Float;False;Property;_ScatterColor;ScatterColor;7;0;Create;True;0;0;False;0;0.9264706,0.8583477,0.8583477,0;1,0.3308824,0.3308824,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;164;2543.524,-539.9807;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;70;2064.509,-1140.559;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;169;3510.438,-1517.435;Float;True;Property;_MicroNormal;MicroNormal;29;0;Create;True;0;0;False;0;None;7b97ea4a2cda39341948cd735a02e5d0;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;121;2639.439,-773.9588;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;160;2860.397,-812.7064;Float;False;Constant;_Float0;Float 0;24;0;Create;True;0;0;False;0;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;140;1546.102,544.9302;Float;False;ConstantBiasScale;-1;;11;63208df05c83e8e49a48ffbdce2e43a0;0;3;3;FLOAT2;0,0;False;1;FLOAT;0.5;False;2;FLOAT;0.1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;143;2100.594,201.1475;Float;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;166;3187.783,-1090.39;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;170;3467.974,-1314.906;Float;True;Property;_MicroNormalWeightR;MicroNormalWeight(R);27;0;Create;True;0;0;False;0;None;2028939b4d5715f4db521dbb3af614a6;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;142;2087.044,-333.8391;Float;False;Property;_LightScatter;LightScatter;10;0;Create;True;0;0;False;0;0.25;0.044;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;159;3062.049,-943.2079;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.BlendNormalsNode;174;3886.29,-1346.055;Float;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;110;2618.086,-946.5806;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;144;2858.938,-567.1876;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;165;3349.115,-1051.593;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;173;4193.828,-1260.317;Float;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CustomStandardSurface;105;4335.635,-996.5568;Float;False;Metallic;Tangent;6;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,1;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ClampOpNode;145;1897.481,537.6832;Float;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;1,1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;177;3525.38,-1083.543;Float;False;Property;_DetailMaskContrast;DetailMaskContrast;28;0;Create;True;0;0;False;0;0;0.1176471;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;4835.559,-1241.153;Float;False;True;2;Float;ASEMaterialInspector;0;0;CustomLighting;RRF_HumanShaders/Skin Shaders/RnD/SimpleSkinShader_Feb19;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;60;0;7;0
WireConnection;60;1;61;0
WireConnection;29;0;28;0
WireConnection;80;0;61;0
WireConnection;79;1;60;0
WireConnection;79;2;80;0
WireConnection;30;0;29;0
WireConnection;62;0;79;0
WireConnection;93;0;2;0
WireConnection;93;1;92;0
WireConnection;108;0;106;0
WireConnection;108;1;109;0
WireConnection;3;0;30;0
WireConnection;3;1;2;0
WireConnection;122;0;62;0
WireConnection;122;1;108;0
WireConnection;69;0;3;0
WireConnection;69;1;68;0
WireConnection;130;0;38;0
WireConnection;130;1;24;0
WireConnection;130;2;131;0
WireConnection;102;3;93;0
WireConnection;126;1;122;0
WireConnection;126;2;123;0
WireConnection;104;0;102;0
WireConnection;26;0;25;0
WireConnection;26;1;24;0
WireConnection;168;0;167;0
WireConnection;57;0;69;0
WireConnection;40;0;41;0
WireConnection;40;1;130;0
WireConnection;43;0;42;0
WireConnection;43;1;57;0
WireConnection;43;2;62;0
WireConnection;43;3;26;0
WireConnection;147;0;168;0
WireConnection;31;2;33;0
WireConnection;95;0;104;0
WireConnection;95;1;40;0
WireConnection;94;0;104;0
WireConnection;27;0;25;4
WireConnection;27;1;24;4
WireConnection;128;0;126;0
WireConnection;23;0;26;0
WireConnection;23;1;27;0
WireConnection;44;0;43;0
WireConnection;44;1;95;0
WireConnection;73;0;63;4
WireConnection;73;2;31;0
WireConnection;54;0;53;0
WireConnection;54;1;94;0
WireConnection;148;0;147;0
WireConnection;56;0;23;0
WireConnection;134;0;133;0
WireConnection;49;0;44;0
WireConnection;151;0;148;0
WireConnection;84;0;73;0
WireConnection;84;1;83;0
WireConnection;127;0;54;0
WireConnection;127;1;129;0
WireConnection;127;2;129;0
WireConnection;127;3;129;0
WireConnection;136;0;134;0
WireConnection;45;5;46;0
WireConnection;152;0;151;0
WireConnection;152;1;151;0
WireConnection;74;0;49;0
WireConnection;74;1;89;0
WireConnection;74;2;31;0
WireConnection;88;0;84;0
WireConnection;55;0;127;0
WireConnection;55;1;56;0
WireConnection;59;0;55;0
WireConnection;59;1;58;2
WireConnection;72;0;71;0
WireConnection;72;1;130;0
WireConnection;158;0;152;0
WireConnection;137;0;135;0
WireConnection;137;1;136;0
WireConnection;120;42;74;0
WireConnection;120;52;45;0
WireConnection;120;43;88;0
WireConnection;171;0;172;0
WireConnection;163;0;137;0
WireConnection;164;0;126;0
WireConnection;70;0;72;0
WireConnection;70;1;59;0
WireConnection;169;1;171;0
WireConnection;169;5;175;0
WireConnection;121;0;74;0
WireConnection;121;1;120;0
WireConnection;140;3;158;0
WireConnection;140;1;138;0
WireConnection;140;2;139;0
WireConnection;143;0;141;0
WireConnection;143;1;140;0
WireConnection;143;2;163;0
WireConnection;166;0;41;0
WireConnection;166;1;38;0
WireConnection;159;0;121;0
WireConnection;159;1;38;0
WireConnection;159;2;160;0
WireConnection;174;0;169;0
WireConnection;174;1;45;0
WireConnection;110;0;70;0
WireConnection;110;1;164;0
WireConnection;144;0;110;0
WireConnection;144;1;143;0
WireConnection;144;2;142;0
WireConnection;165;0;166;0
WireConnection;165;1;159;0
WireConnection;165;2;131;0
WireConnection;173;0;45;0
WireConnection;173;1;174;0
WireConnection;173;2;170;1
WireConnection;105;0;165;0
WireConnection;105;1;173;0
WireConnection;105;2;144;0
WireConnection;105;3;63;0
WireConnection;105;4;88;0
WireConnection;145;0;140;0
WireConnection;0;13;105;0
ASEEND*/
//CHKSM=9057F813AA7592B7138B52421105B4477F460367