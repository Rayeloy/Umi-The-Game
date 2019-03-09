// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SimpleCandleShader"
{
	Properties
	{
		_ScatterAdd("ScatterAdd", Range( 0 , 2)) = 0
		_SSS_ColorWeightA("SSS_Color-Weight(A)", 2D) = "white" {}
		_Albedo("Albedo", 2D) = "white" {}
		_AlbedoColor("AlbedoColor", Color) = (1,1,1,0)
		_NormalMap("NormalMap", 2D) = "bump" {}
		_NormalBumpScale("NormalBumpScale", Range( 0 , 2)) = 1
		_MetalGloss("Metal-Gloss", 2D) = "white" {}
		_SmoothnessAdjust("SmoothnessAdjust", Range( -1 , 1)) = 0
		_CandleLightColorPower("CandleLightColor-Power", Color) = (1,0.8896551,0,0.516)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TreeOpaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "UnityStandardUtils.cginc"
		#include "UnityShaderVariables.cginc"
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
		uniform float _NormalBumpScale;
		uniform sampler2D _NormalMap;
		uniform float4 _NormalMap_ST;
		uniform float _ScatterAdd;
		uniform sampler2D _SSS_ColorWeightA;
		uniform float4 _SSS_ColorWeightA_ST;
		uniform float4 _CandleLightColorPower;
		uniform sampler2D _MetalGloss;
		uniform float4 _MetalGloss_ST;
		uniform float _SmoothnessAdjust;

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			SurfaceOutputStandard s95 = (SurfaceOutputStandard ) 0;
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			float4 temp_output_40_0 = ( _AlbedoColor * tex2D( _Albedo, uv_Albedo ) );
			s95.Albedo = temp_output_40_0.rgb;
			float2 uv_NormalMap = i.uv_texcoord * _NormalMap_ST.xy + _NormalMap_ST.zw;
			s95.Normal = WorldNormalVector( i , UnpackScaleNormal( tex2D( _NormalMap, uv_NormalMap ), _NormalBumpScale ) );
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float3 objToWorldDir29 = mul( unity_ObjectToWorld, float4( ase_vertex3Pos, 0 ) ).xyz;
			float3 normalizeResult30 = normalize( objToWorldDir29 );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			UnityGI gi93 = gi;
			float3 diffNorm93 = ase_worldNormal;
			gi93 = UnityGI_Base( data, 1, diffNorm93 );
			float3 indirectDiffuse93 = gi93.indirect.diffuse + diffNorm93 * 0.0001;
			float dotResult3 = dot( normalizeResult30 , indirectDiffuse93 );
			float temp_output_69_0 = ( dotResult3 + _ScatterAdd );
			float2 uv_SSS_ColorWeightA = i.uv_texcoord * _SSS_ColorWeightA_ST.xy + _SSS_ColorWeightA_ST.zw;
			float clampResult102 = clamp( distance( indirectDiffuse93 , ase_vertex3Pos ) , 0.0 , 1.0 );
			float temp_output_117_0 = ( 1.0 - _CandleLightColorPower.a );
			float4 temp_cast_1 = (temp_output_117_0).xxxx;
			s95.Emission = ( pow( ( temp_output_69_0 * tex2D( _SSS_ColorWeightA, uv_SSS_ColorWeightA ) * clampResult102 ) , temp_cast_1 ) * _CandleLightColorPower ).rgb;
			float2 uv_MetalGloss = i.uv_texcoord * _MetalGloss_ST.xy + _MetalGloss_ST.zw;
			float4 tex2DNode63 = tex2D( _MetalGloss, uv_MetalGloss );
			s95.Metallic = tex2DNode63.r;
			float clampResult88 = clamp( ( tex2DNode63.a + _SmoothnessAdjust ) , 0.0 , 1.0 );
			s95.Smoothness = clampResult88;
			s95.Occlusion = 1.0;

			data.light = gi.light;

			UnityGI gi95 = gi;
			#ifdef UNITY_PASS_FORWARDBASE
			Unity_GlossyEnvironmentData g95 = UnityGlossyEnvironmentSetup( s95.Smoothness, data.worldViewDir, s95.Normal, float3(0,0,0));
			gi95 = UnityGlobalIllumination( data, s95.Occlusion, s95.Normal, g95 );
			#endif

			float3 surfResult95 = LightingStandard ( s95, viewDir, gi95 ).rgb;
			surfResult95 += s95.Emission;

			c.rgb = surfResult95;
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
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16100
779;734;1359;973;-478.8142;973.6594;2.317247;True;False
Node;AmplifyShaderEditor.CommentaryNode;77;-938.4332,-1232.552;Float;False;1108.382;385.6998;SimpleSSS;8;2;28;29;30;68;3;69;93;;1,1,1,1;0;0
Node;AmplifyShaderEditor.PosVertexDataNode;28;-888.4332,-1179.816;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TransformDirectionNode;29;-607.2095,-1182.552;Float;False;Object;World;False;Fast;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NormalizeNode;30;-325.0698,-1169.971;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.IndirectDiffuseLighting;93;-387.1292,-1056.978;Float;False;World;1;0;FLOAT3;0,0,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PosVertexDataNode;100;825.3483,209.4254;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DotProductOpNode;3;-119.7678,-1146.372;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;68;-597.0049,-976.291;Float;False;Property;_ScatterAdd;ScatterAdd;3;0;Create;True;0;0;False;0;0;1.09;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;99;1069.037,35.257;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;111;1728.512,29.24258;Float;False;Property;_CandleLightColorPower;CandleLightColor-Power;17;0;Create;True;0;0;False;0;1,0.8896551,0,0.516;1,0.8896551,0,0.516;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;87;764.8707,-814.0339;Float;False;1021.492;310.8159;Normal;3;45;46;63;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;69;15.94911,-1059.531;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;24;1456.85,-258.5871;Float;True;Property;_SSS_ColorWeightA;SSS_Color-Weight(A);7;0;Create;True;0;0;False;0;None;7d7538cd14e17ae4d97e9cd8fb9b3741;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;102;1490.031,-3.513114;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;117;2074.043,85.54639;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;76;-965.8088,-828.8407;Float;False;597.4731;458.7154;Albedo;3;38;40;41;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;83;1587.071,-460.6741;Float;False;Property;_SmoothnessAdjust;SmoothnessAdjust;16;0;Create;True;0;0;False;0;0;-0.94;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;63;1455.362,-733.218;Float;True;Property;_MetalGloss;Metal-Gloss;14;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;94;1999.778,-230.9566;Float;False;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;46;814.8707,-721.9401;Float;False;Property;_NormalBumpScale;NormalBumpScale;11;0;Create;True;0;0;False;0;1;0.7;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;41;-910.8441,-778.8407;Float;False;Property;_AlbedoColor;AlbedoColor;9;0;Create;True;0;0;False;0;1,1,1,0;0.9044118,0.832866,0.7049092,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;38;-915.8088,-600.1253;Float;True;Property;_Albedo;Albedo;8;0;Create;True;0;0;False;0;None;7eb2903236958284198a2cfac5ce608e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;115;2213.835,-183.0448;Float;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;114;1997.221,-499.7608;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;85;39.89126,-1806.763;Float;False;1649.776;553.4731;Control;6;71;42;54;55;56;53;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ClampOpNode;88;2291.234,-538.729;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;-537.3356,-699.2377;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;45;1116.772,-770.1738;Float;True;Property;_NormalMap;NormalMap;10;0;Create;True;0;0;False;0;None;36f7ba881340a024b8473f7c24599bf0;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;112;2485.33,-227.1226;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;78;-918.687,-1657.276;Float;False;889.334;398.333;Curvature;5;61;7;60;79;62;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;86;772.435,-443.9589;Float;False;636.4833;252;Fuzz;2;33;31;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;75;-941.9908,-337.8607;Float;False;1004.829;503.8087;Main SSS Map;4;25;27;26;23;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;118;2174.57,192.3546;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;42;89.89128,-1645.714;Float;False;Property;_DirectScatter;DirectScatter;1;0;Create;True;0;0;False;0;0;2.32;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;80;-571.4392,-1408.739;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;4;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;60;-568.4681,-1613.79;Float;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;33;814.0356,-369.9691;Float;False;Property;_Fuzz;Fuzz;4;0;Create;True;0;0;False;0;0;1.48;0;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;89;822.6454,-285.4754;Float;False;Property;_FuzzColor;FuzzColor;5;0;Create;True;0;0;False;0;0.5147059,0.5071367,0.5071367,0;0.5147059,0.5071367,0.5071367,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;-423.3605,-262.4104;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;-106.1614,-135.8701;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;59;1517.202,-1218.469;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;44;976.8253,-1027.785;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;49;1393.87,-929.8268;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;53;96.26378,-1756.763;Float;False;Property;_BackScatter;BackScatter;2;0;Create;True;0;0;False;0;0;1.4;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;79;-327.5943,-1514.224;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;74;1843.045,-882.6793;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0.8014706,0.7174218,0.6718209,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;7;-886.368,-1605.669;Float;True;Property;_Curvature;Curvature;12;0;Create;True;0;0;False;0;None;dda7f4ba96ec36945a2df867628a104b;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;2;-881.5512,-1025.852;Float;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;70;2064.509,-1140.559;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;56;855.6223,-1409.29;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;54;545.054,-1728.234;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;1112.855,-1620.776;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.BlendOpsNode;82;2295.384,-1002.187;Float;False;Screen;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;110;2341.991,374.0472;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;58;1129.132,-1138.361;Float;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;72;1799.137,-1221.052;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;61;-906.7864,-1396.689;Float;False;Property;_Curvature_Power;Curvature_Power;13;0;Create;True;0;0;False;0;1;2.47;0.1;4;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;71;1387.667,-1433.63;Float;False;Property;_BaseSSSGlow;BaseSSSGlow;0;0;Create;True;0;0;False;0;0;0.446;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;25;-891.9908,-287.8607;Float;False;Property;_SSS_BaseColorWeightA;SSS_BaseColor-Weight(A);6;0;Create;True;0;0;False;0;0.8970588,0.5474697,0.5474697,1;0.8529412,0.8377162,0.7525951,0.628;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;43;597.1438,-1226.49;Float;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FresnelNode;31;1145.918,-393.9589;Float;False;Standard;WorldNormal;ViewDir;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;3;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;57;210.213,-1064.721;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CustomStandardSurface;95;2718.4,-400.7144;Float;False;Metallic;Tangent;6;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,1;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;-422.4095,-9.845176;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;96;2400.412,643.2797;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;62;-172.4582,-1513.124;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;3083.031,-633.2825;Float;False;True;2;Float;ASEMaterialInspector;0;0;CustomLighting;SimpleCandleShader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;TreeOpaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;15;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;29;0;28;0
WireConnection;30;0;29;0
WireConnection;3;0;30;0
WireConnection;3;1;93;0
WireConnection;99;0;93;0
WireConnection;99;1;100;0
WireConnection;69;0;3;0
WireConnection;69;1;68;0
WireConnection;102;0;99;0
WireConnection;117;0;111;4
WireConnection;94;0;69;0
WireConnection;94;1;24;0
WireConnection;94;2;102;0
WireConnection;115;0;94;0
WireConnection;115;1;117;0
WireConnection;114;0;63;4
WireConnection;114;1;83;0
WireConnection;88;0;114;0
WireConnection;40;0;41;0
WireConnection;40;1;38;0
WireConnection;45;5;46;0
WireConnection;112;0;115;0
WireConnection;112;1;111;0
WireConnection;118;0;117;0
WireConnection;80;0;61;0
WireConnection;60;0;7;0
WireConnection;60;1;61;0
WireConnection;26;0;25;0
WireConnection;23;0;26;0
WireConnection;23;1;27;0
WireConnection;59;0;55;0
WireConnection;59;1;58;2
WireConnection;44;0;43;0
WireConnection;44;1;40;0
WireConnection;49;0;44;0
WireConnection;79;1;60;0
WireConnection;79;2;80;0
WireConnection;74;0;49;0
WireConnection;74;1;89;0
WireConnection;74;2;31;0
WireConnection;70;0;72;0
WireConnection;70;1;59;0
WireConnection;56;0;23;0
WireConnection;54;0;53;0
WireConnection;54;1;57;0
WireConnection;55;0;54;0
WireConnection;55;1;56;0
WireConnection;82;0;70;0
WireConnection;82;1;62;0
WireConnection;72;0;71;0
WireConnection;43;0;42;0
WireConnection;43;1;57;0
WireConnection;43;2;62;0
WireConnection;43;3;26;0
WireConnection;31;2;33;0
WireConnection;57;0;69;0
WireConnection;95;0;40;0
WireConnection;95;1;45;0
WireConnection;95;2;112;0
WireConnection;95;3;63;0
WireConnection;95;4;88;0
WireConnection;27;0;25;4
WireConnection;62;0;79;0
WireConnection;0;13;95;0
ASEEND*/
//CHKSM=1A4C47E06CC2E333475A7786BEC238676E3AD9B5