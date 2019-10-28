// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "RRF_HumanShaders/Skin Shaders/SimpleSSS/SimpleSSS4"
{
	Properties
	{
		_MainLightTint("MainLightTint", Color) = (1,1,1,0)
		_Albedo_Tint("Albedo_Tint", Color) = (1,1,1,0)
		_Albedo("Albedo", 2D) = "white" {}
		_NormalMap("NormalMap", 2D) = "bump" {}
		_MetalGloss("Metal-Gloss", 2D) = "black" {}
		_SmoothnessDeviate("SmoothnessDeviate", Range( -1 , 1)) = 0
		_SSS_TextureWeightA("SSS_Texture-Weight(A)", 2D) = "white" {}
		_SSS_WeightAmount("SSS_WeightAmount", Range( 0 , 1)) = 0
		_ScatterColor("ScatterColor", Color) = (0,0,0,0)
		_Light_Bias("Light_Bias", Range( 0 , 1)) = 0
		_Light_Scale("Light_Scale", Range( 0 , 10)) = 0
		_LightScatter("LightScatter", Range( 0 , 1)) = 0
		_BackScatterPower("BackScatterPower", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Background+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityCG.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 4.6
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
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
		};

		uniform sampler2D _NormalMap;
		uniform float4 _NormalMap_ST;
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform float4 _Albedo_Tint;
		uniform float4 _MainLightTint;
		uniform float4 _ScatterColor;
		uniform sampler2D _SSS_TextureWeightA;
		uniform float4 _SSS_TextureWeightA_ST;
		uniform float _SSS_WeightAmount;
		uniform float _Light_Bias;
		uniform float _Light_Scale;
		uniform float _LightScatter;
		uniform float _BackScatterPower;
		uniform sampler2D _MetalGloss;
		uniform float4 _MetalGloss_ST;
		uniform float _SmoothnessDeviate;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_NormalMap = i.uv_texcoord * _NormalMap_ST.xy + _NormalMap_ST.zw;
			o.Normal = UnpackNormal( tex2D( _NormalMap, uv_NormalMap ) );
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			float4 temp_output_113_0 = ( tex2D( _Albedo, uv_Albedo ) * _Albedo_Tint );
			float3 ase_worldPos = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_vertexNormal = mul( unity_WorldToObject, float4( ase_worldNormal, 0 ) );
			float dotResult24 = dot( ase_worldlightDir , ase_vertexNormal );
			float clampResult82 = clamp( dotResult24 , 0.0 , 1.0 );
			#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			float clampResult107 = clamp( ase_lightColor.a , 0.0 , 1.0 );
			float2 uv_SSS_TextureWeightA = i.uv_texcoord * _SSS_TextureWeightA_ST.xy + _SSS_TextureWeightA_ST.zw;
			float4 tex2DNode114 = tex2D( _SSS_TextureWeightA, uv_SSS_TextureWeightA );
			float lerpResult133 = lerp( 0.0 , tex2DNode114.a , _SSS_WeightAmount);
			float4 temp_output_115_0 = ( _ScatterColor * tex2DNode114 * lerpResult133 );
			float dotResult136 = dot( ase_vertexNormal , ase_worldlightDir );
			float3 temp_cast_1 = (dotResult136).xxx;
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float3 objToWorldDir30 = mul( unity_ObjectToWorld, float4( ase_vertex3Pos, 0 ) ).xyz;
			float3 normalizeResult32 = normalize( objToWorldDir30 );
			float dotResult37 = dot( temp_cast_1 , normalizeResult32 );
			float clampResult131 = clamp( ( ( dotResult37 + _Light_Bias ) * _Light_Scale ) , 0.0 , 1.0 );
			float4 lerpResult102 = lerp( ( _MainLightTint * clampResult82 * temp_output_113_0 ) , abs( ( temp_output_115_0 * clampResult131 ) ) , _LightScatter);
			o.Albedo = ( temp_output_113_0 + ( float4( ( clampResult82 * ase_lightColor.rgb * clampResult107 ) , 0.0 ) * lerpResult102 ) ).rgb;
			float dotResult64 = dot( ase_worldlightDir , normalizeResult32 );
			float clampResult69 = clamp( dotResult64 , 0.0 , 1.0 );
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float dotResult72 = dot( -ase_worldViewDir , ase_worldlightDir );
			float clampResult73 = clamp( dotResult72 , 0.0 , 1.0 );
			float4 clampResult110 = clamp( ( lerpResult102 + ( temp_output_115_0 * ( ( clampResult69 * clampResult73 ) * _BackScatterPower ) ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
			o.Emission = clampResult110.rgb;
			float2 uv_MetalGloss = i.uv_texcoord * _MetalGloss_ST.xy + _MetalGloss_ST.zw;
			float4 tex2DNode118 = tex2D( _MetalGloss, uv_MetalGloss );
			o.Metallic = tex2DNode118.r;
			o.Smoothness = ( tex2DNode118.a + _SmoothnessDeviate );
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.6
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
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
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
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
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
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
Version=17000
1927;7;1906;1014;3363.4;901.5865;2.311922;True;True
Node;AmplifyShaderEditor.CommentaryNode;56;-1832.857,19.23205;Float;False;1435.461;680.1272;ScatterLight;12;29;30;32;35;40;39;37;44;114;117;133;136;;1,1,1,1;0;0
Node;AmplifyShaderEditor.PosVertexDataNode;29;-1782.857,303.8703;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;35;-1772.715,146.5148;Float;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NormalVertexDataNode;135;-1839.107,-149.7365;Float;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;76;-348.7604,442.9608;Float;False;1116.434;621.9487;BackScatter;9;64;70;71;74;72;69;75;73;87;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TransformDirectionNode;30;-1516.109,325.217;Float;False;Object;World;False;Fast;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;70;-264.73,684.7723;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NormalizeNode;32;-1241.061,337.4789;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DotProductOpNode;136;-1315.748,51.76304;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;71;-298.7604,885.9095;Float;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;39;-1538.345,472.2159;Float;False;Property;_Light_Bias;Light_Bias;10;0;Create;True;0;0;False;0;0;0.156;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;40;-1545.164,584.3593;Float;False;Property;_Light_Scale;Light_Scale;11;0;Create;True;0;0;False;0;0;1.92;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;74;-5.66875,765.5759;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;117;-1105.745,577.0661;Float;False;Property;_SSS_WeightAmount;SSS_WeightAmount;8;0;Create;True;0;0;False;0;0;0.392;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;114;-1099.03,344.3062;Float;True;Property;_SSS_TextureWeightA;SSS_Texture-Weight(A);7;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;57;-1537.432,-617.6573;Float;False;915.4287;532.0931;MainLight;8;22;23;42;24;43;61;82;107;;1,1,1,1;0;0
Node;AmplifyShaderEditor.DotProductOpNode;37;-1006.646,176.1735;Float;False;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;44;-972.6374,10.94351;Float;False;Property;_ScatterColor;ScatterColor;9;0;Create;True;0;0;False;0;0,0,0,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DotProductOpNode;72;214.1929,816.2384;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;133;-712.0639,506.3657;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;22;-1487.432,-427.36;Float;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NormalVertexDataNode;23;-1457.624,-264.5642;Float;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DotProductOpNode;64;-201.1009,496.7227;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;38;-818.4666,755.3414;Float;False;ConstantBiasScale;-1;;8;63208df05c83e8e49a48ffbdce2e43a0;0;3;3;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;69;97.08311,516.852;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;131;-473.285,693.9008;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;112;-1507.575,-1047.724;Float;True;Property;_Albedo;Albedo;3;0;Create;True;0;0;False;0;None;ac4faf4063f480b46af72cd620d04440;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DotProductOpNode;24;-1079.135,-380.4155;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;115;-528.6672,247.3148;Float;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;73;434.0124,816.8043;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;58;-293.5292,-316.7261;Float;False;722.2638;725.0239;FuzzMix;4;45;128;130;33;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;51;-1471.425,-841.6143;Float;False;Property;_Albedo_Tint;Albedo_Tint;2;0;Create;True;0;0;False;0;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;75;598.6734,627.3169;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;82;-900.0018,-377.0577;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;87;345.288,969.9673;Float;False;Property;_BackScatterPower;BackScatterPower;13;0;Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;113;-1096.581,-893.8199;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;45;-223.9199,184.3272;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;42;-1142.132,-567.6573;Float;False;Property;_MainLightTint;MainLightTint;0;0;Create;True;0;0;False;0;1,1,1,0;0.3897059,0.29722,0.2865484,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LightColorNode;61;-1175.316,-228.1119;Float;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;33;159.0275,301.7422;Float;False;Property;_LightScatter;LightScatter;12;0;Create;True;0;0;False;0;0;0.775;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;130;201.4902,208.0211;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;86;903.1274,658.6421;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;107;-795.5683,-123.053;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;43;-680.5528,-435.8714;Float;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;62;264.8788,-424.4785;Float;False;3;3;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;102;558.9145,119.4884;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;108;1132.269,414.7261;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;109;1337.204,121.2112;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;118;1547.486,-258.6144;Float;True;Property;_MetalGloss;Metal-Gloss;5;0;Create;True;0;0;False;0;None;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;59;1566.091,-49.84906;Float;False;Property;_SmoothnessDeviate;SmoothnessDeviate;6;0;Create;True;0;0;False;0;0;-0.03;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;106;771.8459,17.08136;Float;False;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;119;2055.61,-115.5921;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;111;1130.728,-389.6214;Float;True;Property;_NormalMap;NormalMap;4;0;Create;True;0;0;False;0;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;128;-103.5498,-18.56374;Float;False;Constant;_Color0;Color 0;14;0;Create;True;0;0;False;0;0.5808823,0.5808823,0.5808823,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;120;1368.783,-606.5608;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;110;1569.712,46.90427;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;21;2877.527,-243.5165;Float;False;True;6;Float;ASEMaterialInspector;0;0;Standard;RRF_HumanShaders/Skin Shaders/SimpleSSS/SimpleSSS4;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;Opaque;;Background;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;20.4;10;25;False;0.5;True;0;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;30;0;29;0
WireConnection;32;0;30;0
WireConnection;136;0;135;0
WireConnection;136;1;35;0
WireConnection;74;0;70;0
WireConnection;37;0;136;0
WireConnection;37;1;32;0
WireConnection;72;0;74;0
WireConnection;72;1;71;0
WireConnection;133;1;114;4
WireConnection;133;2;117;0
WireConnection;64;0;35;0
WireConnection;64;1;32;0
WireConnection;38;3;37;0
WireConnection;38;1;39;0
WireConnection;38;2;40;0
WireConnection;69;0;64;0
WireConnection;131;0;38;0
WireConnection;24;0;22;0
WireConnection;24;1;23;0
WireConnection;115;0;44;0
WireConnection;115;1;114;0
WireConnection;115;2;133;0
WireConnection;73;0;72;0
WireConnection;75;0;69;0
WireConnection;75;1;73;0
WireConnection;82;0;24;0
WireConnection;113;0;112;0
WireConnection;113;1;51;0
WireConnection;45;0;115;0
WireConnection;45;1;131;0
WireConnection;130;0;45;0
WireConnection;86;0;75;0
WireConnection;86;1;87;0
WireConnection;107;0;61;2
WireConnection;43;0;42;0
WireConnection;43;1;82;0
WireConnection;43;2;113;0
WireConnection;62;0;82;0
WireConnection;62;1;61;1
WireConnection;62;2;107;0
WireConnection;102;0;43;0
WireConnection;102;1;130;0
WireConnection;102;2;33;0
WireConnection;108;0;115;0
WireConnection;108;1;86;0
WireConnection;109;0;102;0
WireConnection;109;1;108;0
WireConnection;106;0;62;0
WireConnection;106;1;102;0
WireConnection;119;0;118;4
WireConnection;119;1;59;0
WireConnection;120;0;113;0
WireConnection;120;1;106;0
WireConnection;110;0;109;0
WireConnection;21;0;120;0
WireConnection;21;1;111;0
WireConnection;21;2;110;0
WireConnection;21;3;118;0
WireConnection;21;4;119;0
ASEEND*/
//CHKSM=D471A15688D6A04A8037C28991FFC48EBFE8485B