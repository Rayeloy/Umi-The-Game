// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "RRF/EyeShader_Model3"
{
	Properties
	{
		_FrontGlow("FrontGlow", Range( 0 , 5)) = 0
		_EyeShadingPower("EyeShadingPower", Range( 0.1 , 2)) = 0.5
		_BackGlow("BackGlow", Range( 0 , 5)) = 0
		[NoScaleOffset]_EyeExtras("EyeExtras", 2D) = "white" {}
		[NoScaleOffset]_IrisExtraDetail("IrisExtraDetail", 2D) = "white" {}
		[NoScaleOffset]_NormalMapBase("NormalMapBase", 2D) = "bump" {}
		[NoScaleOffset]_Sclera_Normal("Sclera_Normal", 2D) = "bump" {}
		_ScleraBumpScale("ScleraBumpScale", Range( 0 , 1)) = 0
		_EyeBallColorGlossA("EyeBallColor-Gloss(A)", Color) = (1,1,1,0.853)
		_IrisBaseColor("IrisBaseColor", Color) = (0.4999702,0.5441177,0.4641004,1)
		_IrisExtraColorAmountA("IrisExtraColor-Amount(A)", Color) = (0.08088237,0.07573904,0.04698314,0.591)
		_EyeVeinColorAmountA("EyeVeinColor-Amount(A)", Color) = (0.375,0,0,0)
		_RingColorAmount("RingColor-Amount", Color) = (0,0,0,0)
		_EyeSize("EyeSize", Range( 0 , 2)) = 1
		_IrisSize("IrisSize", Range( 0 , 10)) = 1
		_LensGloss("LensGloss", Range( 0 , 1)) = 0.98
		_LensPush("LensPush", Range( 0 , 1)) = 0.64
		[NoScaleOffset]_CausticMask("CausticMask", 2D) = "white" {}
		_CausticPower("CausticPower", Range( 0 , 10)) = 17
		_ForceCausticFX("ForceCausticFX", Range( 0 , 1)) = 0
		_PupilSize("PupilSize", Range( 0.001 , 99)) = 70
		_PupilHeight1Width1("Pupil Height>1/Width<1", Range( 0.01 , 10)) = 1
		_PupilSharpness("PupilSharpness", Range( 0.1 , 5)) = 5
		[NoScaleOffset]_ParallaxMask("ParallaxMask", 2D) = "black" {}
		_PushParallaxMask("PushParallaxMask", Range( 0 , 5)) = 1
		_PupilParallaxHeight("PupilParallaxHeight", Range( 0 , 3)) = 2.5
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityStandardUtils.cginc"
		#include "UnityCG.cginc"
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
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float3 viewDir;
			INTERNAL_DATA
			float3 worldPos;
			float3 worldNormal;
		};

		uniform float _ScleraBumpScale;
		uniform sampler2D _Sclera_Normal;
		uniform float _EyeSize;
		uniform sampler2D _NormalMapBase;
		uniform float _LensPush;
		uniform half _PupilSize;
		uniform float _PupilHeight1Width1;
		uniform sampler2D _ParallaxMask;
		uniform float _PushParallaxMask;
		uniform float _PupilParallaxHeight;
		uniform float _PupilSharpness;
		uniform sampler2D _EyeExtras;
		uniform float4 _EyeBallColorGlossA;
		uniform float4 _RingColorAmount;
		uniform float4 _EyeVeinColorAmountA;
		uniform sampler2D _IrisExtraDetail;
		uniform float _IrisSize;
		uniform float4 _IrisExtraColorAmountA;
		uniform float4 _IrisBaseColor;
		uniform sampler2D _CausticMask;
		uniform float _CausticPower;
		uniform float _ForceCausticFX;
		uniform float _BackGlow;
		uniform float _FrontGlow;
		uniform float _EyeShadingPower;
		uniform float _LensGloss;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 temp_cast_0 = (_EyeSize).xx;
			float2 temp_cast_1 = (( ( 1.0 - _EyeSize ) / 2.0 )).xx;
			float2 uv_TexCoord264 = i.uv_texcoord * temp_cast_0 + temp_cast_1;
			float3 lerpResult139 = lerp( float3(0,0,1) , UnpackNormal( tex2D( _NormalMapBase, uv_TexCoord264 ) ) , _LensPush);
			o.Normal = BlendNormals( UnpackScaleNormal( tex2D( _Sclera_Normal, uv_TexCoord264 ), _ScleraBumpScale ) , lerpResult139 );
			float temp_output_151_0 = ( 100.0 - _PupilSize );
			float2 appendResult149 = (float2(( temp_output_151_0 / 2.0 ) , ( temp_output_151_0 / ( _PupilHeight1Width1 * 2.0 ) )));
			float4 appendResult152 = (float4(temp_output_151_0 , ( temp_output_151_0 / _PupilHeight1Width1 ) , 0.0 , 0.0));
			float2 uv_ParallaxMask416 = i.uv_texcoord;
			float4 tex2DNode416 = tex2D( _ParallaxMask, uv_ParallaxMask416 );
			float4 lerpResult418 = lerp( float4( 0,0,0,0 ) , tex2DNode416 , _PushParallaxMask);
			float2 paralaxOffset255 = ParallaxOffset( lerpResult418.r , _PupilParallaxHeight , i.viewDir );
			float2 uv_TexCoord147 = i.uv_texcoord * appendResult152.xy + paralaxOffset255;
			float4 tex2DNode166 = tex2D( _EyeExtras, uv_TexCoord264 );
			float clampResult122 = clamp( ( pow( distance( appendResult149 , uv_TexCoord147 ) , ( _PupilSharpness * 7.0 ) ) + ( 1.0 - tex2DNode166.b ) ) , 0.0 , 1.0 );
			float4 temp_cast_4 = (( tex2DNode166.b * 0.0 )).xxxx;
			float4 lerpResult127 = lerp( _EyeBallColorGlossA , temp_cast_4 , tex2DNode166.b);
			float4 lerpResult184 = lerp( lerpResult127 , ( _RingColorAmount * tex2DNode166.r ) , ( _RingColorAmount.a * tex2DNode166.r ));
			float4 lerpResult177 = lerp( lerpResult184 , ( _EyeVeinColorAmountA * tex2DNode166.g ) , ( ( _EyeVeinColorAmountA.a * 1.5 ) * tex2DNode166.g ));
			float temp_output_323_0 = ( _IrisSize + _EyeSize + ( temp_output_151_0 * 0.017 ) );
			float2 temp_cast_5 = (temp_output_323_0).xx;
			float2 uv_TexCoord190 = i.uv_texcoord * temp_cast_5 + ( ( paralaxOffset255 * float2( 0.15,0.15 ) ) + ( ( 1.0 - temp_output_323_0 ) / 2.0 ) );
			float4 temp_output_322_0 = ( ( ( tex2D( _IrisExtraDetail, uv_TexCoord190 ) * _IrisExtraColorAmountA ) * _IrisExtraColorAmountA.a ) * tex2DNode166.b );
			float4 temp_output_326_0 = ( clampResult122 * ( ( lerpResult184 * lerpResult177 ) + ( temp_output_322_0 + ( tex2DNode166.b * _IrisBaseColor ) ) ) );
			float4 lerpResult442 = lerp( temp_output_326_0 , ( temp_output_326_0 * float4( 0,0,0,0 ) ) , float4( 0,0,0,0 ));
			o.Albedo = ( lerpResult442 + float4( 0,0,0,0 ) ).rgb;
			float2 paralaxOffset411 = ParallaxOffset( tex2DNode416.r , ( _PupilParallaxHeight * 0.03 ) , i.viewDir );
			float2 uv_TexCoord409 = i.uv_texcoord * float2( 0,0 ) + paralaxOffset411;
			float3 ase_worldPos = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = Unity_SafeNormalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float cos373 = cos( ase_worldlightDir.x );
			float sin373 = sin( ase_worldlightDir.x );
			float2 rotator373 = mul( ( uv_TexCoord264 + uv_TexCoord409 ) - float2( 0.5,0.5 ) , float2x2( cos373 , -sin373 , sin373 , cos373 )) + float2( 0.5,0.5 );
			float4 tex2DNode370 = tex2D( _CausticMask, rotator373 );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_vertexNormal = mul( unity_WorldToObject, float4( ase_worldNormal, 0 ) );
			float dotResult375 = dot( ase_worldlightDir , ase_vertexNormal );
			float4 clampResult381 = clamp( ( tex2DNode370 * dotResult375 * _CausticPower ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
			float4 lerpResult398 = lerp( ( temp_output_322_0 * clampResult381 * clampResult122 ) , ( temp_output_322_0 * tex2DNode370 * clampResult122 * _CausticPower ) , _ForceCausticFX);
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float3 objToWorldDir422 = mul( unity_ObjectToWorld, float4( ase_vertex3Pos, 0 ) ).xyz;
			float3 normalizeResult424 = normalize( objToWorldDir422 );
			float dotResult425 = dot( normalizeResult424 , ase_worldlightDir );
			float clampResult448 = clamp( ( _BackGlow * -dotResult425 ) , 0.0 , 1.0 );
			float dotResult433 = dot( ase_worldlightDir , ase_vertexNormal );
			float clampResult478 = clamp( ( ( clampResult122 * 0.0 ) + ( clampResult122 * ( _FrontGlow * dotResult433 ) ) ) , 0.0 , 1.0 );
			float4 transform457 = mul(unity_ObjectToWorld,float4( 0,-3,1,0.78 ));
			float dotResult458 = dot( float4( ase_worldNormal , 0.0 ) , transform457 );
			float clampResult464 = clamp( dotResult458 , 0.0 , 1.0 );
			float4 transform462 = mul(unity_ObjectToWorld,float4( 0,1.2,1,0.78 ));
			float dotResult461 = dot( float4( ase_worldNormal , 0.0 ) , transform462 );
			float clampResult465 = clamp( dotResult461 , 0.0 , 1.0 );
			float3 temp_cast_10 = (pow( ( clampResult464 * clampResult465 ) , _EyeShadingPower )).xxx;
			float temp_output_2_0_g1 = ( _EyeShadingPower * 0.5 );
			float temp_output_3_0_g1 = ( 1.0 - temp_output_2_0_g1 );
			float3 appendResult7_g1 = (float3(temp_output_3_0_g1 , temp_output_3_0_g1 , temp_output_3_0_g1));
			o.Emission = ( ( lerpResult398 + ( clampResult122 * clampResult448 * temp_output_326_0 ) ) + ( float4( ( clampResult478 * ( ( temp_cast_10 * temp_output_2_0_g1 ) + appendResult7_g1 ) ) , 0.0 ) * lerpResult442 ) ).rgb;
			float lerpResult135 = lerp( _EyeBallColorGlossA.a , ( tex2DNode166.b * _LensGloss ) , tex2DNode166.b);
			o.Smoothness = lerpResult135;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows exclude_path:deferred 

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
				surfIN.viewDir = IN.tSpace0.xyz * worldViewDir.x + IN.tSpace1.xyz * worldViewDir.y + IN.tSpace2.xyz * worldViewDir.z;
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
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16100
266;299;1359;744;-3301.696;830.4345;2.975647;True;False
Node;AmplifyShaderEditor.CommentaryNode;164;379.8337,-1375.361;Float;False;2247.17;550.2261;Pupil;17;91;151;156;153;157;148;154;152;149;147;146;155;213;214;285;327;328;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;91;420.0602,-1256.936;Half;False;Property;_PupilSize;PupilSize;22;0;Create;True;0;0;False;0;70;79.7;0.001;99;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;151;760.092,-1282.874;Float;False;2;0;FLOAT;100;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;328;447.5334,-1113.258;Float;False;Constant;_IrisPupilBond;Iris-Pupil-Bond;23;0;Create;True;0;0;False;0;0.017;0.017;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;298;148.7998,-1740.208;Float;False;932.0083;353.6761;Parralax;3;256;255;257;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;327;818.963,-1134.793;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0.01;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;419;-436.7072,-1381.403;Float;False;Property;_PushParallaxMask;PushParallaxMask;26;0;Create;True;0;0;False;0;1;3.84;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;267;-998.581,-330.824;Float;False;Property;_EyeSize;EyeSize;14;0;Create;True;0;0;False;0;1;1.2;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;247;-428.1711,-447.047;Float;False;Property;_IrisSize;IrisSize;15;0;Create;True;0;0;False;0;1;2.33;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;416;-533.8765,-1240.39;Float;True;Property;_ParallaxMask;ParallaxMask;25;1;[NoScaleOffset];Create;True;0;0;False;0;None;451268057d3fa344e8695ec36ec39129;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;257;237.2419,-1664.595;Float;False;Property;_PupilParallaxHeight;PupilParallaxHeight;27;0;Create;True;0;0;False;0;2.5;2.36;0;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;256;219.8662,-1566.438;Float;False;Tangent;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;168;463.7572,-851.6978;Float;False;803.1489;1005.685;Inputs;8;133;158;20;170;182;183;249;278;;1,1,1,1;0;0
Node;AmplifyShaderEditor.LerpOp;418;-126.1631,-1407.255;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;323;142.9483,-376.3304;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ParallaxOffsetHlpNode;255;819.2643,-1557.992;Float;False;3;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.OneMinusNode;249;1000.997,-337.0559;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;218;1286.445,-840.8356;Float;False;1193.157;961.5587;IrisFunk;16;125;190;127;185;187;210;184;175;177;251;261;250;283;284;330;415;;1,1,1,1;0;0
Node;AmplifyShaderEditor.OneMinusNode;266;-747.2682,-134.4663;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;250;1192.376,-329.3226;Float;False;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;284;1278.145,-481.7476;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0.15,0.15;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;265;-569.3465,-76.13261;Float;False;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;283;1331.333,-344.4919;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;264;-360.8761,-214.9127;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;6,6;False;1;FLOAT2;-2.5,-2.5;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;156;408.895,-995.9824;Float;False;Property;_PupilHeight1Width1;Pupil Height>1/Width<1;23;0;Create;True;0;0;False;0;1;1;0.01;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;166;-81.00919,-77.77974;Float;True;Property;_EyeExtras;EyeExtras;4;1;[NoScaleOffset];Create;True;0;0;False;0;d0431c3a16ed8b54c8d648bb79ca09a5;ea1c686f188c8ef46b8c015f1c344441;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;190;1463.338,-280.9104;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;6,6;False;1;FLOAT2;-2.5,-2.5;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleDivideOpNode;153;1003.357,-1256.106;Float;False;2;0;FLOAT;2;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;157;805.5457,-959.7744;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;412;-979.1753,343.7845;Float;False;Tangent;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ColorNode;133;503.652,-774.4395;Float;False;Property;_EyeBallColorGlossA;EyeBallColor-Gloss(A);9;0;Create;True;0;0;False;0;1,1,1,0.853;1,1,1,0.653;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;125;1358.178,-594.572;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;185;1796.143,-284.6462;Float;True;Property;_IrisExtraDetail;IrisExtraDetail;5;1;[NoScaleOffset];Create;True;0;0;False;0;7b7c97e104d9817418725e17f5ca2659;e2d81eed3c8d580428baa06160f91a78;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;414;-903.1498,50.56799;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0.03;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;170;448.6407,-593.486;Float;False;Property;_EyeVeinColorAmountA;EyeVeinColor-Amount(A);12;0;Create;True;0;0;False;0;0.375,0,0,0;0.7941176,0.4846453,0.4846453,0.984;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;152;1194.87,-1274.417;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;148;1108.712,-1104.026;Float;False;2;0;FLOAT;2;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;154;1148.628,-958.1364;Float;False;2;0;FLOAT;2;False;1;FLOAT;4;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;187;1391.817,-115.1497;Float;False;Property;_IrisExtraColorAmountA;IrisExtraColor-Amount(A);11;0;Create;True;0;0;False;0;0.08088237,0.07573904,0.04698314,0.591;0.291955,0.519586,0.5294118,0.641;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;182;795.4716,-780.5615;Float;False;Property;_RingColorAmount;RingColor-Amount;13;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0.359;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;415;1698.05,-533.9419;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;1.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;183;1139.501,-760.5087;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;210;2130.213,-221.2353;Float;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ParallaxOffsetHlpNode;411;-671.7384,310.5547;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;127;1557.454,-785.5184;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;147;1437.897,-1289.529;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;149;1384.686,-1059.491;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;213;1518.967,-962.9574;Float;False;Property;_PupilSharpness;PupilSharpness;24;0;Create;True;0;0;False;0;5;0.64;0.1;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;420;3329.814,1058.677;Float;False;1108.382;385.6998;SimpleSSS;6;425;424;423;422;421;430;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;261;1362.131,-706.0457;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;219;1476.895,184.2316;Float;False;2546.831;732.6423;IrisConeCaustics;14;319;322;376;373;377;375;50;334;399;370;378;381;401;408;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;330;1982.832,-549.2875;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;421;3379.814,1111.412;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;409;-367.9864,306.5428;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;175;1726.928,-615.2676;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DistanceOpNode;146;1746.54,-1220.662;Float;True;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;214;1837.676,-964.7169;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;7;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;251;2415.011,-31.03593;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;2;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;126;1018.012,300.2839;Float;False;Property;_IrisBaseColor;IrisBaseColor;10;0;Create;True;0;0;False;0;0.4999702,0.5441177,0.4641004,1;0.4117647,0.2422563,0.1120242,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;184;1859.063,-780.4681;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;475;3710.559,1729.255;Float;False;1159.451;586.8235;LocalShadowing;11;462;457;459;458;461;465;464;463;467;466;484;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;319;1536.708,239.3736;Float;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;376;1831.96,408.9701;Float;False;True;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TransformDirectionNode;422;3661.038,1108.677;Float;False;Object;World;False;Fast;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;322;3597.196,114.4498;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;177;2241.192,-641.3635;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;410;37.16052,241.9778;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;423;3386.696,1265.376;Float;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;169;2513.791,-819.8705;Float;False;2125.866;921.7941;Mixing;8;26;134;135;103;325;321;326;402;;1,1,1,1;0;0
Node;AmplifyShaderEditor.OneMinusNode;285;2299.325,-942.8307;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;432;3385.13,1552.45;Float;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ObjectToWorldTransfNode;457;3772.921,1938.325;Float;False;1;0;FLOAT4;0,-3,1,0.78;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;155;2066.258,-1227.059;Float;True;2;0;FLOAT;0;False;1;FLOAT;12;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;459;3766.384,1779.255;Float;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ObjectToWorldTransfNode;462;3760.559,2114.078;Float;False;1;0;FLOAT4;0,1.2,1,0.78;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NormalVertexDataNode;377;1848.211,565.2993;Float;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NormalizeNode;424;3943.177,1121.258;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RotatorNode;373;2259.485,334.7047;Float;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;103;3054.176,-727.463;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;286;2529.892,-1107.551;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;458;4107.219,1794.219;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;461;4068.185,2017.373;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;430;3793.697,1412.507;Float;False;Property;_FrontGlow;FrontGlow;1;0;Create;True;0;0;False;0;0;1.63;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;321;2936.587,-539.7786;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DotProductOpNode;433;3674.917,1506.347;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;425;4135.097,1139.121;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;50;2193.552,781.9643;Float;False;Property;_CausticPower;CausticPower;20;0;Create;True;0;0;False;0;17;2.78;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;370;2621.182,407.5727;Float;True;Property;_CausticMask;CausticMask;19;1;[NoScaleOffset];Create;True;0;0;False;0;None;4567a6987e49477429e516f992aac3ab;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DotProductOpNode;375;2336.867,553.9872;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;325;3375.45,-626.8168;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;464;4300.79,1832.641;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;122;2699.702,-1201.836;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;435;4224.534,1487.793;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;465;4308.601,1988.809;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;444;4298.38,927.3215;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;378;3011.71,525.6826;Float;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;467;4080.262,2143.981;Float;False;Property;_EyeShadingPower;EyeShadingPower;2;0;Create;True;0;0;False;0;0.5;0.62;0.1;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;476;4601.449,1254.62;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;463;4533.16,1894.023;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;436;4589.235,954.4244;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;402;3868.071,-423.4704;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;445;4157.314,793.7405;Float;False;Property;_BackGlow;BackGlow;3;0;Create;True;0;0;False;0;0;0.82;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;326;4452.679,-480.1984;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;381;3339.21,516.9429;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;477;4790.12,1098.856;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;466;4690.01,1921.419;Float;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;446;4510.704,768.1705;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;484;4543.276,2184.274;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;441;5201.747,622.5063;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;448;4682.052,680.9326;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;167;3145.671,-1403.067;Float;False;1620.849;536.6165;Normal Maps;7;332;331;139;333;138;140;141;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;334;3659.683,317.7768;Float;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;399;3665.092,504.8685;Float;False;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;478;4994.783,1129.739;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;401;3626.957,720.7513;Float;False;Property;_ForceCausticFX;ForceCausticFX;21;0;Create;True;0;0;False;0;0;0.38;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;483;5106.291,1835.967;Float;False;Lerp White To;-1;;1;047d7c189c36a62438973bad9d37b1c2;0;2;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;138;3195.671,-1285.736;Float;True;Property;_NormalMapBase;NormalMapBase;6;1;[NoScaleOffset];Create;True;0;0;False;0;8ee6d0418eaa08e40ad667b400177c1c;3171a4d91fe9d464ea499c8380d8597c;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;141;3195.745,-1080.986;Float;False;Property;_LensPush;LensPush;18;0;Create;True;0;0;False;0;0.64;0.45;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;20;824.8633,36.09259;Float;False;Property;_LensGloss;LensGloss;17;0;Create;True;0;0;False;0;0.98;0.74;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;140;3535.54,-1353.067;Float;False;Constant;_Vector0;Vector 0;10;0;Create;True;0;0;False;0;0,0,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;333;4012.667,-1289.306;Float;False;Property;_ScleraBumpScale;ScleraBumpScale;8;0;Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;398;4320.071,337.113;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;451;4863.203,465.8748;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;481;5288.86,1235.942;Float;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;442;5263.597,129.8051;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;139;3798.193,-1116.57;Float;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;134;2943.958,-145.0298;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;331;4343.103,-1331.481;Float;True;Property;_Sclera_Normal;Sclera_Normal;7;1;[NoScaleOffset];Create;True;0;0;False;0;None;97ac39d433ae05047abf79173f71d460;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;452;5138.924,389.7706;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;470;5606.975,820.4972;Float;False;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;278;1078.965,-486.7992;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ParallaxOffsetHlpNode;408;1830.874,289.333;Float;False;3;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;486;5976.701,278.3811;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;288;4258.221,502.169;Float;False;Property;_TranslucencyPower;TranslucencyPower;0;0;Create;True;0;0;False;0;0.2;0.263;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;26;2625.04,-36.13949;Float;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.BlendNormalsNode;332;4511.249,-1023.529;Float;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;471;5707.688,657.5266;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;158;803.1615,-84.68832;Float;False;Property;_IrisGlow;IrisGlow;16;0;Create;True;0;0;False;0;0;3.36;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;135;3239.743,-161.6185;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;6638.848,514.3286;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;RRF/EyeShader_Model3;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;ForwardOnly;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;151;1;91;0
WireConnection;327;0;151;0
WireConnection;327;1;328;0
WireConnection;418;1;416;0
WireConnection;418;2;419;0
WireConnection;323;0;247;0
WireConnection;323;1;267;0
WireConnection;323;2;327;0
WireConnection;255;0;418;0
WireConnection;255;1;257;0
WireConnection;255;2;256;0
WireConnection;249;0;323;0
WireConnection;266;0;267;0
WireConnection;250;0;249;0
WireConnection;284;0;255;0
WireConnection;265;0;266;0
WireConnection;283;0;284;0
WireConnection;283;1;250;0
WireConnection;264;0;267;0
WireConnection;264;1;265;0
WireConnection;166;1;264;0
WireConnection;190;0;323;0
WireConnection;190;1;283;0
WireConnection;153;0;151;0
WireConnection;153;1;156;0
WireConnection;157;0;156;0
WireConnection;125;0;166;3
WireConnection;185;1;190;0
WireConnection;414;0;257;0
WireConnection;152;0;151;0
WireConnection;152;1;153;0
WireConnection;148;0;151;0
WireConnection;154;0;151;0
WireConnection;154;1;157;0
WireConnection;415;0;170;4
WireConnection;183;0;182;0
WireConnection;183;1;166;1
WireConnection;210;0;185;0
WireConnection;210;1;187;0
WireConnection;411;0;416;0
WireConnection;411;1;414;0
WireConnection;411;2;412;0
WireConnection;127;0;133;0
WireConnection;127;1;125;0
WireConnection;127;2;166;3
WireConnection;147;0;152;0
WireConnection;147;1;255;0
WireConnection;149;0;148;0
WireConnection;149;1;154;0
WireConnection;261;0;182;4
WireConnection;261;1;166;1
WireConnection;330;0;415;0
WireConnection;330;1;166;2
WireConnection;409;1;411;0
WireConnection;175;0;170;0
WireConnection;175;1;166;2
WireConnection;146;0;149;0
WireConnection;146;1;147;0
WireConnection;214;0;213;0
WireConnection;251;0;210;0
WireConnection;251;1;187;4
WireConnection;184;0;127;0
WireConnection;184;1;183;0
WireConnection;184;2;261;0
WireConnection;319;0;166;3
WireConnection;319;1;126;0
WireConnection;422;0;421;0
WireConnection;322;0;251;0
WireConnection;322;1;166;3
WireConnection;177;0;184;0
WireConnection;177;1;175;0
WireConnection;177;2;330;0
WireConnection;410;0;264;0
WireConnection;410;1;409;0
WireConnection;285;0;166;3
WireConnection;155;0;146;0
WireConnection;155;1;214;0
WireConnection;424;0;422;0
WireConnection;373;0;410;0
WireConnection;373;2;376;1
WireConnection;103;0;184;0
WireConnection;103;1;177;0
WireConnection;286;0;155;0
WireConnection;286;1;285;0
WireConnection;458;0;459;0
WireConnection;458;1;457;0
WireConnection;461;0;459;0
WireConnection;461;1;462;0
WireConnection;321;0;322;0
WireConnection;321;1;319;0
WireConnection;433;0;423;0
WireConnection;433;1;432;0
WireConnection;425;0;424;0
WireConnection;425;1;423;0
WireConnection;370;1;373;0
WireConnection;375;0;376;0
WireConnection;375;1;377;0
WireConnection;325;0;103;0
WireConnection;325;1;321;0
WireConnection;464;0;458;0
WireConnection;122;0;286;0
WireConnection;435;0;430;0
WireConnection;435;1;433;0
WireConnection;465;0;461;0
WireConnection;444;0;425;0
WireConnection;378;0;370;0
WireConnection;378;1;375;0
WireConnection;378;2;50;0
WireConnection;476;0;122;0
WireConnection;476;1;435;0
WireConnection;463;0;464;0
WireConnection;463;1;465;0
WireConnection;436;0;122;0
WireConnection;402;0;325;0
WireConnection;326;0;122;0
WireConnection;326;1;402;0
WireConnection;381;0;378;0
WireConnection;477;0;436;0
WireConnection;477;1;476;0
WireConnection;466;0;463;0
WireConnection;466;1;467;0
WireConnection;446;0;445;0
WireConnection;446;1;444;0
WireConnection;484;0;467;0
WireConnection;441;0;326;0
WireConnection;448;0;446;0
WireConnection;334;0;322;0
WireConnection;334;1;381;0
WireConnection;334;2;122;0
WireConnection;399;0;322;0
WireConnection;399;1;370;0
WireConnection;399;2;122;0
WireConnection;399;3;50;0
WireConnection;478;0;477;0
WireConnection;483;1;466;0
WireConnection;483;2;484;0
WireConnection;138;1;264;0
WireConnection;398;0;334;0
WireConnection;398;1;399;0
WireConnection;398;2;401;0
WireConnection;451;0;122;0
WireConnection;451;1;448;0
WireConnection;451;2;326;0
WireConnection;481;0;478;0
WireConnection;481;1;483;0
WireConnection;442;0;326;0
WireConnection;442;1;441;0
WireConnection;139;0;140;0
WireConnection;139;1;138;0
WireConnection;139;2;141;0
WireConnection;134;0;166;3
WireConnection;134;1;20;0
WireConnection;331;1;264;0
WireConnection;331;5;333;0
WireConnection;452;0;398;0
WireConnection;452;1;451;0
WireConnection;470;0;481;0
WireConnection;470;1;442;0
WireConnection;278;0;255;0
WireConnection;278;1;323;0
WireConnection;408;0;264;0
WireConnection;486;0;442;0
WireConnection;332;0;331;0
WireConnection;332;1;139;0
WireConnection;471;0;452;0
WireConnection;471;1;470;0
WireConnection;135;0;133;4
WireConnection;135;1;134;0
WireConnection;135;2;166;3
WireConnection;0;0;486;0
WireConnection;0;1;332;0
WireConnection;0;2;471;0
WireConnection;0;4;135;0
ASEEND*/
//CHKSM=553582220E4A5A91EA55642EA471FF680BCB3005