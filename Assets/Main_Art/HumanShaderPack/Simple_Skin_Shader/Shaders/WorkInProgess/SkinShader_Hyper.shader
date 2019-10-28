// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SkinShader_Hyper"
{
	Properties
	{
		_LightAddMultiplyMix("LightAddMultiplyMix", Range( 0 , 1)) = 0.5
		_NormalMap("NormalMap", 2D) = "bump" {}
		_PorosityMap("PorosityMap", 2D) = "white" {}
		_Albedo("Albedo", 2D) = "white" {}
		_AlbedoColor("AlbedoColor", Color) = (0,0,0,0)
		_Reflectivity("Reflectivity", Float) = 0
		_NormalMapPower("NormalMapPower", Range( 0 , 2)) = 1
		_PorosityScale("PorosityScale", Range( 0 , 2)) = 0
		_PorosityBias("PorosityBias", Range( 0 , 2)) = 1
		_MixeldLightAlbedo("MixeldLightAlbedo", Range( 0 , 1)) = 0
		_Albedo_Light_Mix("Albedo_Light_Mix", Range( 0 , 1)) = 0.5
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
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
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
		uniform float _LightAddMultiplyMix;
		uniform float _MixeldLightAlbedo;
		uniform float _NormalMapPower;
		uniform sampler2D _NormalMap;
		uniform float4 _NormalMap_ST;
		uniform float _Reflectivity;
		uniform sampler2D _PorosityMap;
		uniform float4 _PorosityMap_ST;
		uniform float _PorosityBias;
		uniform float _PorosityScale;
		uniform float _Albedo_Light_Mix;

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
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			float4 AlbedoTint81 = ( _AlbedoColor * tex2D( _Albedo, uv_Albedo ) );
			float3 ase_worldPos = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_vertexNormal = mul( unity_WorldToObject, float4( ase_worldNormal, 0 ) );
			float dotResult54 = dot( ase_worldlightDir , ase_vertexNormal );
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			float4 LightFalloff55 = ( dotResult54 * ase_lightColor );
			float4 LightAttenuation69 = ( ase_lightAtten * ase_lightColor );
			float4 lerpResult73 = lerp( ( LightFalloff55 + LightAttenuation69 ) , ( LightFalloff55 * LightAttenuation69 ) , _LightAddMultiplyMix);
			float4 MixedLight75 = lerpResult73;
			float4 lerpResult109 = lerp( MixedLight75 , AlbedoTint81 , _MixeldLightAlbedo);
			float2 uv_NormalMap = i.uv_texcoord * _NormalMap_ST.xy + _NormalMap_ST.zw;
			float3 MainNormalMap95 = UnpackScaleNormal( tex2D( _NormalMap, uv_NormalMap ), _NormalMapPower );
			float3 indirectNormal84 = WorldNormalVector( i , MainNormalMap95 );
			float2 uv_PorosityMap = i.uv_texcoord * _PorosityMap_ST.xy + _PorosityMap_ST.zw;
			float4 Porosity97 = ( ( tex2D( _PorosityMap, uv_PorosityMap ) + _PorosityBias ) * _PorosityScale );
			Unity_GlossyEnvironmentData g84 = UnityGlossyEnvironmentSetup( _Reflectivity, data.worldViewDir, indirectNormal84, float3(0,0,0));
			float3 indirectSpecular84 = UnityGI_IndirectSpecular( data, Porosity97.r, indirectNormal84, g84 );
			float3 SpecularResult92 = indirectSpecular84;
			float4 PreAlbedo113 = ( lerpResult109 * float4( SpecularResult92 , 0.0 ) );
			float4 lerpResult117 = lerp( AlbedoTint81 , PreAlbedo113 , _Albedo_Light_Mix);
			c.rgb = lerpResult117.rgb;
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
-45;552;1009;858;1613.736;1469.046;2.132999;True;False
Node;AmplifyShaderEditor.CommentaryNode;105;-3484.46,83.88979;Float;False;2367.164;985.5066;Comment;3;68;57;77;LIGHT;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;57;-3437.94,620.1055;Float;False;918.1544;402.8202;Comment;6;66;54;52;53;55;67;DirectionalLightFalloff;1,1,1,1;0;0
Node;AmplifyShaderEditor.NormalVertexDataNode;53;-3334.858,824.3275;Float;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;68;-3419.548,217.3988;Float;False;803.5861;297.7572;Comment;4;65;63;64;69;LightAttenuation;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;52;-3387.94,670.1056;Float;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.LightAttenuation;64;-3369.547,267.3989;Float;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;63;-3327.963,356.818;Float;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.DotProductOpNode;54;-3129.856,747.1011;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;66;-3115.665,850.7645;Float;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;67;-2885.732,813.7409;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;65;-3100.564,327.4509;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;103;-2531.165,-1781.9;Float;False;1278.087;280;Comment;5;100;98;93;99;97;Porosity Component;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;77;-2427.703,344.9637;Float;False;1180.377;466.0579;Comment;7;72;71;56;70;73;74;75;MixedLight;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;102;-2770.664,-1373.828;Float;False;1802.878;284.1809;Comment;7;96;94;95;101;86;84;92;Specular Component;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;100;-2113.178,-1637.065;Float;False;Property;_PorosityScale;PorosityScale;7;0;Create;True;0;0;False;0;0;0;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;55;-2731.609,808.9812;Float;False;LightFalloff;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;93;-2481.165,-1731.9;Float;True;Property;_PorosityMap;PorosityMap;2;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;98;-2130.355,-1705.768;Float;False;Property;_PorosityBias;PorosityBias;8;0;Create;True;0;0;False;0;1;0;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;69;-2907.234,325.0651;Float;False;LightAttenuation;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;104;-2342.448,-2361.936;Float;False;862.005;447.717;Comment;4;78;79;81;80;AlbedoComponent;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;70;-2377.703,579.4541;Float;False;69;LightAttenuation;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;56;-2358.841,403.2632;Float;False;55;LightFalloff;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;99;-1823.644,-1721.716;Float;False;ConstantBiasScale;-1;;3;63208df05c83e8e49a48ffbdce2e43a0;0;3;3;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;96;-2720.664,-1275.542;Float;False;Property;_NormalMapPower;NormalMapPower;6;0;Create;True;0;0;False;0;1;0;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;79;-2208.423,-2311.936;Float;False;Property;_AlbedoColor;AlbedoColor;4;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;72;-2040.755,562.7985;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;94;-2357.386,-1319.647;Float;True;Property;_NormalMap;NormalMap;1;0;Create;True;0;0;False;0;None;None;True;0;False;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;71;-2040.755,394.9638;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;74;-2135.952,696.022;Float;False;Property;_LightAddMultiplyMix;LightAddMultiplyMix;0;0;Create;True;0;0;False;0;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;97;-1496.078,-1727.851;Float;False;Porosity;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;78;-2292.448,-2144.219;Float;True;Property;_Albedo;Albedo;3;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;80;-1944.987,-2161.663;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;73;-1725.564,457.4711;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;101;-1824.963,-1215.25;Float;False;97;Porosity;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;95;-2031.954,-1319.669;Float;False;MainNormalMap;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;86;-1809.467,-1298.466;Float;False;Property;_Reflectivity;Reflectivity;5;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;114;-2398.14,-984.2859;Float;False;1116.205;324.8481;Comment;7;106;107;110;76;109;112;113;Pre Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.IndirectSpecularLight;84;-1536.754,-1319.612;Float;False;Tangent;3;0;FLOAT3;0,0,1;False;1;FLOAT;0.5;False;2;FLOAT;0.5;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;75;-1490.327,451.9513;Float;False;MixedLight;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;81;-1723.443,-2165.882;Float;False;AlbedoTint;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;92;-1218.786,-1323.828;Float;False;SpecularResult;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;106;-2270.951,-934.2859;Float;False;75;MixedLight;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;107;-2267.963,-856.5852;Float;False;81;AlbedoTint;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;110;-2348.14,-781.752;Float;False;Property;_MixeldLightAlbedo;MixeldLightAlbedo;9;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;76;-2015.441,-774.4378;Float;False;92;SpecularResult;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;109;-1981.041,-914.6521;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;112;-1730.194,-837.0033;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;113;-1524.935,-841.5536;Float;False;PreAlbedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;115;-543.0923,-975.6312;Float;False;81;AlbedoTint;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;118;-610.7231,-811.4263;Float;False;Property;_Albedo_Light_Mix;Albedo_Light_Mix;10;0;Create;True;0;0;False;0;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;116;-538.1414,-899.0409;Float;False;113;PreAlbedo;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;117;-272.1414,-944.0409;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;-51.43379,-1177.233;Float;False;True;2;Float;ASEMaterialInspector;0;0;CustomLighting;SkinShader_Hyper;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;54;0;52;0
WireConnection;54;1;53;0
WireConnection;67;0;54;0
WireConnection;67;1;66;0
WireConnection;65;0;64;0
WireConnection;65;1;63;0
WireConnection;55;0;67;0
WireConnection;69;0;65;0
WireConnection;99;3;93;0
WireConnection;99;1;98;0
WireConnection;99;2;100;0
WireConnection;72;0;56;0
WireConnection;72;1;70;0
WireConnection;94;5;96;0
WireConnection;71;0;56;0
WireConnection;71;1;70;0
WireConnection;97;0;99;0
WireConnection;80;0;79;0
WireConnection;80;1;78;0
WireConnection;73;0;71;0
WireConnection;73;1;72;0
WireConnection;73;2;74;0
WireConnection;95;0;94;0
WireConnection;84;0;95;0
WireConnection;84;1;86;0
WireConnection;84;2;101;0
WireConnection;75;0;73;0
WireConnection;81;0;80;0
WireConnection;92;0;84;0
WireConnection;109;0;106;0
WireConnection;109;1;107;0
WireConnection;109;2;110;0
WireConnection;112;0;109;0
WireConnection;112;1;76;0
WireConnection;113;0;112;0
WireConnection;117;0;115;0
WireConnection;117;1;116;0
WireConnection;117;2;118;0
WireConnection;0;13;117;0
ASEEND*/
//CHKSM=C7170D60CD6BBDDCC498507030A0A6922B449AD5