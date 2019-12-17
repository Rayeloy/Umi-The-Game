
Shader "Transformation/BlendTextures"
{
	Properties
	{
		_Albedo("Albedo", 2D) = "white" {}
		_Albedo2("Albedo2", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		[NoScaleOffset]_NormalMap2("NormalMap2", 2D) = "bump" {}
		[NoScaleOffset]_NormalMap("NormalMap", 2D) = "bump" {}
		_Specular("Specular", 2D) = "white" {}
		_Specular2("Specular2", 2D) = "white" {}
		_Spec("Spec", Color) = (0,0,0,0)
		_AO2("AO2", 2D) = "white" {}
		_AO("AO", 2D) = "white" {}
		_SSS_thickness2("SSS_thickness2", 2D) = "white" {}
		_SSS_thickness("SSS_thickness", 2D) = "white" {}
		[Header(Translucency)]
		_Translucency("Strength", Range( 0 , 50)) = 1
		_TransNormalDistortion("Normal Distortion", Range( 0 , 1)) = 0.1
		_TransScattering("Scaterring Falloff", Range( 1 , 50)) = 2
		_TransDirect("Direct", Range( 0 , 1)) = 1
		_TransAmbient("Ambient", Range( 0 , 1)) = 0.2
		_Translucency_("Translucency_", Color) = (1,0,0,0)
		_TransShadow("Shadow", Range( 0 , 1)) = 0.9
		_TransformationMask("TransformationMask", 2D) = "white" {}
		_Transformation("Transformation", Range( 0 , 1)) = 0
		[Toggle]_uv3Model2("uv3Model2", Float) = 0
		[Toggle]_uv3("uv3", Float) = 0
		[Toggle]_uv0Model2("uv0Model2", Float) = 0
		[Toggle]_uv3Mask("uv3Mask", Float) = 0
		[Toggle]_uv0Mask("uv0Mask", Float) = 0
		[Toggle]_uv2Mask("uv2Mask", Float) = 0
		[Toggle]_uv1Model2("uv1Model2", Float) = 0
		[Toggle]_uv2Model2("uv2Model2", Float) = 0
		[Toggle]_uv2("uv2", Float) = 0
		[Toggle]_uv1Mask("uv1Mask", Float) = 0
		[Toggle]_uv1("uv1", Float) = 0
		[Toggle]_uv0("uv0", Float) = 0
		_TillingMask("TillingMask", Float) = 1
		_OffsetMask("OffsetMask", Float) = 0
		_powerFresnel("powerFresnel", Float) = 0
		_IntensityFresnel("IntensityFresnel", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] _texcoord4( "", 2D ) = "white" {}
		[HideInInspector] _texcoord3( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float2 uv2_texcoord2;
			float2 uv3_texcoord3;
			float2 uv4_texcoord4;
			float3 worldPos;
			INTERNAL_DATA
			float3 worldNormal;
		};

		struct SurfaceOutputStandardSpecularCustom
		{
			fixed3 Albedo;
			fixed3 Normal;
			half3 Emission;
			fixed3 Specular;
			half Smoothness;
			half Occlusion;
			fixed Alpha;
			fixed3 Translucency;
		};

		uniform sampler2D _NormalMap;
		uniform float _uv3;
		uniform float _uv2;
		uniform float _uv1;
		uniform float _uv0;
		uniform sampler2D _NormalMap2;
		uniform float _uv3Model2;
		uniform float _uv2Model2;
		uniform float _uv1Model2;
		uniform float _uv0Model2;
		uniform float _Transformation;
		uniform sampler2D _TransformationMask;
		uniform float _uv3Mask;
		uniform float _uv2Mask;
		uniform float _uv1Mask;
		uniform float _uv0Mask;
		uniform float _TillingMask;
		uniform float _OffsetMask;
		uniform sampler2D _Albedo;
		uniform sampler2D _Albedo2;
		uniform float4 _Color;
		uniform sampler2D _Specular;
		uniform sampler2D _Specular2;
		uniform float4 _Spec;
		uniform float _powerFresnel;
		uniform float _IntensityFresnel;
		uniform sampler2D _AO;
		uniform sampler2D _AO2;
		uniform half _Translucency;
		uniform half _TransNormalDistortion;
		uniform half _TransScattering;
		uniform half _TransDirect;
		uniform half _TransAmbient;
		uniform half _TransShadow;
		uniform float4 _Translucency_;
		uniform sampler2D _SSS_thickness;
		uniform sampler2D _SSS_thickness2;

		inline half4 LightingStandardSpecularCustom(SurfaceOutputStandardSpecularCustom s, half3 viewDir, UnityGI gi )
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

			SurfaceOutputStandardSpecular r;
			r.Albedo = s.Albedo;
			r.Normal = s.Normal;
			r.Emission = s.Emission;
			r.Specular = s.Specular;
			r.Smoothness = s.Smoothness;
			r.Occlusion = s.Occlusion;
			r.Alpha = s.Alpha;
			return LightingStandardSpecular (r, viewDir, gi) + c;
		}

		inline void LightingStandardSpecularCustom_GI(SurfaceOutputStandardSpecularCustom s, UnityGIInput data, inout UnityGI gi )
		{
			#if defined(UNITY_PASS_DEFERRED) && UNITY_ENABLE_REFLECTION_BUFFERS
				gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal);
			#else
				UNITY_GLOSSY_ENV_FROM_SURFACE( g, s, data );
				gi = UnityGlobalIllumination( data, s.Occlusion, s.Normal, g );
			#endif
		}

		void surf( Input i , inout SurfaceOutputStandardSpecularCustom o )
		{
			float2 uvDefault214 = lerp(lerp(lerp(lerp(i.uv_texcoord,i.uv_texcoord,_uv0),i.uv2_texcoord2,_uv1),i.uv3_texcoord3,_uv2),i.uv4_texcoord4,_uv3);
			float2 uvDefault2217 = lerp(lerp(lerp(lerp(i.uv_texcoord,i.uv_texcoord,_uv0Model2),i.uv2_texcoord2,_uv1Model2),i.uv3_texcoord3,_uv2Model2),i.uv4_texcoord4,_uv3Model2);
			float2 temp_cast_0 = (_TillingMask).xx;
			float2 temp_cast_1 = (_OffsetMask).xx;
			float2 uv_TexCoord231 = i.uv_texcoord * temp_cast_0 + temp_cast_1;
			float2 temp_cast_2 = (_TillingMask).xx;
			float2 temp_cast_3 = (_OffsetMask).xx;
			float2 uv2_TexCoord232 = i.uv2_texcoord2 * temp_cast_2 + temp_cast_3;
			float2 temp_cast_4 = (_TillingMask).xx;
			float2 temp_cast_5 = (_OffsetMask).xx;
			float2 uv3_TexCoord233 = i.uv3_texcoord3 * temp_cast_4 + temp_cast_5;
			float2 temp_cast_6 = (_TillingMask).xx;
			float2 uv4_TexCoord230 = i.uv4_texcoord4 * temp_cast_6;
			float2 uvMask_218 = lerp(lerp(lerp(lerp(uv_TexCoord231,uv_TexCoord231,_uv0Mask),uv2_TexCoord232,_uv1Mask),uv3_TexCoord233,_uv2Mask),uv4_TexCoord230,_uv3Mask);
			float mask177 = ( _Transformation * tex2D( _TransformationMask, uvMask_218 ).r );
			float3 lerpResult184 = lerp( UnpackNormal( tex2D( _NormalMap, uvDefault214 ) ) , UnpackNormal( tex2D( _NormalMap2, uvDefault2217 ) ) , mask177);
			o.Normal = lerpResult184;
			float4 lerpResult174 = lerp( tex2D( _Albedo, uvDefault214 ) , tex2D( _Albedo2, uvDefault2217 ) , mask177);
			float4 temp_output_52_0 = ( lerpResult174 * _Color );
			o.Albedo = temp_output_52_0.rgb;
			float4 tex2DNode58 = tex2D( _Specular, uvDefault214 );
			float4 tex2DNode180 = tex2D( _Specular2, uvDefault2217 );
			float4 lerpResult182 = lerp( tex2DNode58 , tex2DNode180 , mask177);
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float fresnelNDotV61 = dot( WorldNormalVector( i , lerpResult184 ), ase_worldViewDir );
			float fresnelNode61 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNDotV61, _powerFresnel ) );
			float clampResult164 = clamp( fresnelNode61 , 0.0 , 1.0 );
			float lerpResult166 = lerp( 1.0 , clampResult164 , _IntensityFresnel);
			o.Specular = ( lerpResult182 * _Spec * lerpResult166 ).rgb;
			float lerpResult187 = lerp( tex2DNode58.a , tex2DNode180.a , mask177);
			o.Smoothness = ( lerpResult187 * _Spec.a );
			float lerpResult189 = lerp( tex2D( _AO, uvDefault214 ).r , tex2D( _AO2, uvDefault2217 ).r , mask177);
			o.Occlusion = lerpResult189;
			float4 lerpResult80 = lerp( temp_output_52_0 , _Translucency_ , _Translucency_.a);
			float4 lerpResult192 = lerp( tex2D( _SSS_thickness, uvDefault214 ) , tex2D( _SSS_thickness2, uvDefault2217 ) , mask177);
			float clampResult82 = clamp( lerpResult189 , 0.3 , 1.0 );
			float clampResult85 = clamp( ( clampResult82 + 0.4 ) , 0.3 , 1.0 );
			o.Translucency = ( lerpResult80 * lerpResult192 * clampResult85 ).rgb;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardSpecularCustom keepalpha fullforwardshadows exclude_path:deferred 

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
				float4 customPack1 : TEXCOORD1;
				float4 customPack2 : TEXCOORD2;
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
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
				fixed3 worldNormal = UnityObjectToWorldNormal( v.normal );
				fixed3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				fixed3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.customPack1.zw = customInputData.uv2_texcoord2;
				o.customPack1.zw = v.texcoord1;
				o.customPack2.xy = customInputData.uv3_texcoord3;
				o.customPack2.xy = v.texcoord2;
				o.customPack2.zw = customInputData.uv4_texcoord4;
				o.customPack2.zw = v.texcoord3;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			fixed4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				surfIN.uv2_texcoord2 = IN.customPack1.zw;
				surfIN.uv3_texcoord3 = IN.customPack2.xy;
				surfIN.uv4_texcoord4 = IN.customPack2.zw;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				fixed3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandardSpecularCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandardSpecularCustom, o )
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
Version=15301
7;29;1352;692;659.5408;2509.41;11.83368;True;True
Node;AmplifyShaderEditor.RangedFloatNode;235;5718.433,3207.621;Float;False;Property;_OffsetMask;OffsetMask;34;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;234;5771.812,3108.646;Float;False;Property;_TillingMask;TillingMask;33;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;231;6064.518,2961.245;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ToggleSwitchNode;208;6381.068,2889.401;Float;False;Property;_uv0Mask;uv0Mask;25;0;Create;True;0;0;False;0;0;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;232;6227.392,3180.736;Float;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ToggleSwitchNode;209;6651.91,2950.718;Float;False;Property;_uv1Mask;uv1Mask;30;0;Create;True;0;0;False;0;0;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;233;6240.566,3324.479;Float;False;2;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;198;5627.788,2128.461;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ToggleSwitchNode;194;6093.537,2155.268;Float;False;Property;_uv0;uv0;32;0;Create;True;0;0;False;0;0;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;199;5648.388,2254.261;Float;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ToggleSwitchNode;206;6990.579,3013.25;Float;False;Property;_uv2Mask;uv2Mask;26;0;Create;True;0;0;False;0;0;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ToggleSwitchNode;203;6700.429,2568.992;Float;False;Property;_uv0Model2;uv0Model2;23;0;Create;True;0;0;False;0;0;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;230;6454.026,3478.971;Float;False;3;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ToggleSwitchNode;195;6366.379,2216.584;Float;False;Property;_uv1;uv1;31;0;Create;True;0;0;False;0;0;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ToggleSwitchNode;207;7252.884,3107.375;Float;False;Property;_uv3Mask;uv3Mask;24;0;Create;True;0;0;False;0;0;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ToggleSwitchNode;204;6973.271,2630.31;Float;False;Property;_uv1Model2;uv1Model2;27;0;Create;True;0;0;False;0;0;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;200;5659.827,2398.003;Float;False;2;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ToggleSwitchNode;205;7309.94,2692.84;Float;False;Property;_uv2Model2;uv2Model2;28;0;Create;True;0;0;False;0;0;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;229;7099.964,1349.307;Float;False;218;0;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;201;5725.808,2519.529;Float;False;3;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ToggleSwitchNode;196;6632.399,2276.291;Float;False;Property;_uv2;uv2;29;0;Create;True;0;0;False;0;0;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;218;7502.149,3081.982;Float;False;uvMask_;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ToggleSwitchNode;197;6894.704,2370.417;Float;False;Property;_uv3;uv3;22;0;Create;True;0;0;False;0;0;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ToggleSwitchNode;202;7572.245,2786.966;Float;False;Property;_uv3Model2;uv3Model2;21;0;Create;True;0;0;False;0;0;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;92;7413.039,1316.339;Float;True;Property;_TransformationMask;TransformationMask;19;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;90;7002.134,926.7128;Float;False;Property;_Transformation;Transformation;20;0;Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;217;7878.97,2751.904;Float;False;uvDefault2;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;228;2306.207,268.3581;Float;False;214;0;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;226;6537.592,26.52943;Float;False;214;0;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;214;7159.282,2358.601;Float;False;uvDefault;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;93;7672.603,923.2294;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;227;2314.227,514.6996;Float;False;217;0;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;225;6525.612,167.8709;Float;False;217;0;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;188;6844.896,413.0689;Float;False;177;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;68;6913.847,-46.52463;Float;True;Property;_AO;AO;9;0;Create;True;0;0;False;0;None;f456a7e873e6a734090181a285098ea9;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;185;2978.982,721.965;Float;False;177;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;190;6760.896,200.0689;Float;True;Property;_AO2;AO2;8;0;Create;True;0;0;False;0;None;f456a7e873e6a734090181a285098ea9;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;219;4287.107,-1102.432;Float;False;214;0;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;183;2659.982,489.9651;Float;True;Property;_NormalMap2;NormalMap2;3;1;[NoScaleOffset];Create;True;0;0;False;0;None;07843f4f6c8c3c04ea8f1506ebcd8407;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;7;2640.258,220.9868;Float;True;Property;_NormalMap;NormalMap;4;1;[NoScaleOffset];Create;True;0;0;False;0;None;07843f4f6c8c3c04ea8f1506ebcd8407;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;177;7879.171,930.0622;Float;False;mask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;220;4390.127,-936.0906;Float;False;217;0;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;51;4626.261,-1189.858;Float;True;Property;_Albedo;Albedo;0;0;Create;True;0;0;False;0;None;8c22148c2c2a36044bdea3c13e45fa4b;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;172;4642.938,-961.3585;Float;True;Property;_Albedo2;Albedo2;1;0;Create;True;0;0;False;0;None;8c22148c2c2a36044bdea3c13e45fa4b;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;179;5044.945,-850.6085;Float;False;177;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;184;3209.982,538.9651;Float;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;189;7148.706,223.9062;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;69;4242.824,1487.774;Float;False;Property;_powerFresnel;powerFresnel;35;0;Create;True;0;0;False;0;0;0.4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;222;4090.871,42.68402;Float;False;217;0;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;224;5946.396,586.3837;Float;False;217;0;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;174;5325.451,-1034.971;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FresnelNode;61;4645.314,1221.809;Float;True;Tangent;4;0;FLOAT3;0,0,1;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;53;5146.999,-644.4785;Float;False;Property;_Color;Color;2;0;Create;True;0;0;False;0;1,1,1,1;0.986,0.9927157,1,1;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;82;7367.077,108.3103;Float;False;3;0;FLOAT;0;False;1;FLOAT;0.3;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;223;5958.376,445.0422;Float;False;214;0;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;221;4119.751,-203.9569;Float;False;214;0;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;180;4497.46,30.15833;Float;True;Property;_Specular2;Specular2;6;0;Create;True;0;0;False;0;None;dd9486ab72ecf344ca2bd6445a1ae12c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;191;6268.369,598.8527;Float;True;Property;_SSS_thickness2;SSS_thickness2;10;0;Create;True;0;0;False;0;None;eb1a931b58ee09944809c86421fcae15;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;164;5070.095,1197.014;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;181;4760.713,331.5294;Float;False;177;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;83;7552.393,102.9292;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0.4;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;72;6042.848,25.44462;Float;False;Property;_Translucency_;Translucency_;18;0;Create;True;0;0;False;0;1,0,0,0;1,0,0.2376567,0.6431373;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;165;5045.66,1489.387;Float;False;Constant;_Float3;Float 3;31;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;58;4494.451,-162.7116;Float;True;Property;_Specular;Specular;5;0;Create;True;0;0;False;0;None;dd9486ab72ecf344ca2bd6445a1ae12c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;193;6433.369,875.8527;Float;False;177;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;52;5673.819,-940.3196;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;74;6217.067,371.6264;Float;True;Property;_SSS_thickness;SSS_thickness;11;0;Create;True;0;0;False;0;None;eb1a931b58ee09944809c86421fcae15;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;70;4899.765,1591.43;Float;False;Property;_IntensityFresnel;IntensityFresnel;36;0;Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;85;7716.257,73.13563;Float;False;3;0;FLOAT;0;False;1;FLOAT;0.3;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;166;5242.343,1502.433;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;66;5250.819,-160.9125;Float;False;Property;_Spec;Spec;7;0;Create;True;0;0;False;0;0,0,0,0;0.9009434,0.9174528,1,1;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;192;6706.369,554.8527;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;187;5195.127,-314.6534;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;80;6386.399,-107.5221;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;182;4979.162,26.31233;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;79;8119.419,-80.92761;Float;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;67;5821.972,-178.8449;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;63;5498.699,147.4687;Float;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;71;9683.518,-351.4303;Float;False;True;2;Float;ASEMaterialInspector;0;0;StandardSpecular;Transformation/SSS;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;0;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;ForwardOnly;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;-1;False;-1;-1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Absolute;0;;-1;12;-1;-1;0;0;0;False;0;0;0;False;-1;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;231;0;234;0
WireConnection;231;1;235;0
WireConnection;208;0;231;0
WireConnection;208;1;231;0
WireConnection;232;0;234;0
WireConnection;232;1;235;0
WireConnection;209;0;208;0
WireConnection;209;1;232;0
WireConnection;233;0;234;0
WireConnection;233;1;235;0
WireConnection;194;0;198;0
WireConnection;194;1;198;0
WireConnection;206;0;209;0
WireConnection;206;1;233;0
WireConnection;203;0;198;0
WireConnection;203;1;198;0
WireConnection;230;0;234;0
WireConnection;195;0;194;0
WireConnection;195;1;199;0
WireConnection;207;0;206;0
WireConnection;207;1;230;0
WireConnection;204;0;203;0
WireConnection;204;1;199;0
WireConnection;205;0;204;0
WireConnection;205;1;200;0
WireConnection;196;0;195;0
WireConnection;196;1;200;0
WireConnection;218;0;207;0
WireConnection;197;0;196;0
WireConnection;197;1;201;0
WireConnection;202;0;205;0
WireConnection;202;1;201;0
WireConnection;92;1;229;0
WireConnection;217;0;202;0
WireConnection;214;0;197;0
WireConnection;93;0;90;0
WireConnection;93;1;92;1
WireConnection;68;1;226;0
WireConnection;190;1;225;0
WireConnection;183;1;227;0
WireConnection;7;1;228;0
WireConnection;177;0;93;0
WireConnection;51;1;219;0
WireConnection;172;1;220;0
WireConnection;184;0;7;0
WireConnection;184;1;183;0
WireConnection;184;2;185;0
WireConnection;189;0;68;1
WireConnection;189;1;190;1
WireConnection;189;2;188;0
WireConnection;174;0;51;0
WireConnection;174;1;172;0
WireConnection;174;2;179;0
WireConnection;61;0;184;0
WireConnection;61;3;69;0
WireConnection;82;0;189;0
WireConnection;180;1;222;0
WireConnection;191;1;224;0
WireConnection;164;0;61;0
WireConnection;83;0;82;0
WireConnection;58;1;221;0
WireConnection;52;0;174;0
WireConnection;52;1;53;0
WireConnection;74;1;223;0
WireConnection;85;0;83;0
WireConnection;166;0;165;0
WireConnection;166;1;164;0
WireConnection;166;2;70;0
WireConnection;192;0;74;0
WireConnection;192;1;191;0
WireConnection;192;2;193;0
WireConnection;187;0;58;4
WireConnection;187;1;180;4
WireConnection;187;2;181;0
WireConnection;80;0;52;0
WireConnection;80;1;72;0
WireConnection;80;2;72;4
WireConnection;182;0;58;0
WireConnection;182;1;180;0
WireConnection;182;2;181;0
WireConnection;79;0;80;0
WireConnection;79;1;192;0
WireConnection;79;2;85;0
WireConnection;67;0;187;0
WireConnection;67;1;66;4
WireConnection;63;0;182;0
WireConnection;63;1;66;0
WireConnection;63;2;166;0
WireConnection;71;0;52;0
WireConnection;71;1;184;0
WireConnection;71;3;63;0
WireConnection;71;4;67;0
WireConnection;71;5;189;0
WireConnection;71;7;79;0
ASEEND*/
//CHKSM=6FEC0F711DCFA5B301E020707CEB1DCAB5478EAF