// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "RRF_HumanShaders/Skin Shaders/SimpleSSS/SimpleSSS6"
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
		_SSS_ToEmissive("SSS_ToEmissive", Range( 0.1 , 2)) = 0.1
		_ScatterColor("ScatterColor", Color) = (0,0,0,0)
		_Light_Bias("Light_Bias", Range( 0 , 1)) = 0
		_Light_Scale("Light_Scale", Range( 0 , 10)) = 0
		_LightScatter("LightScatter", Range( 0 , 1)) = 0
		_BackScatterPower("BackScatterPower", Range( 0 , 4)) = 0
		_MicroDetailNormalMap("MicroDetail(NormalMap)", 2D) = "bump" {}
		_MicroDetail_Tiling("MicroDetail_Tiling", Range( 1 , 12)) = 1
		_MicroDetail_Offset("MicroDetail_Offset", Range( 0 , 1)) = 0
		_MicroDetail_Power("MicroDetail_Power", Range( 0 , 2)) = 1
		_MicroDetail_WeightMapGreyscaleR("MicroDetail_WeightMap(Greyscale(R))", 2D) = "white" {}
		_MakeUpMask1_RGB("MakeUpMask1_RGB", 2D) = "black" {}
		_MakeUpMask2_RGB("MakeUpMask2_RGB", 2D) = "black" {}
		_Mask1_Rchannel_ColorAmountA("Mask1_Rchannel_Color-Amount(A)", Color) = (0.7735849,0.2006942,0.3233189,0.7843137)
		_Mask1_Gchannel_ColorAmountA("Mask1_Gchannel_Color-Amount(A)", Color) = (0.2,0.772549,0.4917381,0.7843137)
		_Mask1_Bchannel_ColorAmountA("Mask1_Bchannel_Color-Amount(A)", Color) = (0.2,0.406334,0.772549,0.7843137)
		_Mask2_Rchannel_ColorAmountA("Mask2_Rchannel_Color-Amount(A)", Color) = (0.2,0.772549,0.7071339,0.7843137)
		_Mask2_Gchannel_ColorAmountA("Mask2_Gchannel_Color-Amount(A)", Color) = (0.772549,0.2,0.6776864,0.7843137)
		_Mask2_Bchannel_ColorAmountA("Mask2_Bchannel_Color-Amount(A)", Color) = (0.772549,0.7037676,0.2,0.7843137)
		_GlossAdjust_Mask1Rchannel("GlossAdjust_Mask1-Rchannel", Range( -1 , 1)) = 0
		_GlossAdjust_Mask1Gchannel("GlossAdjust_Mask1-Gchannel", Range( -1 , 1)) = 0
		_GlossAdjust_Mask1Bchannel("GlossAdjust_Mask1-Bchannel", Range( -1 , 1)) = -1
		_GlossAdjust_Mask2Rchannel("GlossAdjust_Mask2-Rchannel", Range( -1 , 1)) = 0
		_GlossAdjust_Mask2Gchannel("GlossAdjust_Mask2-Gchannel", Range( -1 , 1)) = 0
		_GlossAdjust_Mask2Bchannel("GlossAdjust_Mask2-Bchannel", Range( -1 , 1)) = 0
		_FinalNormalMapPower("FinalNormalMapPower", Range( 0 , 4)) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityStandardUtils.cginc"
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
		uniform float _MicroDetail_Power;
		uniform sampler2D _MicroDetailNormalMap;
		uniform float _MicroDetail_Tiling;
		uniform float _MicroDetail_Offset;
		uniform sampler2D _MicroDetail_WeightMapGreyscaleR;
		uniform float4 _MicroDetail_WeightMapGreyscaleR_ST;
		uniform float _FinalNormalMapPower;
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
		uniform float4 _Mask1_Rchannel_ColorAmountA;
		uniform sampler2D _MakeUpMask1_RGB;
		uniform float4 _MakeUpMask1_RGB_ST;
		uniform float4 _Mask1_Gchannel_ColorAmountA;
		uniform float4 _Mask1_Bchannel_ColorAmountA;
		uniform float4 _Mask2_Rchannel_ColorAmountA;
		uniform sampler2D _MakeUpMask2_RGB;
		uniform float4 _MakeUpMask2_RGB_ST;
		uniform float4 _Mask2_Gchannel_ColorAmountA;
		uniform float4 _Mask2_Bchannel_ColorAmountA;
		uniform float _BackScatterPower;
		uniform float _SSS_ToEmissive;
		uniform sampler2D _MetalGloss;
		uniform float4 _MetalGloss_ST;
		uniform float _SmoothnessDeviate;
		uniform float _GlossAdjust_Mask1Rchannel;
		uniform float _GlossAdjust_Mask1Gchannel;
		uniform float _GlossAdjust_Mask1Bchannel;
		uniform float _GlossAdjust_Mask2Rchannel;
		uniform float _GlossAdjust_Mask2Gchannel;
		uniform float _GlossAdjust_Mask2Bchannel;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_NormalMap = i.uv_texcoord * _NormalMap_ST.xy + _NormalMap_ST.zw;
			float3 tex2DNode111 = UnpackNormal( tex2D( _NormalMap, uv_NormalMap ) );
			float2 temp_cast_0 = (_MicroDetail_Tiling).xx;
			float2 temp_cast_1 = (_MicroDetail_Offset).xx;
			float2 uv_TexCoord142 = i.uv_texcoord * temp_cast_0 + temp_cast_1;
			float2 uv_MicroDetail_WeightMapGreyscaleR = i.uv_texcoord * _MicroDetail_WeightMapGreyscaleR_ST.xy + _MicroDetail_WeightMapGreyscaleR_ST.zw;
			float3 lerpResult135 = lerp( tex2DNode111 , BlendNormals( tex2DNode111 , UnpackScaleNormal( tex2D( _MicroDetailNormalMap, uv_TexCoord142 ), _MicroDetail_Power ) ) , tex2D( _MicroDetail_WeightMapGreyscaleR, uv_MicroDetail_WeightMapGreyscaleR ).r);
			float3 lerpResult187 = lerp( float3(0,0,1) , lerpResult135 , _FinalNormalMapPower);
			float3 normalizeResult188 = normalize( lerpResult187 );
			o.Normal = normalizeResult188;
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
			float4 temp_output_115_0 = ( tex2DNode114 * lerpResult133 );
			float dotResult193 = dot( ase_vertexNormal , ase_worldlightDir );
			float3 temp_cast_3 = (dotResult193).xxx;
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float3 objToWorldDir30 = mul( unity_ObjectToWorld, float4( ase_vertex3Pos, 0 ) ).xyz;
			float3 normalizeResult32 = normalize( objToWorldDir30 );
			float dotResult37 = dot( temp_cast_3 , normalizeResult32 );
			float clampResult131 = clamp( ( ( dotResult37 + _Light_Bias ) * _Light_Scale ) , 0.0 , 1.0 );
			float4 lerpResult102 = lerp( ( _MainLightTint * clampResult82 * temp_output_113_0 ) , ( _ScatterColor * abs( ( temp_output_115_0 * clampResult131 ) ) ) , _LightScatter);
			float2 uv_MakeUpMask1_RGB = i.uv_texcoord * _MakeUpMask1_RGB_ST.xy + _MakeUpMask1_RGB_ST.zw;
			float4 tex2DNode143 = tex2D( _MakeUpMask1_RGB, uv_MakeUpMask1_RGB );
			float temp_output_154_0 = ( tex2DNode143.r * _Mask1_Rchannel_ColorAmountA.a );
			float4 lerpResult145 = lerp( ( temp_output_113_0 + ( float4( ( clampResult82 * ase_lightColor.rgb * clampResult107 ) , 0.0 ) * lerpResult102 ) ) , _Mask1_Rchannel_ColorAmountA , temp_output_154_0);
			float temp_output_155_0 = ( tex2DNode143.g * _Mask1_Gchannel_ColorAmountA.a );
			float4 lerpResult160 = lerp( lerpResult145 , _Mask1_Gchannel_ColorAmountA , temp_output_155_0);
			float temp_output_156_0 = ( tex2DNode143.b * _Mask1_Bchannel_ColorAmountA.a );
			float4 lerpResult161 = lerp( lerpResult160 , _Mask1_Bchannel_ColorAmountA , temp_output_156_0);
			float2 uv_MakeUpMask2_RGB = i.uv_texcoord * _MakeUpMask2_RGB_ST.xy + _MakeUpMask2_RGB_ST.zw;
			float4 tex2DNode144 = tex2D( _MakeUpMask2_RGB, uv_MakeUpMask2_RGB );
			float temp_output_157_0 = ( tex2DNode144.r * _Mask2_Rchannel_ColorAmountA.a );
			float4 lerpResult162 = lerp( lerpResult161 , _Mask2_Rchannel_ColorAmountA , temp_output_157_0);
			float temp_output_158_0 = ( tex2DNode144.g * _Mask2_Gchannel_ColorAmountA.a );
			float4 lerpResult163 = lerp( lerpResult162 , _Mask2_Gchannel_ColorAmountA , temp_output_158_0);
			float temp_output_159_0 = ( tex2DNode144.b * _Mask2_Bchannel_ColorAmountA.a );
			float4 lerpResult164 = lerp( lerpResult163 , _Mask2_Bchannel_ColorAmountA , temp_output_159_0);
			float dotResult64 = dot( ase_worldlightDir , normalizeResult32 );
			float clampResult69 = clamp( dotResult64 , 0.0 , 1.0 );
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float dotResult72 = dot( -ase_worldViewDir , ase_worldlightDir );
			float clampResult73 = clamp( dotResult72 , 0.0 , 1.0 );
			float temp_output_86_0 = ( ( clampResult69 * clampResult73 ) * _BackScatterPower );
			float4 clampResult110 = clamp( ( lerpResult102 + ( temp_output_115_0 * temp_output_86_0 ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
			float4 clampResult246 = clamp( ( lerpResult164 + clampResult110 ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
			o.Albedo = clampResult246.rgb;
			float4 lerpResult241 = lerp( float4( 0,0,0,0 ) , clampResult110 , _SSS_ToEmissive);
			o.Emission = lerpResult241.rgb;
			float2 uv_MetalGloss = i.uv_texcoord * _MetalGloss_ST.xy + _MetalGloss_ST.zw;
			float4 tex2DNode118 = tex2D( _MetalGloss, uv_MetalGloss );
			o.Metallic = tex2DNode118.r;
			float clampResult170 = clamp( ( ( tex2DNode118.a + _SmoothnessDeviate ) + ( ( temp_output_154_0 * _GlossAdjust_Mask1Rchannel ) + ( temp_output_155_0 * _GlossAdjust_Mask1Gchannel ) + ( temp_output_156_0 * _GlossAdjust_Mask1Bchannel ) + ( temp_output_157_0 * _GlossAdjust_Mask2Rchannel ) + ( temp_output_158_0 * _GlossAdjust_Mask2Gchannel ) + ( temp_output_159_0 * _GlossAdjust_Mask2Bchannel ) ) ) , 0.0 , 1.0 );
			float Gloss237 = clampResult170;
			o.Smoothness = Gloss237;
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
1927;29;1906;1014;-3988.468;902.6322;1.38621;True;True
Node;AmplifyShaderEditor.CommentaryNode;56;-1832.857,19.23205;Float;False;1435.461;680.1272;ScatterLight;13;29;30;32;35;40;39;37;44;114;117;133;115;193;;1,1,1,1;0;0
Node;AmplifyShaderEditor.PosVertexDataNode;29;-1782.857,346.5135;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;35;-1798.165,182.4451;Float;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NormalVertexDataNode;191;-2044.716,18.31868;Float;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TransformDirectionNode;30;-1516.109,325.217;Float;False;Object;World;False;Fast;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DotProductOpNode;193;-1447.417,58.50114;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalizeNode;32;-1256.061,303.4789;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;40;-1545.164,584.3593;Float;False;Property;_Light_Scale;Light_Scale;11;0;Create;True;0;0;False;0;0;0.43;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;37;-1006.646,176.1735;Float;False;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;114;-1087.933,336.5719;Float;True;Property;_SSS_TextureWeightA;SSS_Texture-Weight(A);6;0;Create;True;0;0;False;0;None;7b26834653248cd44a8460d6b82df114;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;39;-1538.345,472.2159;Float;False;Property;_Light_Bias;Light_Bias;10;0;Create;True;0;0;False;0;0;0.359;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;117;-1115.745,546.0661;Float;False;Property;_SSS_WeightAmount;SSS_WeightAmount;7;0;Create;True;0;0;False;0;0;0.664;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;38;-760.9614,705.5035;Float;False;ConstantBiasScale;-1;;8;63208df05c83e8e49a48ffbdce2e43a0;0;3;3;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;133;-712.0639,506.3657;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;57;-1537.432,-617.6573;Float;False;915.4287;532.0931;MainLight;8;23;42;24;43;61;82;107;238;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;58;-293.5292,-316.7261;Float;False;722.2638;725.0239;FuzzMix;4;45;130;33;62;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;115;-528.6672,247.3148;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;131;-473.285,693.9008;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;238;-1483.503,-544.5786;Float;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NormalVertexDataNode;23;-1469.568,-266.2705;Float;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;112;-1507.575,-1047.724;Float;True;Property;_Albedo;Albedo;2;0;Create;True;0;0;False;0;None;0260083bbedd7c6429e6919fc5564e85;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;45;-156.6521,194.803;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;51;-1471.425,-841.6143;Float;False;Property;_Albedo_Tint;Albedo_Tint;1;0;Create;True;0;0;False;0;1,1,1,0;0.8113208,0.8113208,0.8113208,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DotProductOpNode;24;-1079.135,-380.4155;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;44;-840.3965,42.26363;Float;False;Property;_ScatterColor;ScatterColor;9;0;Create;True;0;0;False;0;0,0,0,0;1,0.2028302,0.2028302,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.AbsOpNode;130;129.6605,200.4248;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;82;-900.0018,-377.0577;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;113;-1096.581,-893.8199;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;42;-1142.132,-567.6573;Float;False;Property;_MainLightTint;MainLightTint;0;0;Create;True;0;0;False;0;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LightColorNode;61;-1175.316,-228.1119;Float;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;240;232.0033,100.2536;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;43;-680.5528,-435.8714;Float;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;76;-348.7604,442.9608;Float;False;1116.434;621.9487;BackScatter;9;64;70;71;74;72;69;75;73;87;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ClampOpNode;107;-795.5683,-123.053;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;33;211.0021,254.6402;Float;False;Property;_LightScatter;LightScatter;12;0;Create;True;0;0;False;0;0;0.146;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;62;201.8478,-212.6943;Float;False;3;3;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;70;-292.5702,702.1722;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;165;1750.598,-1740.372;Float;False;2639.649;1190.825;Dual Make up mixer (6-channel);20;150;148;149;143;154;155;156;151;152;153;144;157;158;159;145;160;161;162;163;164;;1,1,1,1;0;0
Node;AmplifyShaderEditor.LerpOp;102;558.9145,119.4884;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;71;-298.7604,885.9095;Float;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;106;771.8459,17.08136;Float;False;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.NegateNode;74;-19.58881,765.5759;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;148;2471.771,-1690.372;Float;False;Property;_Mask1_Rchannel_ColorAmountA;Mask1_Rchannel_Color-Amount(A);21;0;Create;True;0;0;False;0;0.7735849,0.2006942,0.3233189,0.7843137;0.7735849,0.2006942,0.3233189,0.8235294;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;143;1800.598,-1609.35;Float;True;Property;_MakeUpMask1_RGB;MakeUpMask1_RGB;19;0;Create;True;0;0;False;0;None;a707636a483c29f468a3a8103ee280f0;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DotProductOpNode;64;-201.1009,496.7227;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;149;2508.771,-1515.372;Float;False;Property;_Mask1_Gchannel_ColorAmountA;Mask1_Gchannel_Color-Amount(A);22;0;Create;True;0;0;False;0;0.2,0.772549,0.4917381,0.7843137;0.2,0.772549,0.4917381,0.8470588;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DotProductOpNode;72;214.1929,816.2384;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;154;2938.419,-1620.001;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;120;1368.783,-606.5608;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;69;97.08311,516.852;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;150;2533.771,-1343.372;Float;False;Property;_Mask1_Bchannel_ColorAmountA;Mask1_Bchannel_Color-Amount(A);23;0;Create;True;0;0;False;0;0.2,0.406334,0.772549,0.7843137;0.2,0.334116,0.772549,0.6980392;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;145;3360.889,-1652.063;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;73;434.0124,816.8043;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;134;1214.742,606.8441;Float;False;1638.046;638.4865;MicroDetails;8;142;141;140;139;138;137;136;135;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;155;2977.822,-1467.519;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;153;2542.521,-756.5474;Float;False;Property;_Mask2_Bchannel_ColorAmountA;Mask2_Bchannel_Color-Amount(A);26;0;Create;True;0;0;False;0;0.772549,0.7037676,0.2,0.7843137;0.003470994,0.2562132,0.735849,0.7490196;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;87;345.288,969.9673;Float;False;Property;_BackScatterPower;BackScatterPower;13;0;Create;True;0;0;False;0;0;0.364;0;4;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;160;3553.487,-1518.916;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;75;598.6734,627.3169;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;156;3005.234,-1299.617;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;151;2480.521,-1103.547;Float;False;Property;_Mask2_Rchannel_ColorAmountA;Mask2_Rchannel_Color-Amount(A);24;0;Create;True;0;0;False;0;0.2,0.772549,0.7071339,0.7843137;0.1341669,0.1415094,0.1406545,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;152;2517.521,-928.5472;Float;False;Property;_Mask2_Gchannel_ColorAmountA;Mask2_Gchannel_Color-Amount(A);25;0;Create;True;0;0;False;0;0.772549,0.2,0.6776864,0.7843137;1,1,1,0.7058824;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;140;1278.675,919.4932;Float;False;Property;_MicroDetail_Offset;MicroDetail_Offset;16;0;Create;True;0;0;False;0;0;0.503;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;139;1264.742,800.3571;Float;False;Property;_MicroDetail_Tiling;MicroDetail_Tiling;15;0;Create;True;0;0;False;0;1;7.79;1;12;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;183;3047.648,-543.5031;Float;False;1484.969;839.1447;Additional Gloss Control via Masks;15;173;169;171;172;179;176;168;182;181;180;178;167;170;175;174;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;144;1830.065,-1082.506;Float;True;Property;_MakeUpMask2_RGB;MakeUpMask2_RGB;20;0;Create;True;0;0;False;0;None;f741a9a852f37114883f4edcc8a4f63b;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;172;3112.004,-342.6861;Float;False;Property;_GlossAdjust_Mask1Gchannel;GlossAdjust_Mask1-Gchannel;28;0;Create;True;0;0;False;0;0;-0.274;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;157;3010.615,-1086.17;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;141;1268.047,1031.532;Float;False;Property;_MicroDetail_Power;MicroDetail_Power;17;0;Create;True;0;0;False;0;1;2;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;161;3753.937,-1335.595;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;86;903.1274,658.6421;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;175;3097.648,179.0922;Float;False;Property;_GlossAdjust_Mask2Bchannel;GlossAdjust_Mask2-Bchannel;32;0;Create;True;0;0;False;0;0;-0.349;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;159;3067.207,-669.6664;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;173;3099.442,-55.58076;Float;False;Property;_GlossAdjust_Mask2Rchannel;GlossAdjust_Mask2-Rchannel;30;0;Create;True;0;0;False;0;0;-0.475;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;174;3100.015,67.42487;Float;False;Property;_GlossAdjust_Mask2Gchannel;GlossAdjust_Mask2-Gchannel;31;0;Create;True;0;0;False;0;0;-0.459;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;142;1591.898,822.9332;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;158;3036.074,-874.7222;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;169;3099.972,-253.1673;Float;False;Property;_GlossAdjust_Mask1Bchannel;GlossAdjust_Mask1-Bchannel;29;0;Create;True;0;0;False;0;-1;-0.421;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;171;3121.295,-432.9468;Float;False;Property;_GlossAdjust_Mask1Rchannel;GlossAdjust_Mask1-Rchannel;27;0;Create;True;0;0;False;0;0;0.251;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;59;1566.091,-49.84906;Float;False;Property;_SmoothnessDeviate;SmoothnessDeviate;5;0;Create;True;0;0;False;0;0;-0.313;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;137;1967.302,799.6944;Float;True;Property;_MicroDetailNormalMap;MicroDetail(NormalMap);14;0;Create;True;0;0;False;0;None;287acedd1e86ed140951c44576dc5b5c;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;162;3925.267,-1124.862;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;108;1036.37,348.5114;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;118;1547.486,-258.6144;Float;True;Property;_MetalGloss;Metal-Gloss;4;0;Create;True;0;0;False;0;None;a049867252a39ad4185eaca07e577617;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;168;3580.638,-227.8324;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;181;3578.128,36.91891;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;180;3578.128,-84.31366;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;111;1912.566,370.4979;Float;True;Property;_NormalMap;NormalMap;3;0;Create;True;0;0;False;0;None;e2479a6af2d8c4b4b8479c74b02a5982;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;182;3581.122,162.6415;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;179;3573.146,-493.5031;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;176;3578.629,-360.4333;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;178;3860.469,-245.5717;Float;False;6;6;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;163;4070.897,-948.3935;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;109;1337.204,121.2112;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;184;3418.921,674.4404;Float;False;558;427.8339;FinalNormal;3;187;186;185;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;119;2055.61,-115.5921;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BlendNormalsNode;136;2327.886,713.2786;Float;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;138;1960.642,1015.33;Float;True;Property;_MicroDetail_WeightMapGreyscaleR;MicroDetail_WeightMap(Greyscale(R));18;0;Create;True;0;0;False;0;None;7a3b1f679607db44f87bf2ac8402c1d3;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;167;4062.887,-295.5503;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;110;2377.878,105.0362;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;164;4206.247,-759.9321;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.Vector3Node;186;3468.921,724.4404;Float;False;Constant;_Vector0;Vector 0;32;0;Create;True;0;0;False;0;0,0,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;185;3522.607,987.2743;Float;False;Property;_FinalNormalMapPower;FinalNormalMapPower;33;0;Create;True;0;0;False;0;1;0.37;0;4;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;135;2668.787,656.8441;Float;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;243;2434.838,381.0009;Float;False;Property;_SSS_ToEmissive;SSS_ToEmissive;8;0;Create;True;0;0;False;0;0.1;0;0.1;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;219;5154.535,-480.4781;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;170;4363.73,-183.0071;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;187;3792.921,752.4404;Float;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;245;5730.485,397.6754;Float;False;244;Debug;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;246;5513.298,-454.8865;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.NormalizeNode;188;4331.543,676.1862;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;244;1204.34,1316.448;Float;False;Debug;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;241;2765.912,230.3048;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;237;5110.54,65.79439;Float;False;Gloss;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;195;4680.471,776.2856;Float;False;CarryNormals;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;21;6024.633,-40.72271;Float;False;True;6;Float;ASEMaterialInspector;0;0;Standard;RRF_HumanShaders/Skin Shaders/SimpleSSS/SimpleSSS6;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;30;0;29;0
WireConnection;193;0;191;0
WireConnection;193;1;35;0
WireConnection;32;0;30;0
WireConnection;37;0;193;0
WireConnection;37;1;32;0
WireConnection;38;3;37;0
WireConnection;38;1;39;0
WireConnection;38;2;40;0
WireConnection;133;1;114;4
WireConnection;133;2;117;0
WireConnection;115;0;114;0
WireConnection;115;1;133;0
WireConnection;131;0;38;0
WireConnection;45;0;115;0
WireConnection;45;1;131;0
WireConnection;24;0;238;0
WireConnection;24;1;23;0
WireConnection;130;0;45;0
WireConnection;82;0;24;0
WireConnection;113;0;112;0
WireConnection;113;1;51;0
WireConnection;240;0;44;0
WireConnection;240;1;130;0
WireConnection;43;0;42;0
WireConnection;43;1;82;0
WireConnection;43;2;113;0
WireConnection;107;0;61;2
WireConnection;62;0;82;0
WireConnection;62;1;61;1
WireConnection;62;2;107;0
WireConnection;102;0;43;0
WireConnection;102;1;240;0
WireConnection;102;2;33;0
WireConnection;106;0;62;0
WireConnection;106;1;102;0
WireConnection;74;0;70;0
WireConnection;64;0;35;0
WireConnection;64;1;32;0
WireConnection;72;0;74;0
WireConnection;72;1;71;0
WireConnection;154;0;143;1
WireConnection;154;1;148;4
WireConnection;120;0;113;0
WireConnection;120;1;106;0
WireConnection;69;0;64;0
WireConnection;145;0;120;0
WireConnection;145;1;148;0
WireConnection;145;2;154;0
WireConnection;73;0;72;0
WireConnection;155;0;143;2
WireConnection;155;1;149;4
WireConnection;160;0;145;0
WireConnection;160;1;149;0
WireConnection;160;2;155;0
WireConnection;75;0;69;0
WireConnection;75;1;73;0
WireConnection;156;0;143;3
WireConnection;156;1;150;4
WireConnection;157;0;144;1
WireConnection;157;1;151;4
WireConnection;161;0;160;0
WireConnection;161;1;150;0
WireConnection;161;2;156;0
WireConnection;86;0;75;0
WireConnection;86;1;87;0
WireConnection;159;0;144;3
WireConnection;159;1;153;4
WireConnection;142;0;139;0
WireConnection;142;1;140;0
WireConnection;158;0;144;2
WireConnection;158;1;152;4
WireConnection;137;1;142;0
WireConnection;137;5;141;0
WireConnection;162;0;161;0
WireConnection;162;1;151;0
WireConnection;162;2;157;0
WireConnection;108;0;115;0
WireConnection;108;1;86;0
WireConnection;168;0;156;0
WireConnection;168;1;169;0
WireConnection;181;0;158;0
WireConnection;181;1;174;0
WireConnection;180;0;157;0
WireConnection;180;1;173;0
WireConnection;182;0;159;0
WireConnection;182;1;175;0
WireConnection;179;0;154;0
WireConnection;179;1;171;0
WireConnection;176;0;155;0
WireConnection;176;1;172;0
WireConnection;178;0;179;0
WireConnection;178;1;176;0
WireConnection;178;2;168;0
WireConnection;178;3;180;0
WireConnection;178;4;181;0
WireConnection;178;5;182;0
WireConnection;163;0;162;0
WireConnection;163;1;152;0
WireConnection;163;2;158;0
WireConnection;109;0;102;0
WireConnection;109;1;108;0
WireConnection;119;0;118;4
WireConnection;119;1;59;0
WireConnection;136;0;111;0
WireConnection;136;1;137;0
WireConnection;167;0;119;0
WireConnection;167;1;178;0
WireConnection;110;0;109;0
WireConnection;164;0;163;0
WireConnection;164;1;153;0
WireConnection;164;2;159;0
WireConnection;135;0;111;0
WireConnection;135;1;136;0
WireConnection;135;2;138;1
WireConnection;219;0;164;0
WireConnection;219;1;110;0
WireConnection;170;0;167;0
WireConnection;187;0;186;0
WireConnection;187;1;135;0
WireConnection;187;2;185;0
WireConnection;246;0;219;0
WireConnection;188;0;187;0
WireConnection;244;0;86;0
WireConnection;241;1;110;0
WireConnection;241;2;243;0
WireConnection;237;0;170;0
WireConnection;195;0;188;0
WireConnection;21;0;246;0
WireConnection;21;1;188;0
WireConnection;21;2;241;0
WireConnection;21;3;118;0
WireConnection;21;4;237;0
ASEEND*/
//CHKSM=B08CF012A7287599AC46EFC0D2D83E2DDEAD0B06