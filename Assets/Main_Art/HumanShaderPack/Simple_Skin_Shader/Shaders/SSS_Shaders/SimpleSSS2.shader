// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "RRF_HumanShaders/Skin Shaders/SimpleSSS/SimpleSSS2"
{
	Properties
	{
		_Albedo_Color("Albedo_Color", Color) = (0.8382353,0.7272924,0.7272924,0)
		_ScatterColor("ScatterColor", Color) = (0,0,0,0)
		_Light_Bias("Light_Bias", Range( 0 , 1)) = 0
		_Light_Scale("Light_Scale", Range( 0 , 10)) = 0
		_LightScatter("LightScatter", Range( 0 , 1)) = 0
		_Smoothness("Smoothness", Range( 0 , 1)) = 0
		_BackScatterPower("BackScatterPower", Range( 0 , 1)) = 0
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
			float3 viewDir;
		};

		uniform float4 _Albedo_Color;
		uniform float4 _ScatterColor;
		uniform float _Light_Bias;
		uniform float _Light_Scale;
		uniform float _LightScatter;
		uniform float _BackScatterPower;
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
			float clampResult82 = clamp( dotResult24 , 0.0 , 1.0 );
			float4 temp_output_43_0 = ( _Albedo_Color * clampResult82 );
			float dotResult119 = dot( ase_vertexNormal , ase_worldlightDir );
			float3 temp_cast_1 = (dotResult119).xxx;
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float3 objToWorldDir30 = mul( unity_ObjectToWorld, float4( ase_vertex3Pos, 0 ) ).xyz;
			float3 normalizeResult32 = normalize( objToWorldDir30 );
			float dotResult37 = dot( temp_cast_1 , normalizeResult32 );
			float clampResult112 = clamp( ( ( dotResult37 + _Light_Bias ) * _Light_Scale ) , 0.0 , 1.0 );
			float4 lerpResult102 = lerp( temp_output_43_0 , ( _ScatterColor * clampResult112 ) , _LightScatter);
			float dotResult64 = dot( ase_worldlightDir , normalizeResult32 );
			float clampResult69 = clamp( dotResult64 , 0.0 , 1.0 );
			float dotResult72 = dot( -i.viewDir , ase_worldlightDir );
			float clampResult73 = clamp( dotResult72 , 0.0 , 1.0 );
			float4 lerpResult113 = lerp( temp_output_43_0 , ( lerpResult102 + ( _ScatterColor * ( ( clampResult69 * clampResult73 ) * _BackScatterPower ) ) ) , clampResult112);
			float4 clampResult116 = clamp( lerpResult113 , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
			o.Emission = clampResult116.rgb;
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
				surfIN.viewDir = worldViewDir;
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
1927;7;1906;1014;2860.653;695.8835;1.814341;True;True
Node;AmplifyShaderEditor.CommentaryNode;56;-1832.857,19.23205;Float;False;1435.461;680.1272;ScatterLight;11;29;30;32;35;40;39;37;44;38;33;119;;1,1,1,1;0;0
Node;AmplifyShaderEditor.PosVertexDataNode;29;-1782.857,303.8703;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;76;-348.7604,442.9608;Float;False;1116.434;621.9487;BackScatter;9;64;70;71;74;72;69;75;73;87;;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;35;-1772.715,146.5148;Float;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NormalVertexDataNode;118;-1816.184,-118.8298;Float;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TransformDirectionNode;30;-1516.109,325.217;Float;False;Object;World;False;Fast;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;70;-264.73,684.7723;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;57;-1537.432,-617.6573;Float;False;915.4287;532.0931;MainLight;6;22;23;24;43;82;51;;1,1,1,1;0;0
Node;AmplifyShaderEditor.NormalizeNode;32;-1241.061,337.4789;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NegateNode;74;-5.66875,765.5759;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;71;-298.7604,885.9095;Float;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DotProductOpNode;119;-1231.721,92.84565;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;37;-989.1948,163.7084;Float;False;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;40;-1545.164,584.3593;Float;False;Property;_Light_Scale;Light_Scale;4;0;Create;True;0;0;False;0;0;1.2;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;39;-1538.345,472.2159;Float;False;Property;_Light_Bias;Light_Bias;3;0;Create;True;0;0;False;0;0;0.36;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;23;-1446.278,-249.436;Float;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;22;-1485.335,-402.1946;Float;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DotProductOpNode;64;-201.1009,496.7227;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;72;214.1929,816.2384;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;73;434.0124,816.8043;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;24;-1079.135,-380.4155;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;69;97.08311,516.852;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;38;-820.8953,275.1207;Float;False;ConstantBiasScale;-1;;6;63208df05c83e8e49a48ffbdce2e43a0;0;3;3;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;75;598.6734,627.3169;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;87;345.288,969.9673;Float;False;Property;_BackScatterPower;BackScatterPower;7;0;Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;58;-293.5292,-316.7261;Float;False;722.2638;725.0239;FuzzMix;1;45;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ClampOpNode;82;-900.0018,-377.0577;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;112;-404.1412,277.1945;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;44;-839.0593,69.23205;Float;False;Property;_ScatterColor;ScatterColor;2;0;Create;True;0;0;False;0;0,0,0,0;1,0.0147059,0.0147059,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;51;-1492.851,-595.3783;Float;False;Property;_Albedo_Color;Albedo_Color;1;0;Create;True;0;0;False;0;0.8382353,0.7272924,0.7272924,0;0.6544118,0.6111051,0.6111051,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;33;-895.9128,510.9668;Float;False;Property;_LightScatter;LightScatter;5;0;Create;True;0;0;False;0;0;0.44;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;43;-680.5528,-435.8714;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;45;-195.8947,95.11189;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;86;903.1274,658.6421;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;102;623.5731,137.3984;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;108;1132.269,414.7261;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;109;1294.061,114.0206;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;113;1500.466,-160.2293;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;116;1797.166,-146.4648;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;59;701.1106,-138.2071;Float;False;Property;_Smoothness;Smoothness;6;0;Create;True;0;0;False;0;0;0.58;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;21;2088.293,-201.9155;Float;False;True;6;Float;ASEMaterialInspector;0;0;Standard;RRF_HumanShaders/Skin Shaders/SimpleSSS/SimpleSSS2;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;Opaque;;Background;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;20.4;10;25;False;0.5;True;0;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;30;0;29;0
WireConnection;32;0;30;0
WireConnection;74;0;70;0
WireConnection;119;0;118;0
WireConnection;119;1;35;0
WireConnection;37;0;119;0
WireConnection;37;1;32;0
WireConnection;64;0;35;0
WireConnection;64;1;32;0
WireConnection;72;0;74;0
WireConnection;72;1;71;0
WireConnection;73;0;72;0
WireConnection;24;0;22;0
WireConnection;24;1;23;0
WireConnection;69;0;64;0
WireConnection;38;3;37;0
WireConnection;38;1;39;0
WireConnection;38;2;40;0
WireConnection;75;0;69;0
WireConnection;75;1;73;0
WireConnection;82;0;24;0
WireConnection;112;0;38;0
WireConnection;43;0;51;0
WireConnection;43;1;82;0
WireConnection;45;0;44;0
WireConnection;45;1;112;0
WireConnection;86;0;75;0
WireConnection;86;1;87;0
WireConnection;102;0;43;0
WireConnection;102;1;45;0
WireConnection;102;2;33;0
WireConnection;108;0;44;0
WireConnection;108;1;86;0
WireConnection;109;0;102;0
WireConnection;109;1;108;0
WireConnection;113;0;43;0
WireConnection;113;1;109;0
WireConnection;113;2;112;0
WireConnection;116;0;113;0
WireConnection;21;0;51;0
WireConnection;21;2;116;0
WireConnection;21;4;59;0
ASEEND*/
//CHKSM=F44A90E36067A16D45DE0DA19CF1FBBF90FFB60D