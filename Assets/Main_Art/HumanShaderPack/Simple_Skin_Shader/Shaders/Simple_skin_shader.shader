// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "RRF_HumanShaders/Skin Shaders/Simple_skin_shader"
{
	Properties
	{
		[Header(Translucency)]
		_Translucency("Strength", Range( 0 , 50)) = 1
		_TransNormalDistortion("Normal Distortion", Range( 0 , 1)) = 0.1
		_TransScattering("Scaterring Falloff", Range( 1 , 50)) = 2
		_TransDirect("Direct", Range( 0 , 1)) = 1
		_TransAmbient("Ambient", Range( 0 , 1)) = 0.2
		_TransShadow("Shadow", Range( 0 , 1)) = 0.9
		_SSSMultiply("SSS-Multiply", Range( 1 , 5)) = 1
		_SSSAdd("SSS-Add", Range( 0 , 5)) = 1
		_BaseGlow("BaseGlow", Range( 0 , 1)) = 3
		_Albedo("Albedo", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,0)
		_SSS("SSS", 2D) = "white" {}
		_SSSTint("SSS-Tint", Color) = (0,0,0,0)
		_Normal("Normal", 2D) = "bump" {}
		_MetalRSmoothG("Metal(R)-Smooth(G)", 2D) = "black" {}
		_Metalness_Add("Metalness_Add", Range( 0 , 1)) = 0
		_Smoothness_Add("Smoothness_Add", Range( -1 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityPBSLighting.cginc"
		#pragma target 3.0
		#pragma surface surf StandardCustom keepalpha addshadow fullforwardshadows exclude_path:deferred 
		struct Input
		{
			float2 uv_texcoord;
		};

		struct SurfaceOutputStandardCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			half3 Translucency;
		};

		uniform sampler2D _Normal;
		uniform float4 _Normal_ST;
		uniform float4 _Color;
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform sampler2D _SSS;
		uniform float4 _SSS_ST;
		uniform float _SSSMultiply;
		uniform float _SSSAdd;
		uniform float4 _SSSTint;
		uniform float _BaseGlow;
		uniform sampler2D _MetalRSmoothG;
		uniform float4 _MetalRSmoothG_ST;
		uniform float _Metalness_Add;
		uniform float _Smoothness_Add;
		uniform half _Translucency;
		uniform half _TransNormalDistortion;
		uniform half _TransScattering;
		uniform half _TransDirect;
		uniform half _TransAmbient;
		uniform half _TransShadow;

		inline half4 LightingStandardCustom(SurfaceOutputStandardCustom s, half3 viewDir, UnityGI gi )
		{
			#if !DIRECTIONAL
			float3 lightAtten = gi.light.color;
			#else
			float3 lightAtten = lerp( _LightColor0.rgb, gi.light.color, _TransShadow );
			#endif
			half3 lightDir = gi.light.dir + s.Normal * _TransNormalDistortion;
			half transVdotL = pow( saturate( dot( viewDir, -lightDir ) ), _TransScattering );
			half3 translucency = lightAtten * (transVdotL * _TransDirect + gi.indirect.diffuse * _TransAmbient) * s.Translucency;
			half4 c = half4( s.Albedo * translucency * _Translucency, 0 );

			SurfaceOutputStandard r;
			r.Albedo = s.Albedo;
			r.Normal = s.Normal;
			r.Emission = s.Emission;
			r.Metallic = s.Metallic;
			r.Smoothness = s.Smoothness;
			r.Occlusion = s.Occlusion;
			r.Alpha = s.Alpha;
			return LightingStandard (r, viewDir, gi) + c;
		}

		inline void LightingStandardCustom_GI(SurfaceOutputStandardCustom s, UnityGIInput data, inout UnityGI gi )
		{
			#if defined(UNITY_PASS_DEFERRED) && UNITY_ENABLE_REFLECTION_BUFFERS
				gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal);
			#else
				UNITY_GLOSSY_ENV_FROM_SURFACE( g, s, data );
				gi = UnityGlobalIllumination( data, s.Occlusion, s.Normal, g );
			#endif
		}

		void surf( Input i , inout SurfaceOutputStandardCustom o )
		{
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			o.Normal = UnpackNormal( tex2D( _Normal, uv_Normal ) );
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			float4 temp_output_19_0 = ( _Color * tex2D( _Albedo, uv_Albedo ) );
			o.Albedo = temp_output_19_0.rgb;
			float2 uv_SSS = i.uv_texcoord * _SSS_ST.xy + _SSS_ST.zw;
			float4 temp_output_63_0 = ( ( ( tex2D( _SSS, uv_SSS ) * _SSSMultiply ) + _SSSAdd ) * _SSSTint );
			o.Emission = ( ( temp_output_19_0 + temp_output_63_0 ) * _BaseGlow ).rgb;
			float2 uv_MetalRSmoothG = i.uv_texcoord * _MetalRSmoothG_ST.xy + _MetalRSmoothG_ST.zw;
			float4 tex2DNode53 = tex2D( _MetalRSmoothG, uv_MetalRSmoothG );
			float clampResult60 = clamp( ( tex2DNode53.r + _Metalness_Add ) , 0.0 , 1.0 );
			o.Metallic = clampResult60;
			float clampResult56 = clamp( ( tex2DNode53.g + _Smoothness_Add ) , 0.0 , 1.0 );
			o.Smoothness = clampResult56;
			o.Translucency = temp_output_63_0.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	//CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16204
85;270;1272;773;1146.361;257.5629;2.152905;True;False
Node;AmplifyShaderEditor.SamplerNode;18;-1437.093,66.75894;Float;True;Property;_SSS;SSS;12;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;21;-1416.881,370.4738;Float;False;Property;_SSSMultiply;SSS-Multiply;7;0;Create;True;0;0;False;0;1;1.15;1;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;31;-917.6019,199.2372;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;30;-1422.539,472.2185;Float;False;Property;_SSSAdd;SSS-Add;8;0;Create;True;0;0;False;0;1;0.4;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;38;-552.3506,225.8797;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;1;-1039.244,-289.3623;Float;True;Property;_Albedo;Albedo;10;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;13;-988.0854,-478.3104;Float;False;Property;_Color;Color;11;0;Create;True;0;0;False;0;1,1,1,0;1,1,1,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;23;-760.6592,426.5941;Float;False;Property;_SSSTint;SSS-Tint;13;0;Create;True;0;0;False;0;0,0,0,0;0.8455882,0.4974048,0.4974048,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;19;-423.5151,-480.3395;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;63;-286.5988,341.5035;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;55;331.1261,593.3849;Float;False;Property;_Smoothness_Add;Smoothness_Add;17;0;Create;True;0;0;False;0;0;-0.156;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;53;5.336126,471.4171;Float;True;Property;_MetalRSmoothG;Metal(R)-Smooth(G);15;0;Create;True;0;0;False;0;None;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;57;334.0925,500.6319;Float;False;Property;_Metalness_Add;Metalness_Add;16;0;Create;True;0;0;False;0;0;0.107;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;48;-338.4234,-62.49107;Float;False;Property;_BaseGlow;BaseGlow;9;0;Create;True;0;0;False;0;3;0.071;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;64;-183.2377,-269.0851;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;58;776.0464,471.8301;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;59;756.507,351.7611;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;49;476.549,-201.6709;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;60;964.0833,144.5517;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;56;999.3446,264.197;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;52;435.4141,113.8112;Float;True;Property;_Normal;Normal;14;0;Create;True;0;0;False;0;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1190.648,-116.6752;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;RRF_HumanShaders/Skin Shaders/Simple_skin_shader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;ForwardOnly;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;0;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;31;0;18;0
WireConnection;31;1;21;0
WireConnection;38;0;31;0
WireConnection;38;1;30;0
WireConnection;19;0;13;0
WireConnection;19;1;1;0
WireConnection;63;0;38;0
WireConnection;63;1;23;0
WireConnection;64;0;19;0
WireConnection;64;1;63;0
WireConnection;58;0;53;2
WireConnection;58;1;55;0
WireConnection;59;0;53;1
WireConnection;59;1;57;0
WireConnection;49;0;64;0
WireConnection;49;1;48;0
WireConnection;60;0;59;0
WireConnection;56;0;58;0
WireConnection;0;0;19;0
WireConnection;0;1;52;0
WireConnection;0;2;49;0
WireConnection;0;3;60;0
WireConnection;0;4;56;0
WireConnection;0;7;63;0
ASEEND*/
//CHKSM=980EEE75D7F5D7FCECFCA10E455EDEB0248ECB0C