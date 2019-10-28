// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "RRF_HumanShaders/Skin Shaders/SimpleSSS/SimpleSSS1"
{
	Properties
	{
		_Albedo_Color("Albedo_Color", Color) = (0.8382353,0.7272924,0.7272924,0)
		_ScatterLightTint("ScatterLightTint", Color) = (0,0,0,0)
		_FuzzScale("FuzzScale", Range( 0 , 3)) = 0
		_Fuzz("Fuzz", Range( 0 , 1)) = 0
		_Light_Bias("Light_Bias", Range( 0 , 1)) = 0
		_Light_Scale("Light_Scale", Range( 0 , 10)) = 0
		_LightScatter("LightScatter", Range( 0 , 1)) = 0
		_Smoothness("Smoothness", Range( 0 , 1)) = 0
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
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
		};

		uniform float4 _Albedo_Color;
		uniform float4 _ScatterLightTint;
		uniform float _Light_Bias;
		uniform float _Light_Scale;
		uniform float _LightScatter;
		uniform float _FuzzScale;
		uniform float _Fuzz;
		uniform float _Smoothness;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 temp_output_51_0 = _Albedo_Color;
			o.Albedo = temp_output_51_0.rgb;
			float3 ase_worldPos = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float3 ase_worldNormal = i.worldNormal;
			float3 ase_vertexNormal = mul( unity_WorldToObject, float4( ase_worldNormal, 0 ) );
			float dotResult24 = dot( ase_worldlightDir , ase_vertexNormal );
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float3 objToWorldDir30 = mul( unity_ObjectToWorld, float4( ase_vertex3Pos, 0 ) ).xyz;
			float3 normalizeResult32 = normalize( objToWorldDir30 );
			float dotResult37 = dot( ase_worldlightDir , normalizeResult32 );
			float4 temp_output_45_0 = ( _ScatterLightTint * ( ( dotResult37 + _Light_Bias ) * _Light_Scale ) );
			float4 lerpResult41 = lerp( ( _Albedo_Color * dotResult24 ) , temp_output_45_0 , _LightScatter);
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float fresnelNdotV46 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode46 = ( 0.0 + _FuzzScale * pow( 1.0 - fresnelNdotV46, 5.0 ) );
			float4 lerpResult48 = lerp( lerpResult41 , ( lerpResult41 + fresnelNode46 ) , _Fuzz);
			float4 clampResult55 = clamp( lerpResult48 , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
			o.Emission = clampResult55.rgb;
			o.Smoothness = _Smoothness;
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
				float3 worldPos : TEXCOORD1;
				float3 worldNormal : TEXCOORD2;
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
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.worldNormal = worldNormal;
				o.worldPos = worldPos;
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
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = IN.worldNormal;
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
1927;7;1906;1014;3186.128;1171.529;2.156148;True;True
Node;AmplifyShaderEditor.CommentaryNode;56;-1832.857,19.23205;Float;False;1435.461;680.1272;ScatterLight;13;29;30;32;35;40;39;37;44;38;50;45;33;63;;1,1,1,1;0;0
Node;AmplifyShaderEditor.PosVertexDataNode;29;-1782.857,303.8703;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TransformDirectionNode;30;-1524.187,300.9845;Float;False;Object;World;False;Fast;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NormalizeNode;32;-1234.272,303.5324;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;35;-1772.715,146.5148;Float;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;57;-1537.432,-617.6573;Float;False;915.4287;532.0931;MainLight;5;22;23;42;24;43;;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;22;-1487.432,-427.36;Float;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NormalVertexDataNode;23;-1457.624,-264.5642;Float;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;40;-1545.164,584.3593;Float;False;Property;_Light_Scale;Light_Scale;7;0;Create;True;0;0;False;0;0;0.17;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;37;-989.1948,163.7084;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;39;-1538.345,472.2159;Float;False;Property;_Light_Bias;Light_Bias;6;0;Create;True;0;0;False;0;0;0.253;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;38;-820.8953,275.1207;Float;False;ConstantBiasScale;-1;;6;63208df05c83e8e49a48ffbdce2e43a0;0;3;3;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;44;-839.0593,69.23205;Float;False;Property;_ScatterLightTint;ScatterLightTint;3;0;Create;True;0;0;False;0;0,0,0,0;0.4264706,0.4264706,0.4264706,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;51;-1213.71,-789.9437;Float;False;Property;_Albedo_Color;Albedo_Color;1;0;Create;True;0;0;False;0;0.8382353,0.7272924,0.7272924,0;0.1470588,0.1470588,0.1470588,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DotProductOpNode;24;-1150.235,-358.5388;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;33;-840.1542,441.2682;Float;False;Property;_LightScatter;LightScatter;8;0;Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;58;-8.401138,-318.9194;Float;False;621.3723;707.4776;FuzzMix;5;41;47;48;49;46;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;50;-793.6552,544.8486;Float;False;Property;_FuzzScale;FuzzScale;4;0;Create;True;0;0;False;0;0;0.76;0;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;45;-566.3956,188.8903;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;43;-788.8262,-496.0821;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;41;109.1857,-105.6417;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FresnelNode;46;74.7279,186.5582;Float;False;Standard;WorldNormal;ViewDir;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;49;41.59886,-219.7452;Float;False;Property;_Fuzz;Fuzz;5;0;Create;True;0;0;False;0;0;0.201;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;47;310.3409,-26.5122;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;48;428.9712,-268.9194;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DotProductOpNode;63;-1401.848,71.98749;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;59;701.1106,-138.2071;Float;False;Property;_Smoothness;Smoothness;9;0;Create;True;0;0;False;0;0;0.366;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;60;-113.1615,341.1123;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;42;-1213.989,-530.64;Float;False;Property;_MainLightTint;MainLightTint;2;0;Create;True;0;0;False;0;1,1,1,0;0.2696799,0.3322098,0.6323529,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;55;765.9523,-278.3831;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.NormalVertexDataNode;62;-1770.012,-151.8602;Float;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;21;1494.704,-469.6789;Float;False;True;6;Float;ASEMaterialInspector;0;0;Standard;RRF_HumanShaders/Skin Shaders/SimpleSSS/SimpleSSS1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;Opaque;;Background;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;20.4;10;25;False;0.5;True;0;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;30;0;29;0
WireConnection;32;0;30;0
WireConnection;37;0;35;0
WireConnection;37;1;32;0
WireConnection;38;3;37;0
WireConnection;38;1;39;0
WireConnection;38;2;40;0
WireConnection;24;0;22;0
WireConnection;24;1;23;0
WireConnection;45;0;44;0
WireConnection;45;1;38;0
WireConnection;43;0;51;0
WireConnection;43;1;24;0
WireConnection;41;0;43;0
WireConnection;41;1;45;0
WireConnection;41;2;33;0
WireConnection;46;2;50;0
WireConnection;47;0;41;0
WireConnection;47;1;46;0
WireConnection;48;0;41;0
WireConnection;48;1;47;0
WireConnection;48;2;49;0
WireConnection;63;0;62;0
WireConnection;63;1;35;0
WireConnection;60;0;45;0
WireConnection;55;0;48;0
WireConnection;21;0;51;0
WireConnection;21;2;55;0
WireConnection;21;4;59;0
ASEEND*/
//CHKSM=7136928EEEF8C48992A72720A186C7B7A21175A8