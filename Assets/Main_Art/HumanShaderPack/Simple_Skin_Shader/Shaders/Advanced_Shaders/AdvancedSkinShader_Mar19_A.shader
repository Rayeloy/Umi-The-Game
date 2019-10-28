// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "RRF_HumanShaders/Skin Shaders/AdvancedSkinShader_Mar19_A"
{
	Properties
	{
		_SSSDirectPower("SSSDirectPower", Range( 1 , 10)) = 0
		_AlbedoAndSSS_Mixing("AlbedoAndSSS_Mixing", Range( 0 , 1)) = 0.58506
		_Increase_SSS("Increase_SSS", Range( 0 , 1.5)) = 0
		_Increase_WeightMap("Increase_WeightMap", Range( 0.5 , 4)) = 1
		_Increase_BackLitAmount("Increase_BackLitAmount", Range( 1 , 10)) = 0
		_PorosityPower("PorosityPower", Range( 0.01 , 10)) = 0
		_PorosityEffect("PorosityEffect", Range( 0 , 1)) = 0.1
		_SkinGlow("SkinGlow", Range( 0 , 1)) = 0
		_FuzzColorAmountA("FuzzColor-Amount(A)", Color) = (0.5147059,0.5071367,0.5071367,0)
		_SSS_BaseColor("SSS_BaseColor", Color) = (0.8970588,0.5474697,0.5474697,1)
		[Header(Main Textures)][Space(8)]_SSS_ColorWeightA("SSS_Color-Weight(A)", 2D) = "white" {}
		_Albedo("Albedo", 2D) = "white" {}
		_AlbedoColor("AlbedoColor", Color) = (1,1,1,0)
		_NormalMap("NormalMap", 2D) = "bump" {}
		_NormalBumpScale("NormalBumpScale", Range( 0 , 2)) = 1
		_MetalRGlossA("Metal(R)-Gloss(A)", 2D) = "black" {}
		_MetalAdjust("MetalAdjust", Range( -0.5 , 0.5)) = 0
		_SmoothnessAdjust("SmoothnessAdjust", Range( -1 , 1)) = 0
		[Header(Micro Normal details)][Space(8)]_MicroNormalWeightR("MicroNormalWeight(R)", 2D) = "white" {}
		_MicroNormal("MicroNormal", 2D) = "bump" {}
		_MicroNormalPower("MicroNormalPower", Range( 0 , 3)) = 2
		_MicroNormalTiling("MicroNormalTiling", Range( 0.1 , 50)) = 15
		[Header(MakeUp Control)][Space(8)]_MakeupRGB("Makeup-RGB", 2D) = "black" {}
		_MakeUpColor1GlitterAlpha("MakeUpColor1-Glitter(Alpha)", Color) = (0.1242972,0.6363294,0.8897059,0)
		_MakeUpColor2GlitterAlpha("MakeUpColor2-Glitter(Alpha)", Color) = (0.5624278,0.1242972,0.8897059,0)
		_MakeUpColor3GlitterAlpha("MakeUpColor3-Glitter(Alpha)", Color) = (0.8676471,0,0.4128799,0)
		_ColorPowerRGB("ColorPower(RGB)", Vector) = (0,0,0,0)
		_GlossAdjustingRGB("GlossAdjusting(RGB)", Vector) = (0,0,0,0)
		[Header(Glitter Control)][Space(8)]_GlitterScale("GlitterScale", Range( 1 , 10)) = 9
		_GlitterAmount("GlitterAmount", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "UnityCG.cginc"
		#include "UnityShaderVariables.cginc"
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
		uniform float4 _SSS_BaseColor;
		uniform sampler2D _SSS_ColorWeightA;
		uniform float4 _SSS_ColorWeightA_ST;
		uniform float _Increase_SSS;
		uniform float _SSSDirectPower;
		uniform float _MicroNormalPower;
		uniform sampler2D _MicroNormal;
		uniform float _MicroNormalTiling;
		uniform float _NormalBumpScale;
		uniform sampler2D _NormalMap;
		uniform float4 _NormalMap_ST;
		uniform float _PorosityPower;
		uniform sampler2D _MetalRGlossA;
		uniform float4 _MetalRGlossA_ST;
		uniform float _PorosityEffect;
		uniform float _AlbedoAndSSS_Mixing;
		uniform float4 _FuzzColorAmountA;
		uniform float4 _MakeUpColor1GlitterAlpha;
		uniform float3 _ColorPowerRGB;
		uniform sampler2D _MakeupRGB;
		uniform float4 _MakeupRGB_ST;
		uniform float4 _MakeUpColor2GlitterAlpha;
		uniform float4 _MakeUpColor3GlitterAlpha;
		uniform sampler2D _MicroNormalWeightR;
		uniform float4 _MicroNormalWeightR_ST;
		uniform float _SkinGlow;
		uniform float _Increase_BackLitAmount;
		uniform float _Increase_WeightMap;
		uniform float _MetalAdjust;
		uniform float _SmoothnessAdjust;
		uniform float _GlitterScale;
		uniform float _GlitterAmount;
		uniform float3 _GlossAdjustingRGB;


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
			SurfaceOutputStandard s105 = (SurfaceOutputStandard ) 0;
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			float4 Albedo_input234 = ( _AlbedoColor * tex2D( _Albedo, uv_Albedo ) );
			float2 uv_SSS_ColorWeightA = i.uv_texcoord * _SSS_ColorWeightA_ST.xy + _SSS_ColorWeightA_ST.zw;
			float4 tex2DNode24 = tex2D( _SSS_ColorWeightA, uv_SSS_ColorWeightA );
			float4 SSSColor449 = ( _SSS_BaseColor * tex2DNode24 );
			float4 clampResult511 = clamp( ( tex2DNode24 * _Increase_SSS ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
			float4 clampResult494 = clamp( ( SSSColor449 * clampResult511 ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
			float4 SSSweightColor_Input242 = clampResult494;
			float4 temp_output_452_0 = ( SSSColor449 * SSSweightColor_Input242 );
			float4 clampResult454 = clamp( ( Albedo_input234 + temp_output_452_0 ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
			float3 ase_worldPos = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_vertexNormal = mul( unity_WorldToObject, float4( ase_worldNormal, 0 ) );
			float dotResult390 = dot( ase_worldlightDir , ase_vertexNormal );
			float clampResult391 = clamp( dotResult390 , 0.0 , 1.0 );
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			float clampResult393 = clamp( ase_lightColor.a , 0.0 , 1.0 );
			float3 clampResult493 = clamp( ( clampResult391 * ase_lightColor.rgb * clampResult393 * _SSSDirectPower ) , float3( 0,0,0 ) , float3( 1,1,1 ) );
			float3 SoftDirectionalLight394 = clampResult493;
			float2 temp_cast_0 = (_MicroNormalTiling).xx;
			float2 uv_TexCoord171 = i.uv_texcoord * temp_cast_0;
			float2 uv_NormalMap = i.uv_texcoord * _NormalMap_ST.xy + _NormalMap_ST.zw;
			float3 NormalMap_input252 = UnpackScaleNormal( tex2D( _NormalMap, uv_NormalMap ), _NormalBumpScale );
			float3 temp_output_174_0 = BlendNormals( UnpackScaleNormal( tex2D( _MicroNormal, uv_TexCoord171 ), _MicroNormalPower ) , NormalMap_input252 );
			float temp_output_459_0 = (temp_output_174_0).z;
			float3 GeneratedPorosity460 = ( SoftDirectionalLight394 * temp_output_459_0 * temp_output_459_0 * temp_output_459_0 );
			float4 blendOpSrc470 = float4( ( GeneratedPorosity460 * _PorosityPower ) , 0.0 );
			float4 blendOpDest470 = temp_output_452_0;
			float4 temp_output_472_0 = ( clampResult454 + ( saturate( 2.0f*blendOpDest470*blendOpSrc470 + blendOpDest470*blendOpDest470*(1.0f - 2.0f*blendOpSrc470) )) );
			float2 uv_MetalRGlossA = i.uv_texcoord * _MetalRGlossA_ST.xy + _MetalRGlossA_ST.zw;
			float4 tex2DNode63 = tex2D( _MetalRGlossA, uv_MetalRGlossA );
			float baseGloss593 = tex2DNode63.a;
			float4 lerpResult595 = lerp( temp_output_472_0 , ( temp_output_472_0 * ( baseGloss593 * _PorosityPower ) ) , _PorosityEffect);
			float4 lerpResult261 = lerp( Albedo_input234 , lerpResult595 , _AlbedoAndSSS_Mixing);
			float4 MixedAlbedo370 = lerpResult261;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float fresnelNdotV31 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode31 = ( 0.1 + _FuzzColorAmountA.a * pow( 1.0 - fresnelNdotV31, 0.8 ) );
			float clampResult369 = clamp( ( _FuzzColorAmountA.a * fresnelNode31 ) , 0.0 , 1.0 );
			float4 lerpResult74 = lerp( MixedAlbedo370 , _FuzzColorAmountA , clampResult369);
			float4 AlbedoAndFuzz304 = lerpResult74;
			float2 uv_MakeupRGB = i.uv_texcoord * _MakeupRGB_ST.xy + _MakeupRGB_ST.zw;
			float4 tex2DNode180 = tex2D( _MakeupRGB, uv_MakeupRGB );
			float4 lerpResult197 = lerp( AlbedoAndFuzz304 , _MakeUpColor1GlitterAlpha , ( _ColorPowerRGB.x * tex2DNode180.r ));
			float4 lerpResult198 = lerp( lerpResult197 , _MakeUpColor2GlitterAlpha , ( _ColorPowerRGB.y * tex2DNode180.g ));
			float4 lerpResult199 = lerp( lerpResult198 , _MakeUpColor3GlitterAlpha , ( _ColorPowerRGB.z * tex2DNode180.b ));
			float4 OUTPUT_ALBEDO315 = lerpResult199;
			s105.Albedo = OUTPUT_ALBEDO315.rgb;
			float2 uv_MicroNormalWeightR = i.uv_texcoord * _MicroNormalWeightR_ST.xy + _MicroNormalWeightR_ST.zw;
			float3 lerpResult173 = lerp( NormalMap_input252 , temp_output_174_0 , tex2D( _MicroNormalWeightR, uv_MicroNormalWeightR ).r);
			float3 OUTPUT_NORMAL254 = lerpResult173;
			s105.Normal = WorldNormalVector( i , OUTPUT_NORMAL254 );
			float4 clampResult408 = clamp( ( ( MixedAlbedo370 * ( SSSweightColor_Input242 + float4( SoftDirectionalLight394 , 0.0 ) ) ) + MixedAlbedo370 ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
			float dotResult572 = dot( ase_worldViewDir , ase_worldlightDir );
			float BackLit575 = ( ( ( 1.0 - dotResult572 ) + 0.01 ) * 0.3 );
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float3 objToWorldDir548 = mul( unity_ObjectToWorld, float4( ase_vertex3Pos, 0 ) ).xyz;
			float3 normalizeResult546 = normalize( objToWorldDir548 );
			float dotResult559 = dot( ase_worldlightDir , normalizeResult546 );
			float clampResult563 = clamp( dotResult559 , 0.0 , 1.0 );
			float4 SSS_WeightBased567 = ( _SSS_BaseColor * tex2DNode24 * _Increase_SSS * ( tex2DNode24.a * _Increase_WeightMap ) );
			float4 BackScatter560 = ( ( BackLit575 * _Increase_BackLitAmount ) * clampResult563 * SSS_WeightBased567 );
			float4 clampResult504 = clamp( ( ( clampResult408 * _SkinGlow ) + ( _SkinGlow * MixedAlbedo370 ) + BackScatter560 ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
			float4 FinalSSS534 = ( ( float4( SoftDirectionalLight394 , 0.0 ) * SSSweightColor_Input242 ) + ( SSSweightColor_Input242 * BackScatter560 ) );
			float4 lerpResult536 = lerp( clampResult504 , FinalSSS534 , 0.75);
			float4 OUTPUT_EMISSIVE280 = ( lerpResult536 * float4( ase_lightColor.rgb , 0.0 ) );
			s105.Emission = OUTPUT_EMISSIVE280.rgb;
			float clampResult355 = clamp( ( tex2DNode63.r + _MetalAdjust ) , 0.0 , 1.0 );
			float OUTPUT_METALLIC278 = clampResult355;
			s105.Metallic = OUTPUT_METALLIC278;
			float clampResult88 = clamp( ( tex2DNode63.a + _SmoothnessAdjust ) , 0.0 , 1.0 );
			float InitialGloss309 = clampResult88;
			float2 temp_cast_7 = (( _GlitterScale * 100.0 )).xx;
			float2 uv_TexCoord187 = i.uv_texcoord * temp_cast_7;
			float simplePerlin2D186 = snoise( uv_TexCoord187 );
			float clampResult225 = clamp( ( simplePerlin2D186 + 0.5 ) , 0.0 , 1.0 );
			float clampResult201 = clamp( ( ( tex2DNode180.r * _MakeUpColor1GlitterAlpha.a ) + ( tex2DNode180.g * _MakeUpColor2GlitterAlpha.a ) + ( tex2DNode180.b * _MakeUpColor3GlitterAlpha.a ) ) , 0.0 , 1.0 );
			float Glitter_Mask313 = clampResult201;
			float lerpResult203 = lerp( InitialGloss309 , clampResult225 , ( Glitter_Mask313 * _GlitterAmount ));
			float clampResult478 = clamp( ( ( _GlossAdjustingRGB.x * tex2DNode180.r ) + ( _GlossAdjustingRGB.y * tex2DNode180.g ) + ( _GlossAdjustingRGB.z * tex2DNode180.b ) ) , 0.0 , 1.0 );
			float prefinal_glossAdd479 = clampResult478;
			float clampResult482 = clamp( ( lerpResult203 + prefinal_glossAdd479 ) , 0.0 , 1.0 );
			float OUTPUT_SMOOTHNESS317 = clampResult482;
			s105.Smoothness = OUTPUT_SMOOTHNESS317;
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
841;369;1009;462;3625.217;429.6302;2.046027;True;False
Node;AmplifyShaderEditor.CommentaryNode;395;-1342.428,-608.7443;Float;False;1954.403;508.5303;;10;394;392;391;393;390;389;388;387;493;521;Soft Directional Light;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;387;-1292.428,-558.7452;Float;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NormalVertexDataNode;388;-1262.62,-395.9503;Float;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;87;-4679.807,-1245.637;Float;False;994.0213;274.6934;Base Normal Map;3;252;45;46;Base Normal;1,1,1,1;0;0
Node;AmplifyShaderEditor.DotProductOpNode;390;-884.1282,-511.8004;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;178;-4703.481,-2435.661;Float;False;2178.654;524.5915;MicroDetail;13;460;457;458;459;253;254;173;170;174;169;171;175;172;Micro Detail 1;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;46;-4629.807,-1153.543;Float;False;Property;_NormalBumpScale;NormalBumpScale;14;0;Create;True;0;0;False;0;1;0.38;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;389;-932.7605,-360.8334;Float;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.CommentaryNode;75;-5004.229,-824.8174;Float;False;1649.391;769.2059;Main SSS Map - Multiplied with a custom color and weight in alpha;13;242;26;449;450;25;347;24;494;511;567;568;573;574;Main SSS;1,1,1,1;0;0
Node;AmplifyShaderEditor.ClampOpNode;393;-663.4059,-331.7101;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;45;-4311.533,-1195.637;Float;True;Property;_NormalMap;NormalMap;13;0;Create;True;0;0;False;0;None;7453b5d6856919a4aae138e8f0f6829c;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;521;-782.9129,-192.9464;Float;False;Property;_SSSDirectPower;SSSDirectPower;0;0;Create;True;0;0;False;0;0;3.6;1;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;172;-4653.482,-2378.2;Float;False;Property;_MicroNormalTiling;MicroNormalTiling;21;0;Create;True;0;0;False;0;15;21.1;0.1;50;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;24;-4946.859,-551.8019;Float;True;Property;_SSS_ColorWeightA;SSS_Color-Weight(A);10;0;Create;True;0;0;False;2;Header(Main Textures);Space(8);None;072266bafd15a4940ad8a1a2ab663193;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;25;-4911.355,-763.3995;Float;False;Property;_SSS_BaseColor;SSS_BaseColor;9;0;Create;True;0;0;False;0;0.8970588,0.5474697,0.5474697,1;0.4411765,0.09731833,0.09731833,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;391;-704.9952,-508.4424;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;347;-4938.717,-324.5598;Float;False;Property;_Increase_SSS;Increase_SSS;2;0;Create;True;0;0;False;0;0;1.15;0;1.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;175;-4648.225,-2266.722;Float;False;Property;_MicroNormalPower;MicroNormalPower;20;0;Create;True;0;0;False;0;2;1.61;0;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;252;-3943.14,-1200.794;Float;False;NormalMap_input;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;392;-385.0535,-433.1653;Float;False;4;4;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;171;-4256.215,-2379.566;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;450;-4621.923,-658.4293;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;566;-4498.808,-465.4683;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;253;-3828.544,-2158.87;Float;False;252;NormalMap_input;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;169;-3835.99,-2385.661;Float;True;Property;_MicroNormal;MicroNormal;19;0;Create;True;0;0;False;0;None;287acedd1e86ed140951c44576dc5b5c;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;493;-94.26614,-437.6166;Float;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;1,1,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;449;-4469.023,-668.6228;Float;False;SSSColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;511;-4253.584,-527.0928;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.BlendNormalsNode;174;-3460.137,-2214.282;Float;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;394;221.5843,-446.1836;Float;False;SoftDirectionalLight;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;-4154.205,-743.1174;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;76;-4719.989,-1758.384;Float;False;1068.948;447.9859;Albedo;4;40;38;41;234;Base Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;457;-3469.935,-2374.919;Float;False;394;SoftDirectionalLight;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ComponentMaskNode;459;-3193.861,-2241.852;Float;False;False;False;True;False;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;494;-3864.969,-740.8824;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;38;-4669.989,-1529.668;Float;True;Property;_Albedo;Albedo;11;0;Create;True;0;0;False;0;None;c944fd4c1cdd66f4c88c89c567f64328;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;41;-4665.024,-1708.383;Float;False;Property;_AlbedoColor;AlbedoColor;12;0;Create;True;0;0;False;0;1,1,1,0;0.8088235,0.6055512,0.5590398,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;265;-3032.157,59.03728;Float;False;3854.945;619.559;SSS mixing;38;280;592;591;536;533;537;504;448;538;590;446;447;411;408;413;404;407;370;405;261;406;472;131;470;454;453;471;452;463;466;262;451;263;594;595;596;597;598;SSS Mixing to Emissive;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;458;-2966.505,-2366.518;Float;False;4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;242;-3617.812,-753.892;Float;False;SSSweightColor_Input;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;-4300.229,-1707.043;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;460;-2802.007,-2367.722;Float;False;GeneratedPorosity;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;312;-2281.074,-1207.685;Float;False;1960.138;427.2329;MetalGloss;10;278;230;231;309;88;84;63;83;355;593;Metal Gloss Control;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;451;-3005.405,287.844;Float;False;242;SSSweightColor_Input;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;263;-3003.383,180.2572;Float;False;449;SSSColor;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;234;-3912.13,-1705.353;Float;False;Albedo_input;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;466;-2991.373,475.1551;Float;False;Property;_PorosityPower;PorosityPower;5;0;Create;True;0;0;False;0;0;3.54;0.01;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;463;-2990.952,377.998;Float;False;460;GeneratedPorosity;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;562;960.6237,-665.1611;Float;False;2317.368;522.178;Comment;16;560;559;546;547;548;545;563;569;570;575;572;571;577;579;584;585;BackScatterZone;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;262;-3003.372,102.5902;Float;False;234;Albedo_input;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;452;-2671.615,240.388;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;63;-2244.384,-1147.649;Float;True;Property;_MetalRGlossA;Metal(R)-Gloss(A);15;0;Create;True;0;0;False;0;None;8e3ce40f0a75d3941a9d1b1472af79ff;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;547;995.4952,-501.4427;Float;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;471;-2685.082,379.5627;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;571;1024.947,-660.6982;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;453;-2496.783,157.8141;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;593;-1882.306,-1071.198;Float;False;baseGloss;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;470;-2438.212,296.7642;Float;False;SoftLight;True;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DotProductOpNode;572;1269.61,-616.9232;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;454;-2274.572,169.5032;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;594;-2314.883,396.5905;Float;False;593;baseGloss;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;545;1010.624,-332.0079;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;573;-4929.18,-218.8256;Float;False;Property;_Increase_WeightMap;Increase_WeightMap;3;0;Create;True;0;0;False;0;1;2.63;0.5;4;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;598;-2099.906,402.1575;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TransformDirectionNode;548;1229.287,-336.4384;Float;False;Object;World;False;Fast;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;472;-2066.103,190.0452;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;574;-4615.21,-235.2885;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;577;1435.162,-611.6655;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;596;-1918.495,303.6951;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;597;-2364.647,591.4506;Float;False;Property;_PorosityEffect;PorosityEffect;6;0;Create;True;0;0;False;0;0.1;0.688;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.NormalizeNode;546;1496.462,-332.1394;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;579;1627.005,-627.9496;Float;False;ConstantBiasScale;-1;;1;63208df05c83e8e49a48ffbdce2e43a0;0;3;3;FLOAT;0;False;1;FLOAT;0.01;False;2;FLOAT;0.3;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;568;-4450.584,-362.6037;Float;False;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;567;-4234.693,-344.0746;Float;False;SSS_WeightBased;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DotProductOpNode;559;1701.206,-496.3173;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;585;2019.48,-565.0562;Float;False;Property;_Increase_BackLitAmount;Increase_BackLitAmount;4;0;Create;True;0;0;False;0;0;3.63;1;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;131;-2367.714,503.6816;Float;False;Property;_AlbedoAndSSS_Mixing;AlbedoAndSSS_Mixing;1;0;Create;True;0;0;False;0;0.58506;0.813;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;319;-2177.104,-2527.472;Float;False;2236.685;1113.347;Makeup control;20;185;183;184;200;197;198;180;201;313;199;315;246;409;473;477;478;479;483;488;487;Final Albedo / Glitter mask;1,1,1,1;0;0
Node;AmplifyShaderEditor.LerpOp;595;-1755.981,219.4789;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;86;-3156.416,-660.7691;Float;False;1700.685;608.5836;Fuzz;7;304;74;241;368;89;31;369;Fuzzy Fresnel;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;575;2064.213,-635.6961;Float;False;BackLit;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;89;-3090.35,-465.6487;Float;False;Property;_FuzzColorAmountA;FuzzColor-Amount(A);8;0;Create;True;0;0;False;0;0.5147059,0.5071367,0.5071367,0;0.5147059,0.5071367,0.5071367,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;409;-1401.117,-1911.665;Float;False;379.0802;478.442;Glitter Amounts;3;219;220;218;;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;406;-1510.64,309.5649;Float;False;394;SoftDirectionalLight;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;183;-2113.089,-1998.231;Float;False;Property;_MakeUpColor1GlitterAlpha;MakeUpColor1-Glitter(Alpha);23;0;Create;True;0;0;False;0;0.1242972,0.6363294,0.8897059,0;0.5234646,0.6911765,0.5523804,0.347;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;584;2424.107,-629.122;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;405;-1466.288,200.5992;Float;False;242;SSSweightColor_Input;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;185;-2106.978,-1617.178;Float;False;Property;_MakeUpColor3GlitterAlpha;MakeUpColor3-Glitter(Alpha);25;0;Create;True;0;0;False;0;0.8676471,0,0.4128799,0;0.7426471,0.7426471,0.7426471,0.397;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;311;-193.991,-1270.3;Float;False;2973.652;481.6161;Glitter;15;317;203;225;310;227;223;226;314;186;187;208;207;481;480;482;Final Smoothness;1,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;184;-2110.926,-1819.874;Float;False;Property;_MakeUpColor2GlitterAlpha;MakeUpColor2-Glitter(Alpha);24;0;Create;True;0;0;False;0;0.5624278,0.1242972,0.8897059,0;0.8049055,0.7209126,0.8676471,0.397;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;261;-1604.207,110.7042;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;563;1971.351,-495.3729;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;180;-2181.019,-2202.593;Float;True;Property;_MakeupRGB;Makeup-RGB;22;0;Create;True;0;0;False;2;Header(MakeUp Control);Space(8);None;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;570;1975.138,-315.5743;Float;False;567;SSS_WeightBased;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FresnelNode;31;-2769.972,-241.1636;Float;False;Standard;WorldNormal;ViewDir;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0.1;False;2;FLOAT;1;False;3;FLOAT;0.8;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;370;-1407.664,106.1852;Float;False;MixedAlbedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;407;-1184.874,240.5101;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;207;-143.9907,-921.8503;Float;False;Property;_GlitterScale;GlitterScale;28;0;Create;True;0;0;False;2;Header(Glitter Control);Space(8);9;2.2;1;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;569;2590.761,-501.3543;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;220;-1191.037,-1566.223;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;218;-1351.117,-1861.665;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;219;-1273.962,-1722.762;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;208;234.4757,-935.9693;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;100;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;487;-1752.895,-2503.79;Float;False;219.5601;355.948;GlossGroup;3;476;475;474;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;200;-929.446,-1848.662;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;473;-2081.907,-2495.121;Float;False;Property;_GlossAdjustingRGB;GlossAdjusting(RGB);27;0;Create;True;0;0;False;0;0,0,0;0.61,0.24,0.66;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;404;-956.6782,134.9512;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;83;-2244.384,-916.6738;Float;False;Property;_SmoothnessAdjust;SmoothnessAdjust;17;0;Create;True;0;0;False;0;0;0.31;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;532;977.4856,-1964.562;Float;False;1284.869;628.5698;Comment;6;530;510;534;531;544;561;FinalSSS;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;368;-2480.312,-379.4802;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;560;2797.202,-504.4125;Float;False;BackScatter;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;201;-695.2656,-1850.087;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;369;-2272.443,-424.7597;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;510;1015.708,-1887.073;Float;False;394;SoftDirectionalLight;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;561;1078.202,-1601.755;Float;False;560;BackScatter;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;476;-1702.895,-2280.842;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;474;-1702.335,-2453.79;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;84;-1908.385,-980.6751;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;413;-816.7832,187.8063;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;530;1012.785,-1795.216;Float;False;242;SSSweightColor_Input;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;475;-1702.894,-2366.775;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;187;482.2706,-1005.91;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1000,1200;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;241;-3098.38,-597.2559;Float;False;370;MixedAlbedo;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;411;-977.2863,318.717;Float;False;Property;_SkinGlow;SkinGlow;7;0;Create;True;0;0;False;0;0;0.067;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;88;-1729.576,-998.1245;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;313;-445.8202,-1855.959;Float;False;Glitter_Mask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;447;-959.0251,422.661;Float;False;370;MixedAlbedo;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;477;-1524.781,-2382.901;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;544;1448.894,-1667.134;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;74;-2055.244,-561.645;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0.8014706,0.7174218,0.6718209,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;186;761.9098,-1020.143;Float;False;Simplex2D;1;0;FLOAT2;1000,1000;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;408;-667.019,177.5062;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;531;1403.419,-1822.817;Float;False;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;304;-1691.158,-563.4164;Float;False;AlbedoAndFuzz;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;309;-1556.384,-980.6751;Float;False;InitialGloss;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;478;-1358.225,-2381.621;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;483;-2057.848,-2349.797;Float;False;Property;_ColorPowerRGB;ColorPower(RGB);26;0;Create;True;0;0;False;0;0,0,0;1.33,0.67,0.93;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;226;547.7787,-1125.804;Float;False;Property;_GlitterAmount;GlitterAmount;29;0;Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;488;-1683.709,-2197.955;Float;False;220.9199;357.3585;ColorGroup;3;486;485;484;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;446;-481.3569,196.6181;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;314;1035.927,-1221.963;Float;False;313;Glitter_Mask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;564;1673.097,-1793.294;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;590;-526.687,420.0675;Float;False;560;BackScatter;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;538;-473.297,308.2753;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;223;1015.183,-1019.186;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;484;-1632.247,-2146.492;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;246;-1415.578,-2226.087;Float;False;304;AlbedoAndFuzz;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;479;-1178.859,-2390.59;Float;False;prefinal_glossAdd;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;534;1971.674,-1803.401;Float;False;FinalSSS;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;227;1332.763,-1220.3;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;225;1409.457,-1023.896;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;448;-199.31,206.0702;Float;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;310;1295.761,-1109.088;Float;False;309;InitialGloss;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;485;-1631.98,-2058.986;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;231;-1575.337,-1062.08;Float;False;Property;_MetalAdjust;MetalAdjust;16;0;Create;True;0;0;False;0;0;0.051;-0.5;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;203;1642.017,-1112.935;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;197;-1113.476,-2210.261;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;504;-33.79388,198.2862;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;537;-62.84393,403.0091;Float;False;Constant;_Float1;Float 1;31;0;Create;True;0;0;False;0;0.75;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;481;1558.321,-955.0491;Float;False;479;prefinal_glossAdd;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;533;-69.42792,326.831;Float;False;534;FinalSSS;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;486;-1630.327,-1972.134;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;536;213.679,270.6219;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;230;-1136.435,-1115.152;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;591;-48.01729,502.2393;Float;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.LerpOp;198;-829.2002,-2111.364;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;480;1900.321,-1037.049;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;170;-4284.967,-2183.281;Float;True;Property;_MicroNormalWeightR;MicroNormalWeight(R);18;0;Create;True;0;0;False;2;Header(Micro Normal details);Space(8);None;9622fdfc9a82dd74ab2630f3fa315307;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;355;-885.7393,-1103.325;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;173;-3152.6,-2128.544;Float;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;592;404.8382,359.4747;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;482;2079.321,-1034.049;Float;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;199;-533.9693,-2038.997;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;317;2370.813,-1032.355;Float;False;OUTPUT_SMOOTHNESS;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;410;2558.642,-2009.262;Float;False;1031.204;584.865;Comment;7;255;318;316;105;0;279;281;OUTPUT;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;278;-596.3832,-1076.675;Float;False;OUTPUT_METALLIC;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;254;-2841.946,-2135.283;Float;False;OUTPUT_NORMAL;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;315;-200.421,-2058.483;Float;False;OUTPUT_ALBEDO;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;280;569.1858,267.398;Float;False;OUTPUT_EMISSIVE;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;316;2571.096,-1953.959;Float;False;315;OUTPUT_ALBEDO;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;255;2568.642,-1869.507;Float;False;254;OUTPUT_NORMAL;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;281;2583.801,-1737.52;Float;False;280;OUTPUT_EMISSIVE;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;318;2571.141,-1491.581;Float;False;317;OUTPUT_SMOOTHNESS;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;279;2576.279,-1589.344;Float;False;278;OUTPUT_METALLIC;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CustomStandardSurface;105;3004.584,-1802.105;Float;False;Metallic;Tangent;6;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,1;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;3326.845,-1959.263;Float;False;True;2;Float;ASEMaterialInspector;0;0;CustomLighting;RRF_HumanShaders/Skin Shaders/AdvancedSkinShader_Mar19_A;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;390;0;387;0
WireConnection;390;1;388;0
WireConnection;393;0;389;2
WireConnection;45;5;46;0
WireConnection;391;0;390;0
WireConnection;252;0;45;0
WireConnection;392;0;391;0
WireConnection;392;1;389;1
WireConnection;392;2;393;0
WireConnection;392;3;521;0
WireConnection;171;0;172;0
WireConnection;450;0;25;0
WireConnection;450;1;24;0
WireConnection;566;0;24;0
WireConnection;566;1;347;0
WireConnection;169;1;171;0
WireConnection;169;5;175;0
WireConnection;493;0;392;0
WireConnection;449;0;450;0
WireConnection;511;0;566;0
WireConnection;174;0;169;0
WireConnection;174;1;253;0
WireConnection;394;0;493;0
WireConnection;26;0;449;0
WireConnection;26;1;511;0
WireConnection;459;0;174;0
WireConnection;494;0;26;0
WireConnection;458;0;457;0
WireConnection;458;1;459;0
WireConnection;458;2;459;0
WireConnection;458;3;459;0
WireConnection;242;0;494;0
WireConnection;40;0;41;0
WireConnection;40;1;38;0
WireConnection;460;0;458;0
WireConnection;234;0;40;0
WireConnection;452;0;263;0
WireConnection;452;1;451;0
WireConnection;471;0;463;0
WireConnection;471;1;466;0
WireConnection;453;0;262;0
WireConnection;453;1;452;0
WireConnection;593;0;63;4
WireConnection;470;0;471;0
WireConnection;470;1;452;0
WireConnection;572;0;571;0
WireConnection;572;1;547;0
WireConnection;454;0;453;0
WireConnection;598;0;594;0
WireConnection;598;1;466;0
WireConnection;548;0;545;0
WireConnection;472;0;454;0
WireConnection;472;1;470;0
WireConnection;574;0;24;4
WireConnection;574;1;573;0
WireConnection;577;0;572;0
WireConnection;596;0;472;0
WireConnection;596;1;598;0
WireConnection;546;0;548;0
WireConnection;579;3;577;0
WireConnection;568;0;25;0
WireConnection;568;1;24;0
WireConnection;568;2;347;0
WireConnection;568;3;574;0
WireConnection;567;0;568;0
WireConnection;559;0;547;0
WireConnection;559;1;546;0
WireConnection;595;0;472;0
WireConnection;595;1;596;0
WireConnection;595;2;597;0
WireConnection;575;0;579;0
WireConnection;584;0;575;0
WireConnection;584;1;585;0
WireConnection;261;0;262;0
WireConnection;261;1;595;0
WireConnection;261;2;131;0
WireConnection;563;0;559;0
WireConnection;31;2;89;4
WireConnection;370;0;261;0
WireConnection;407;0;405;0
WireConnection;407;1;406;0
WireConnection;569;0;584;0
WireConnection;569;1;563;0
WireConnection;569;2;570;0
WireConnection;220;0;180;3
WireConnection;220;1;185;4
WireConnection;218;0;180;1
WireConnection;218;1;183;4
WireConnection;219;0;180;2
WireConnection;219;1;184;4
WireConnection;208;0;207;0
WireConnection;200;0;218;0
WireConnection;200;1;219;0
WireConnection;200;2;220;0
WireConnection;404;0;370;0
WireConnection;404;1;407;0
WireConnection;368;0;89;4
WireConnection;368;1;31;0
WireConnection;560;0;569;0
WireConnection;201;0;200;0
WireConnection;369;0;368;0
WireConnection;476;0;473;3
WireConnection;476;1;180;3
WireConnection;474;0;473;1
WireConnection;474;1;180;1
WireConnection;84;0;63;4
WireConnection;84;1;83;0
WireConnection;413;0;404;0
WireConnection;413;1;370;0
WireConnection;475;0;473;2
WireConnection;475;1;180;2
WireConnection;187;0;208;0
WireConnection;88;0;84;0
WireConnection;313;0;201;0
WireConnection;477;0;474;0
WireConnection;477;1;475;0
WireConnection;477;2;476;0
WireConnection;544;0;530;0
WireConnection;544;1;561;0
WireConnection;74;0;241;0
WireConnection;74;1;89;0
WireConnection;74;2;369;0
WireConnection;186;0;187;0
WireConnection;408;0;413;0
WireConnection;531;0;510;0
WireConnection;531;1;530;0
WireConnection;304;0;74;0
WireConnection;309;0;88;0
WireConnection;478;0;477;0
WireConnection;446;0;408;0
WireConnection;446;1;411;0
WireConnection;564;0;531;0
WireConnection;564;1;544;0
WireConnection;538;0;411;0
WireConnection;538;1;447;0
WireConnection;223;0;186;0
WireConnection;484;0;483;1
WireConnection;484;1;180;1
WireConnection;479;0;478;0
WireConnection;534;0;564;0
WireConnection;227;0;314;0
WireConnection;227;1;226;0
WireConnection;225;0;223;0
WireConnection;448;0;446;0
WireConnection;448;1;538;0
WireConnection;448;2;590;0
WireConnection;485;0;483;2
WireConnection;485;1;180;2
WireConnection;203;0;310;0
WireConnection;203;1;225;0
WireConnection;203;2;227;0
WireConnection;197;0;246;0
WireConnection;197;1;183;0
WireConnection;197;2;484;0
WireConnection;504;0;448;0
WireConnection;486;0;483;3
WireConnection;486;1;180;3
WireConnection;536;0;504;0
WireConnection;536;1;533;0
WireConnection;536;2;537;0
WireConnection;230;0;63;1
WireConnection;230;1;231;0
WireConnection;198;0;197;0
WireConnection;198;1;184;0
WireConnection;198;2;485;0
WireConnection;480;0;203;0
WireConnection;480;1;481;0
WireConnection;355;0;230;0
WireConnection;173;0;253;0
WireConnection;173;1;174;0
WireConnection;173;2;170;1
WireConnection;592;0;536;0
WireConnection;592;1;591;1
WireConnection;482;0;480;0
WireConnection;199;0;198;0
WireConnection;199;1;185;0
WireConnection;199;2;486;0
WireConnection;317;0;482;0
WireConnection;278;0;355;0
WireConnection;254;0;173;0
WireConnection;315;0;199;0
WireConnection;280;0;592;0
WireConnection;105;0;316;0
WireConnection;105;1;255;0
WireConnection;105;2;281;0
WireConnection;105;3;279;0
WireConnection;105;4;318;0
WireConnection;0;13;105;0
ASEEND*/
//CHKSM=D1FFDE9D16FEC9C8739B098D8931AE6A9DDB18EB