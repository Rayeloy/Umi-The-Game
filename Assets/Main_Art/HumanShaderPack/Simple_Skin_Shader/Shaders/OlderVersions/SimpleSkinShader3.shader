// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "RRF_HumanShaders/Skin Shaders/RnD/SimpleSkinShader3"
{
	Properties
	{
		_BaseSSSGlow("BaseSSSGlow", Range( 0 , 1)) = 0
		_Shininess("Shininess", Range( 0.01 , 1)) = 0.1
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
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TreeOpaque"  "Queue" = "Geometry+0" }
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
			float3 worldPos;
			float2 uv_texcoord;
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

		uniform float _DirectScatter;
		uniform float _ScatterAdd;
		uniform sampler2D _Curvature;
		uniform float4 _Curvature_ST;
		uniform float _Curvature_Power;
		uniform float4 _SSS_BaseColorWeightA;
		uniform sampler2D _SSS_ColorWeightA;
		uniform float4 _SSS_ColorWeightA_ST;
		uniform float4 _AlbedoColor;
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform float4 _FuzzColor;
		uniform float _Fuzz;
		uniform sampler2D _MetalGloss;
		uniform float4 _MetalGloss_ST;
		uniform float _SmoothnessAdjust;
		uniform float _NormalBumpScale;
		uniform sampler2D _NormalMap;
		uniform float4 _NormalMap_ST;
		uniform float _Shininess;
		uniform float _BaseSSSGlow;
		uniform float _BackScatter;
		uniform float _ShadowPower;

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
			float3 ase_worldPos = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float dotResult131 = dot( ase_worldlightDir , ase_vertex3Pos );
			float clampResult57 = clamp( ( ( ( dotResult131 + 0.1 ) * 2.0 ) + _ScatterAdd ) , 0.0 , 1.0 );
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
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			float4 clampResult49 = clamp( ( ( _DirectScatter * clampResult57 * clampResult62 * temp_output_26_0 ) + ( clampResult104 * ( _AlbedoColor * tex2D( _Albedo, uv_Albedo ) ) ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float fresnelNdotV31 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode31 = ( 0.0 + _Fuzz * pow( 1.0 - fresnelNdotV31, 3.0 ) );
			float4 lerpResult74 = lerp( clampResult49 , _FuzzColor , fresnelNode31);
			float2 uv_MetalGloss = i.uv_texcoord * _MetalGloss_ST.xy + _MetalGloss_ST.zw;
			float4 tex2DNode63 = tex2D( _MetalGloss, uv_MetalGloss );
			float lerpResult73 = lerp( tex2DNode63.a , 0.0 , fresnelNode31);
			float clampResult88 = clamp( ( lerpResult73 + _SmoothnessAdjust ) , 0.0 , 1.0 );
			float4 temp_cast_1 = (clampResult88).xxxx;
			float4 temp_output_43_0_g8 = temp_cast_1;
			float3 normalizeResult4_g9 = normalize( ( ase_worldViewDir + ase_worldlightDir ) );
			float2 uv_NormalMap = i.uv_texcoord * _NormalMap_ST.xy + _NormalMap_ST.zw;
			float3 tex2DNode45 = UnpackScaleNormal( tex2D( _NormalMap, uv_NormalMap ), _NormalBumpScale );
			float3 normalizeResult64_g8 = normalize( (WorldNormalVector( i , tex2DNode45 )) );
			float dotResult19_g8 = dot( normalizeResult4_g9 , normalizeResult64_g8 );
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			float3 temp_output_40_0_g8 = ( ase_lightColor.rgb * ase_lightAtten );
			float dotResult14_g8 = dot( normalizeResult64_g8 , ase_worldlightDir );
			UnityGI gi34_g8 = gi;
			float3 diffNorm34_g8 = normalizeResult64_g8;
			gi34_g8 = UnityGI_Base( data, 1, diffNorm34_g8 );
			float3 indirectDiffuse34_g8 = gi34_g8.indirect.diffuse + diffNorm34_g8 * 0.0001;
			float4 temp_output_42_0_g8 = lerpResult74;
			s105.Albedo = ( lerpResult74 + float4( ( ( (temp_output_43_0_g8).rgb * (temp_output_43_0_g8).a * pow( max( dotResult19_g8 , 0.0 ) , ( _Shininess * 128.0 ) ) * temp_output_40_0_g8 ) + ( ( ( temp_output_40_0_g8 * max( dotResult14_g8 , 0.0 ) ) + indirectDiffuse34_g8 ) * (temp_output_42_0_g8).rgb ) ) , 0.0 ) ).rgb;
			s105.Normal = WorldNormalVector( i , tex2DNode45 );
			float temp_output_2_0_g7 = _ShadowPower;
			float temp_output_3_0_g7 = ( 1.0 - temp_output_2_0_g7 );
			float3 appendResult7_g7 = (float3(temp_output_3_0_g7 , temp_output_3_0_g7 , temp_output_3_0_g7));
			float3 temp_output_126_0 = ( ( ( clampResult62 + ( ase_lightAtten * ase_lightColor ) ).rgb * temp_output_2_0_g7 ) + appendResult7_g7 );
			float3 Scatter128 = temp_output_126_0;
			float4 clampResult56 = clamp( ( temp_output_26_0 * ( _SSS_BaseColorWeightA.a * tex2DNode24.a ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
			float4 blendOpSrc82 = ( ( _BaseSSSGlow * tex2DNode24 ) + ( ( float4( ( ( _BackScatter * ( 1.0 - clampResult104 ) ) * Scatter128 * Scatter128 * Scatter128 ) , 0.0 ) * clampResult56 ) * ase_lightColor.a ) );
			float4 blendOpDest82 = clampResult62;
			s105.Emission = ( ( saturate( ( 1.0 - ( 1.0 - blendOpSrc82 ) * ( 1.0 - blendOpDest82 ) ) )) * float4( temp_output_126_0 , 0.0 ) ).rgb;
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
21;253;1272;773;2757.801;1637.818;4.078541;True;False
Node;AmplifyShaderEditor.CommentaryNode;78;-918.687,-1657.276;Float;False;889.334;398.333;Curvature;5;61;7;60;79;62;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;61;-906.7864,-1396.689;Float;False;Property;_Curvature_Power;Curvature_Power;18;0;Create;True;0;0;False;0;1;0.9;0.1;4;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;7;-886.368,-1605.669;Float;True;Property;_Curvature;Curvature;17;0;Create;True;0;0;False;0;None;36bdb461d755436469d897ba5bdcd44f;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;76;-965.8088,-828.8407;Float;False;597.4731;458.7154;Albedo;4;38;40;41;92;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;77;-938.4332,-1232.552;Float;False;1108.382;385.6998;SimpleSSS;7;2;28;29;30;68;3;69;;1,1,1,1;0;0
Node;AmplifyShaderEditor.PowerNode;60;-568.4681,-1613.79;Float;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCRemapNode;80;-571.4392,-1408.739;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;4;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;2;-893.6961,-990.7667;Float;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.LightAttenuation;106;1356.436,-51.15677;Float;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;109;1357.599,69.949;Float;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;92;-589.1568,-559.2996;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.LerpOp;79;-327.5943,-1514.224;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;62;-172.4582,-1513.124;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;108;1751.63,-77.28663;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;130;-1798.451,-1075.983;Float;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.PosVertexDataNode;133;-1792.505,-909.4947;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DotProductOpNode;93;-349.9818,-788.248;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;75;-941.9908,-337.8607;Float;False;1004.829;503.8087;Main SSS Map;5;24;25;27;26;23;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;122;2032.605,-108.7456;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DotProductOpNode;131;-1441.688,-1042.289;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;123;1891.947,26.35633;Float;False;Property;_ShadowPower;ShadowPower;8;0;Create;True;0;0;False;0;0.6;0.825;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;102;-195.0309,-767.8531;Float;False;ConstantBiasScale;-1;;2;63208df05c83e8e49a48ffbdce2e43a0;0;3;3;FLOAT;0;False;1;FLOAT;0.98;False;2;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;126;2273.553,-221.1221;Float;False;Lerp White To;-1;;7;047d7c189c36a62438973bad9d37b1c2;0;2;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;85;39.89126,-1806.763;Float;False;1649.776;553.4731;Control;8;71;42;54;55;56;53;127;129;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;24;-887.7233,-64.05204;Float;True;Property;_SSS_ColorWeightA;SSS_Color-Weight(A);12;0;Create;True;0;0;False;0;None;8b892976315977d4f86daf1e9423b540;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;68;-597.0049,-976.291;Float;False;Property;_ScatterAdd;ScatterAdd;7;0;Create;True;0;0;False;0;0;0.726;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;134;-1255.379,-1075.983;Float;False;ConstantBiasScale;-1;;6;63208df05c83e8e49a48ffbdce2e43a0;0;3;3;FLOAT;0;False;1;FLOAT;0.1;False;2;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;25;-891.9908,-287.8607;Float;False;Property;_SSS_BaseColorWeightA;SSS_BaseColor-Weight(A);11;0;Create;True;0;0;False;0;0.8970588,0.5474697,0.5474697,1;1,0.3897059,0.3897059,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;104;103.4699,-800.4407;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;128;2578.492,-229.501;Float;False;Scatter;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode;94;249.5262,-874.0358;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;-423.3605,-262.4104;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;38;-915.8088,-600.1253;Float;True;Property;_Albedo;Albedo;13;0;Create;True;0;0;False;0;None;0bc27981a97cc964b84ad58acc049416;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;86;772.435,-443.9589;Float;False;674.7306;340.6641;Fuzz;3;89;31;33;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;53;96.26378,-1756.763;Float;False;Property;_BackScatter;BackScatter;6;0;Create;True;0;0;False;0;0;2.22;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;69;15.94911,-1059.531;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;-422.4095,-9.845176;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;41;-910.8441,-778.8407;Float;False;Property;_AlbedoColor;AlbedoColor;14;0;Create;True;0;0;False;0;1,1,1,0;0.2573529,0.2573529,0.2573529,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;57;210.213,-1064.721;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;33;814.0356,-369.9691;Float;False;Property;_Fuzz;Fuzz;9;0;Create;True;0;0;False;0;0;2.101;0;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;42;89.89128,-1645.714;Float;False;Property;_DirectScatter;DirectScatter;5;0;Create;True;0;0;False;0;0;2.29;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;-106.1614,-135.8701;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;129;648.6948,-1575.3;Float;False;128;Scatter;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;54;545.054,-1728.234;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;87;764.8707,-814.0339;Float;False;1021.492;310.8159;Normal;3;45;46;63;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;-574.5131,-699.2377;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;56;855.6223,-1409.29;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;127;850.1414,-1665.033;Float;False;4;4;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;63;1502.783,-623.1344;Float;True;Property;_MetalGloss;Metal-Gloss;19;0;Create;True;0;0;False;0;None;a049867252a39ad4185eaca07e577617;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FresnelNode;31;1145.918,-393.9589;Float;False;Standard;WorldNormal;ViewDir;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;3;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;95;241.0678,-669.2186;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;43;597.1438,-1226.49;Float;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;71;1387.667,-1433.63;Float;False;Property;_BaseSSSGlow;BaseSSSGlow;0;0;Create;True;0;0;False;0;0;0.087;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;73;1823.824,-523.4587;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;44;976.8253,-1027.785;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;83;1896.256,-393.9281;Float;False;Property;_SmoothnessAdjust;SmoothnessAdjust;21;0;Create;True;0;0;False;0;0;-0.232;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;58;1129.132,-1138.361;Float;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;1112.855,-1620.776;Float;False;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;72;1799.137,-1221.052;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;84;2278.015,-471.472;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;59;1517.202,-1218.469;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;49;1393.87,-929.8268;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;89;822.6454,-285.4754;Float;False;Property;_FuzzColor;FuzzColor;10;0;Create;True;0;0;False;0;0.5147059,0.5071367,0.5071367,0;0.7426471,0.5406034,0.5406034,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;46;814.8707,-721.9401;Float;False;Property;_NormalBumpScale;NormalBumpScale;16;0;Create;True;0;0;False;0;1;1;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;74;1719.877,-901.6283;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0.8014706,0.7174218,0.6718209,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;45;1133.145,-764.0339;Float;True;Property;_NormalMap;NormalMap;15;0;Create;True;0;0;False;0;None;e2479a6af2d8c4b4b8479c74b02a5982;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;70;2064.509,-1140.559;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;88;2414.839,-496.7037;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;120;1920.052,-738.7758;Float;False;Blinn-Phong Light;1;;8;cf814dba44d007a4e958d2ddd5813da6;0;3;42;COLOR;0,0,0,0;False;52;FLOAT3;0,0,0;False;43;COLOR;0,0,0,0;False;2;FLOAT3;0;FLOAT;57
Node;AmplifyShaderEditor.BlendOpsNode;82;2295.384,-1002.187;Float;False;Screen;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;121;2193.679,-766.2207;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;110;2548.132,-895.5336;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.NormalizeNode;30;-325.0698,-1169.971;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TransformDirectionNode;29;-607.2095,-1182.552;Float;False;Object;World;False;Fast;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DotProductOpNode;3;-119.7678,-1146.372;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CustomStandardSurface;105;2704.666,-720.1138;Float;False;Metallic;Tangent;6;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,1;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PosVertexDataNode;28;-888.4332,-1179.816;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;3002.482,-679.3383;Float;False;True;2;Float;ASEMaterialInspector;0;0;CustomLighting;RRF_HumanShaders/Skin Shaders/RnD/SimpleSkinShader3;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;TreeOpaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;20;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;60;0;7;0
WireConnection;60;1;61;0
WireConnection;80;0;61;0
WireConnection;79;1;60;0
WireConnection;79;2;80;0
WireConnection;62;0;79;0
WireConnection;108;0;106;0
WireConnection;108;1;109;0
WireConnection;93;0;2;0
WireConnection;93;1;92;0
WireConnection;122;0;62;0
WireConnection;122;1;108;0
WireConnection;131;0;130;0
WireConnection;131;1;133;0
WireConnection;102;3;93;0
WireConnection;126;1;122;0
WireConnection;126;2;123;0
WireConnection;134;3;131;0
WireConnection;104;0;102;0
WireConnection;128;0;126;0
WireConnection;94;0;104;0
WireConnection;26;0;25;0
WireConnection;26;1;24;0
WireConnection;69;0;134;0
WireConnection;69;1;68;0
WireConnection;27;0;25;4
WireConnection;27;1;24;4
WireConnection;57;0;69;0
WireConnection;23;0;26;0
WireConnection;23;1;27;0
WireConnection;54;0;53;0
WireConnection;54;1;94;0
WireConnection;40;0;41;0
WireConnection;40;1;38;0
WireConnection;56;0;23;0
WireConnection;127;0;54;0
WireConnection;127;1;129;0
WireConnection;127;2;129;0
WireConnection;127;3;129;0
WireConnection;31;2;33;0
WireConnection;95;0;104;0
WireConnection;95;1;40;0
WireConnection;43;0;42;0
WireConnection;43;1;57;0
WireConnection;43;2;62;0
WireConnection;43;3;26;0
WireConnection;73;0;63;4
WireConnection;73;2;31;0
WireConnection;44;0;43;0
WireConnection;44;1;95;0
WireConnection;55;0;127;0
WireConnection;55;1;56;0
WireConnection;72;0;71;0
WireConnection;72;1;24;0
WireConnection;84;0;73;0
WireConnection;84;1;83;0
WireConnection;59;0;55;0
WireConnection;59;1;58;2
WireConnection;49;0;44;0
WireConnection;74;0;49;0
WireConnection;74;1;89;0
WireConnection;74;2;31;0
WireConnection;45;5;46;0
WireConnection;70;0;72;0
WireConnection;70;1;59;0
WireConnection;88;0;84;0
WireConnection;120;42;74;0
WireConnection;120;52;45;0
WireConnection;120;43;88;0
WireConnection;82;0;70;0
WireConnection;82;1;62;0
WireConnection;121;0;74;0
WireConnection;121;1;120;0
WireConnection;110;0;82;0
WireConnection;110;1;126;0
WireConnection;30;0;29;0
WireConnection;29;0;28;0
WireConnection;3;0;30;0
WireConnection;3;1;2;0
WireConnection;105;0;121;0
WireConnection;105;1;45;0
WireConnection;105;2;110;0
WireConnection;105;3;63;0
WireConnection;105;4;88;0
WireConnection;0;13;105;0
ASEEND*/
//CHKSM=1E483BD75EC6BC510575B099C48543B39FF664B1