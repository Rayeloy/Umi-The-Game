Shader "Transformation/SSS"
{
	Properties
	{
		_Albedo("Albedo", 2D) = "white" {}
		_Albedo2("Albedo2", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,0)
		_Color2("Color2", Color) = (1,1,1,0)
		[NoScaleOffset]_NormalMap2("NormalMap2", 2D) = "bump" {}
		[NoScaleOffset]_NormalMap("NormalMap", 2D) = "bump" {}
		_Specular("Specular/Gloss(A)", 2D) = "white" {}
		_Specular2("Specular2/Gloss2(A)", 2D) = "white" {}
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
		_ContrastMaskZ("ContrastMaskZ", Float) = 1
		_ContrastMaskY("ContrastMaskY", Float) = 1
		_ContrastMaskX("ContrastMaskX", Float) = 1
		_LevelsX("LevelsX", Vector) = (-200,0,0,1)
		_LevelsY("LevelsY", Vector) = (-200,0,0,1)
		_LevelsZ("LevelsZ", Vector) = (-200,0,0,1)
		[Toggle]_InvertX("InvertX", Float) = 1
		[Toggle]_InvertY("InvertY", Float) = 1
		[Toggle]_InvertZ("InvertZ", Float) = 1
		_IntensityMaskX("IntensityMaskX", Float) = 1
		_IntensityMaskY("IntensityMaskY", Float) = 1
		_IntensityMaskZ("IntensityMaskZ", Float) = 1
		__scale("scale", Float) = 1
		[Toggle]_MaskInfo("MaskInfo", Float) = 0
		_powerFresnel("powerFresnel", Float) = 0
		_IntensityFresnel("IntensityFresnel", Range( 0 , 1)) = 0
		[Toggle]_uv0("uv0", Float) = 0
		[Toggle]_uv1("uv1", Float) = 0
		[Toggle]_uv2("uv2", Float) = 0
		[Toggle]_uv3("uv3", Float) = 0
		[Toggle]_uv0Model2("uv0Model2", Float) = 0
		[Toggle]_uv1Model2("uv1Model2", Float) = 0
		[Toggle]_uv2Model2("uv2Model2", Float) = 0
		[Toggle]_uv3Model2("uv3Model2", Float) = 0
		[Toggle]_uv0Mask("uv0Mask", Float) = 0
		[Toggle]_uv1Mask("uv1Mask", Float) = 0
		[Toggle]_uv2Mask("uv2Mask", Float) = 0
		[Toggle]_uv3Mask("uv3Mask", Float) = 0

		_TillingMask("TillingMask", Float) = 1
		_OffsetMask("OffsetMask", Range( 0 , 3)) = 0
		_IntensityMask ("IntensityMask", Float) = 1
		_MaxIntensityMask ("MaxIntensityMask", Float) = 1
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] _texcoord4( "", 2D ) = "white" {}
		[HideInInspector] _texcoord3( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
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

				 struct appdata__{
               float4 vertex : POSITION;
    float4 tangent : TANGENT;
    float3 normal : NORMAL;
    float4 texcoord : TEXCOORD0;
    float4 texcoord1 : TEXCOORD1;
	 float4 texcoord2 : TEXCOORD2;
	 float4 texcoord3 : TEXCOORD3;
    fixed4 color : COLOR;
	uint id : SV_VertexID;
            
         };

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

		uniform float _ContrastMaskX;
		uniform float _InvertX;
		uniform float4 _LevelsX;
		uniform float _IntensityMaskX;
		uniform float _ContrastMaskY;
		uniform float _InvertY;
		uniform float4 _LevelsY;
		uniform float _IntensityMaskY;
		uniform float _ContrastMaskZ;
		uniform float _InvertZ;
		uniform float4 _LevelsZ;
		uniform float _IntensityMaskZ;
		uniform float _Transformation;
		uniform sampler2D _TransformationMask;
		uniform float4 _TransformationMask_ST;
		uniform sampler2D _NormalMap;
		uniform sampler2D _NormalMap2;
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform sampler2D _Albedo2;
		uniform float4 _Albedo2_ST;
		uniform float4 _Color;
		uniform float4 _Color2;
		uniform float _MaskInfo;
		uniform sampler2D _Specular;
		uniform float4 _Specular_ST;
		uniform sampler2D _Specular2;
		uniform float4 _Specular2_ST;
		uniform float4 _Spec;
		uniform float _powerFresnel;
		uniform float _IntensityFresnel;
		uniform sampler2D _AO;
		uniform float4 _AO_ST;
		uniform sampler2D _AO2;
		uniform float4 _AO2_ST;
		uniform half _Translucency;
		uniform half _TransNormalDistortion;
		uniform half _TransScattering;
		uniform half _TransDirect;
		uniform half _TransAmbient;
		uniform half _TransShadow;
		uniform float4 _Translucency_;
		uniform sampler2D _SSS_thickness;
		uniform float4 _SSS_thickness_ST;
		uniform sampler2D _SSS_thickness2;
		uniform float4 _SSS_thickness2_ST;
		uniform float _uv3;
		uniform float _uv2;
		uniform float _uv1;
		uniform float _uv0;
		uniform float _uv3Mask;
		uniform float _uv2Mask;
		uniform float _uv1Mask;
		uniform float _uv0Mask;
		uniform float _uv3Model2;
		uniform float _uv2Model2;
		uniform float _uv1Model2;
		uniform float _uv0Model2;  
				uniform float _TillingMask; 
				uniform float _IntensityMask;
				uniform float _MaxIntensityMask;
		uniform float _OffsetMask;
		uniform float __scale;
		float3 RigPos;
		float3 posObj;
		uint id_;
		 #ifdef SHADER_API_D3D11
		StructuredBuffer<float3> _vert_;
		StructuredBuffer<float3> _normal_;
		#endif

		float4 CalculateContrast( float contrastValue, float4 colorTarget )
		{
			float t = 0.5 * ( 1.0 - contrastValue );
			return mul( float4x4( contrastValue,0,0,t, 0,contrastValue,0,t, 0,0,contrastValue,t, 0,0,0,1 ), colorTarget );
		}

		void vertexDataFunc( inout appdata__ v , out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertex3Pos = v.vertex.xyz;
			//float2 uvMask_218 = lerp(lerp(lerp(lerp(v.texcoord.xy,v.texcoord.xy,_uv0Mask),v.texcoord1.xy,_uv1Mask),v.texcoord2.xy,_uv2Mask),v.texcoord3.xy,_uv3Mask);
			float4 temp_cast_0 = ((_LevelsX.z + (lerp(ase_vertex3Pos.x,( ase_vertex3Pos.x * -1.0 ),_InvertX) - _LevelsX.x) * (_LevelsX.w - _LevelsX.z) / (_LevelsX.y - _LevelsX.x))).xxxx;
			float clampResult107 = clamp( (CalculateContrast(_ContrastMaskX,temp_cast_0)).r , 0.0 , 1.0 );
			float4 temp_cast_1 = ((_LevelsY.z + (lerp(ase_vertex3Pos.y,( ase_vertex3Pos.y * -1.0 ),_InvertY) - _LevelsY.x) * (_LevelsY.w - _LevelsY.z) / (_LevelsY.y - _LevelsY.x))).xxxx;
			float clampResult137 = clamp( (CalculateContrast(_ContrastMaskY,temp_cast_1)).g , 0.0 , 1.0 );
			float4 temp_cast_2 = ((_LevelsZ.z + (lerp(ase_vertex3Pos.z,( ase_vertex3Pos.z * -1.0 ),_InvertZ) - _LevelsZ.x) * (_LevelsZ.w - _LevelsZ.z) / (_LevelsZ.y - _LevelsZ.x))).xxxx;
			float clampResult158 = clamp( (CalculateContrast(_ContrastMaskZ,temp_cast_2)).g , 0.0 , 1.0 );
			float clampResult175 = clamp( ( ( clampResult107 * _IntensityMaskX ) + ( clampResult137 * _IntensityMaskY ) + ( clampResult158 * _IntensityMaskZ ) ) , 0.0 , 1.0 );
			float2 temp_cast_3 = (_TillingMask).xx;
			float2 temp_cast_4 = (_OffsetMask).xx;
			float2 uv_TexCoord231 = v.texcoord.xy * temp_cast_3 + temp_cast_4;
			float2 temp_cast_5 = (_TillingMask).xx;
			float2 temp_cast_6 = (_OffsetMask).xx;
			float2 uv2_TexCoord232 = v.texcoord1.xy * temp_cast_5 + temp_cast_6;
			float2 temp_cast_7 = (_TillingMask).xx;
			float2 temp_cast_8 = (_OffsetMask).xx;
			float2 uv3_TexCoord233 = v.texcoord2.xy * temp_cast_7 + temp_cast_8;
			float2 temp_cast_9 = (_TillingMask).xx;
			float2 uv4_TexCoord230 = v.texcoord3.xy * temp_cast_9;
			float2 uvMask_218 = lerp(lerp(lerp(lerp(uv_TexCoord231,uv_TexCoord231,_uv0Mask),uv2_TexCoord232,_uv1Mask),uv3_TexCoord233,_uv2Mask),uv4_TexCoord230,_uv3Mask);
			float2 uv_TransformationMask = v.texcoord * _TransformationMask_ST.xy + _TransformationMask_ST.zw;
			//float temp_output_93_0 = ( ( clampResult175 * _Transformation ) * tex2Dlod( _TransformationMask, float4( uv_TransformationMask, 0, 0.0) ).r );
			float mask__;
			mask__ = clamp( tex2Dlod( _TransformationMask, float4( uvMask_218, 0, 0.0) ).r * _IntensityMask,0,_MaxIntensityMask);
			float temp_output_93_0 = ( ( clampResult175 * _Transformation ) * mask__ );
			float3 vvvv = 0;
			float3 nn = 0;
			float3 delta_;
			delta_ = posObj;
			delta_ = RigPos - delta_;
			#ifdef SHADER_API_D3D11
			vvvv =  _vert_[v.id] - delta_;
			nn = _normal_[v.id];
			#endif
			float3 vvvvv = lerp(0,vvvv,__scale);
			float3 lerpResult86 = lerp( ase_vertex3Pos ,vvvvv, temp_output_93_0);
			v.vertex.xyz = lerpResult86;

			float3 ase_vertexNormal = v.normal.xyz;
			float3 lerpResult169 = lerp( ase_vertexNormal , nn , temp_output_93_0);
			v.normal = lerpResult169;
		}

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
			float3 trans_sss = _Translucency;
			#if UNITY_COLORSPACE_GAMMA
            trans_sss = trans_sss * 0.7;
            #endif
			half4 c = half4( s.Albedo * translucency * trans_sss, 0 );

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
			float3 tex2DNode7 = UnpackNormal( tex2D( _NormalMap, uvDefault214 ) );
			float2 uvDefault2217 = lerp(lerp(lerp(lerp(i.uv_texcoord,i.uv_texcoord,_uv0Model2),i.uv2_texcoord2,_uv1Model2),i.uv3_texcoord3,_uv2Model2),i.uv4_texcoord4,_uv3Model2);
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float4 temp_cast_0 = ((_LevelsX.z + (lerp(ase_vertex3Pos.x,( ase_vertex3Pos.x * -1.0 ),_InvertX) - _LevelsX.x) * (_LevelsX.w - _LevelsX.z) / (_LevelsX.y - _LevelsX.x))).xxxx;
			float clampResult107 = clamp( (CalculateContrast(_ContrastMaskX,temp_cast_0)).r , 0.0 , 1.0 );
			float4 temp_cast_1 = ((_LevelsY.z + (lerp(ase_vertex3Pos.y,( ase_vertex3Pos.y * -1.0 ),_InvertY) - _LevelsY.x) * (_LevelsY.w - _LevelsY.z) / (_LevelsY.y - _LevelsY.x))).xxxx;
			float clampResult137 = clamp( (CalculateContrast(_ContrastMaskY,temp_cast_1)).g , 0.0 , 1.0 );
			float4 temp_cast_2 = ((_LevelsZ.z + (lerp(ase_vertex3Pos.z,( ase_vertex3Pos.z * -1.0 ),_InvertZ) - _LevelsZ.x) * (_LevelsZ.w - _LevelsZ.z) / (_LevelsZ.y - _LevelsZ.x))).xxxx;
			float clampResult158 = clamp( (CalculateContrast(_ContrastMaskZ,temp_cast_2)).g , 0.0 , 1.0 );
			float clampResult175 = clamp( ( ( clampResult107 * _IntensityMaskX ) + ( clampResult137 * _IntensityMaskY ) + ( clampResult158 * _IntensityMaskZ ) ) , 0.0 , 1.0 );
			float2 temp_cast_3 = (_TillingMask).xx;
			float2 temp_cast_4 = (_OffsetMask).xx;
			float2 uv_TexCoord231 = i.uv_texcoord * temp_cast_3 + temp_cast_4;
			float2 temp_cast_5 = (_TillingMask).xx;
			float2 temp_cast_6 = (_OffsetMask).xx;
			float2 uv2_TexCoord232 = i.uv2_texcoord2 * temp_cast_5 + temp_cast_6;
			float2 temp_cast_7 = (_TillingMask).xx;
			float2 temp_cast_8 = (_OffsetMask).xx;
			float2 uv3_TexCoord233 = i.uv3_texcoord3 * temp_cast_7 + temp_cast_8;
			float2 temp_cast_9 = (_TillingMask).xx;
			float2 uv4_TexCoord230 = i.uv4_texcoord4 * temp_cast_9;

			float2 uvMask_218 = lerp(lerp(lerp(lerp(uv_TexCoord231,uv_TexCoord231,_uv0Mask),uv2_TexCoord232,_uv1Mask),uv3_TexCoord233,_uv2Mask),uv4_TexCoord230,_uv3Mask);
							float mask__;
			mask__ = clamp ( tex2D( _TransformationMask, uvMask_218 ).r * _IntensityMask,0,_MaxIntensityMask);
			float temp_output_93_0 = ( ( clampResult175 * _Transformation ) * mask__ );
			float mask177 = temp_output_93_0;
			float3 lerpResult184 = lerp( tex2DNode7 , UnpackNormal( tex2D( _NormalMap2, uvDefault2217 ) ) , mask177);
			o.Normal = lerpResult184;
			float4 lerpResult174 = lerp( tex2D( _Albedo, uvDefault214 ) , tex2D( _Albedo2, uvDefault2217 ) , mask177);
			float4 temp_output_52_0 = ( lerpResult174 * lerp(_Color,_Color2, mask177));
			o.Albedo = temp_output_52_0.rgb;
			float3 temp_cast_11 = (lerp(0.0,mask177,_MaskInfo)).xxx;
			o.Emission = temp_cast_11;
			float4 tex2DNode58 = tex2D( _Specular, uvDefault214 );
			float4 tex2DNode180 = tex2D( _Specular2, uvDefault2217 );
			float4 lerpResult182 = lerp( tex2DNode58 , tex2DNode180 , mask177);
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float fresnelNDotV61 = dot( WorldNormalVector( i , tex2DNode7 ), ase_worldViewDir );
			float fresnelNode61 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNDotV61, _powerFresnel ) );
			float clampResult164 = clamp( fresnelNode61 , 0.0 , 1.0 );
			float lerpResult166 = lerp( 1.0 , clampResult164 , _IntensityFresnel);
			o.Specular = ( lerpResult182 * _Spec * lerpResult166 ).rgb;
			float lerpResult187 = lerp( tex2DNode58.a , tex2DNode180.a , mask177);
			o.Smoothness = ( lerpResult187 * _Spec.a );
			float lerpResult189 = lerp( tex2D( _AO, uvDefault214 ).r , tex2D( _AO2, uvDefault2217 ).r , mask177);
			o.Occlusion = lerpResult189;
			float4 lerpResult80 = lerp( temp_output_52_0 , _Translucency_ , _Translucency_.a);
			float3 lerpResult192 = lerp( tex2D( _SSS_thickness, uvDefault214 ) , tex2D( _SSS_thickness2, uvDefault2217 ) , mask177);
			#if UNITY_COLORSPACE_GAMMA
            lerpResult192 = GammaToLinearSpace(lerpResult192);
            #endif
			float clampResult82 = clamp( lerpResult189 , 0.3 , 1.0 );
			float clampResult85 = clamp( ( clampResult82 + 0.4 ) , 0.3 , 1.0 ); 
			o.Translucency = ( lerpResult80 * lerpResult192 * clampResult85 ).rgb;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardSpecularCustom keepalpha fullforwardshadows exclude_path:deferred vertex:vertexDataFunc 

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
			v2f vert(  appdata__ v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
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
}