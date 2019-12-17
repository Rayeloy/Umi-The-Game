
Shader "Transformation/SSSLWRP"
{
	Properties
	{
		_Albedo("Albedo", 2D) = "white" {}
		_Albedo2("Albedo2", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,0)
		[NoScaleOffset]_NormalMap2("NormalMap2", 2D) = "bump" {}
		[NoScaleOffset]_NormalMap("NormalMap", 2D) = "bump" {}
		_Specular("Specular(rgb)Gloss(a)", 2D) = "white" {}
		_Specular2("Specular2(rgb)Gloss2(a)", 2D) = "white" {}
		_Spec("Spec", Color) = (0,0,0,0)
		_AO2("AO2", 2D) = "white" {}
		_AO("AO", 2D) = "white" {}
		_SSS_thickness2("SSS_thickness2", 2D) = "white" {}
		_SSS_thickness("SSS_thickness", 2D) = "white" {}
		_TransformationMask("TransformationMask", 2D) = "white" {}
		_Transformation("Transformation", Range( 0 , 1)) = 0
		_ContrastMaskZ("ContrastMaskZ", Float) = 0
		_ContrastMaskY("ContrastMaskY", Float) = 0
		_ContrastMaskX("ContrastMaskX", Float) = 0
		_LevelsX("LevelsX", Vector) = (0,0,0,0)
		_LevelsZ("LevelsZ", Vector) = (0,0,0,0)
		_LevelsY("LevelsY", Vector) = (0,0,0,0)
		_IntensityMaskX("IntensityMaskX", Float) = 0
		_IntensityMaskY("IntensityMaskY", Float) = 0
		_IntensityMaskZ("IntensityMaskZ", Float) = 0
		__scale("scale", Float) = 1

		

		
		[Toggle]_uv0Mask("uv0Mask", Float) = 0
		[Toggle]_uv1Mask("uv1Mask", Float) = 1
		[Toggle]_uv2Mask("uv2Mask", Float) = 0
		[Toggle]_uv0Model2("uv0Model2", Float) = 0
		[Toggle]_uv1Model2("uv1Model2", Float) = 0
		[Toggle]_uv2Model2("uv2Model2", Float) = 0
		
		[Toggle]_uv0("uv0", Float) = 0
		[Toggle]_uv1("uv1", Float) = 0
		[Toggle]_uv2("uv2", Float) = 0
		
		
		
		_TillingMask("TillingMask", Float) = 1
		_OffsetMask("OffsetMask", Float) = 0
		[Toggle]_MaskInfo("MaskInfo", Float) = 0
		_SSScolor1("SSScolor1", Color) = (0,0,0,0)
		_SSScolor2("SSScolor2", Color) = (0,0,0,0)
		_SSScolor3("SSScolor3", Color) = (0,0,0,0)
		_LvlSSS1("LvlSSS1", Range( 0 , 1)) = 0
		_LvlSSS2("LvlSSS2", Range( 0 , 1)) = 0
		_IntensitySSS("IntensitySSS", Range( 0 , 2)) = 0
		_DepthSSS("DepthSSS", Range( 0 , 2)) = 0
		_Transmission("Transmission", Range( 0 , 1)) = 0
		_powerFresnel("powerFresnel", Float) = 0
		_IntensityFresnel("IntensityFresnel", Range( 0 , 1)) = 0
	}

	SubShader
	{
		Tags { "RenderPipeline"="LightweightPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
		
		Cull Back
		HLSLINCLUDE
		#pragma target 3.0
		ENDHLSL
		
		Pass
		{
			
			Tags { "LightMode"="LightweightForward" }
			Name "Base"
			Blend One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA
            
        	HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            

        	// -------------------------------------
            // Lightweight Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
            
        	// -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex vert
        	#pragma fragment frag

        	#define _NORMALMAP 1
        	#define _SPECULAR_SETUP 1


        	#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
        	#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Lighting.hlsl"
        	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
        	#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/ShaderGraphFunctions.hlsl"
			
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
			uniform float _uv2Mask;
			uniform float _uv1Mask;
			uniform float _uv0Mask;
			uniform float _TillingMask;
			uniform float _OffsetMask;
			uniform sampler2D _Albedo;
			uniform float _uv2;
			uniform float _uv1;
			uniform float _uv0;
			uniform sampler2D _Albedo2;
			uniform float _uv2Model2;
			uniform float _uv1Model2;
			uniform float _uv0Model2;
			uniform float4 _Color;
			uniform sampler2D _NormalMap;
			uniform sampler2D _NormalMap2;
			uniform sampler2D _SSS_thickness;
			uniform sampler2D _SSS_thickness2;
			uniform float _DepthSSS;
			uniform float4 _SSScolor1;
			uniform float4 _SSScolor2;
			uniform float4 _SSScolor3;
			uniform float _LvlSSS1;
			uniform float _LvlSSS2;
			uniform float _IntensitySSS;
			uniform sampler2D _AO;
			uniform sampler2D _AO2;
			uniform float _MaskInfo;
			uniform sampler2D _Specular;
			uniform sampler2D _Specular2;
			uniform float4 _Spec;
			uniform float _powerFresnel;
			uniform float _IntensityFresnel;
            uniform float _Transmission;

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
			float3 SSS2_g1( float3 Normal , float Depth , float3 ColorSSS1 , float3 ColorSSS2 , float3 ColorSSS3 , float LevelColor1 , float LevelColor2 , float Transmission , float Intensity , float3 lightDirection , out float3 Out_ )
			{
				float remap1 = ((Depth)*4.0+-3.0);
				float nulll = 0.0;
				float M_ = (-1.0);
				float _Gradient = saturate((nulll + ( ((dot(Normal,lightDirection)*(1.0 - remap1)) - M_) * (1.0 - nulll) ) / (2.0 - M_)));
				float Gr = (1.0 - (_Gradient*0.9+0.1));
				float3 _ColorSSS_ = lerp(lerp(ColorSSS1.rgb,ColorSSS2.rgb,saturate((0.0 + ( (_Gradient - 1.0) * (1.0 - 0.0) ) / (LevelColor1 - 1.0)))),ColorSSS3.rgb,saturate((0.0 + ( (_Gradient - LevelColor2) * (1.0 - 0.0) ) / (0.0 - LevelColor2))));
				 float3 FinalResult = (lerp((_ColorSSS_*saturate(lerp(_Gradient,(_Gradient*Gr),(_Gradient*(Transmission+0.3))))),_ColorSSS_,Transmission)*(Intensity*3.0));
				return Out_ = FinalResult;
			}
			
					
			struct GraphVertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord1 : TEXCOORD1;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				uint id : SV_VertexID;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct GraphVertexOutput
			{
				float4 clipPos					: SV_POSITION;
				float4 lightmapUVOrVertexSH		: TEXCOORD0;
				half4 fogFactorAndVertexLight	: TEXCOORD1; 
				float4 shadowCoord				: TEXCOORD2;
				float4 tSpace0					: TEXCOORD3;
				float4 tSpace1					: TEXCOORD4;
				float4 tSpace2					: TEXCOORD5;
				float3 WorldSpaceViewDirection	: TEXCOORD6;
				float4 ase_texcoord7 : TEXCOORD7;
				float4 ase_texcoord8 : TEXCOORD8;
				float4 ase_texcoord9 : TEXCOORD9;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            	UNITY_VERTEX_OUTPUT_STEREO
			};

			GraphVertexOutput vert (GraphVertexInput v)
			{
		        GraphVertexOutput o = (GraphVertexOutput)0;
		
		        UNITY_SETUP_INSTANCE_ID(v);
		    	UNITY_TRANSFER_INSTANCE_ID(v, o);
		
				float4 temp_cast_0 = ((_LevelsX.z + (lerp(v.vertex.xyz.x,( v.vertex.xyz.x * -1.0 ),_InvertX) - _LevelsX.x) * (_LevelsX.w - _LevelsX.z) / (_LevelsX.y - _LevelsX.x))).xxxx;
				float clampResult107 = clamp( (CalculateContrast(_ContrastMaskX,temp_cast_0)).r , 0.0 , 1.0 );
				float4 temp_cast_1 = ((_LevelsY.z + (lerp(v.vertex.xyz.y,( v.vertex.xyz.y * -1.0 ),_InvertY) - _LevelsY.x) * (_LevelsY.w - _LevelsY.z) / (_LevelsY.y - _LevelsY.x))).xxxx;
				float clampResult137 = clamp( (CalculateContrast(_ContrastMaskY,temp_cast_1)).g , 0.0 , 1.0 );
				float4 temp_cast_2 = ((_LevelsZ.z + (lerp(v.vertex.xyz.z,( v.vertex.xyz.z * -1.0 ),_InvertZ) - _LevelsZ.x) * (_LevelsZ.w - _LevelsZ.z) / (_LevelsZ.y - _LevelsZ.x))).xxxx;
				float clampResult158 = clamp( (CalculateContrast(_ContrastMaskZ,temp_cast_2)).g , 0.0 , 1.0 );
				float clampResult175 = clamp( ( ( clampResult107 * _IntensityMaskX ) + ( clampResult137 * _IntensityMaskY ) + ( clampResult158 * _IntensityMaskZ ) ) , 0.0 , 1.0 );
				float2 temp_cast_3 = (_TillingMask).xx;
				float2 temp_cast_4 = (_OffsetMask).xx;
				float2 uv231 = v.ase_texcoord.xy * temp_cast_3 + temp_cast_4;
				float2 temp_cast_5 = (_TillingMask).xx;
				float2 temp_cast_6 = (_OffsetMask).xx;
				float2 uv232 = v.texcoord1.xyzw.xy * temp_cast_5 + temp_cast_6;
				float2 temp_cast_7 = (_TillingMask).xx;
				float2 temp_cast_8 = (_OffsetMask).xx;
				float2 uv233 = v.ase_texcoord2.xy * temp_cast_7 + temp_cast_8;
				float2 temp_cast_9 = (_TillingMask).xx;
				float2 uvMask_218 = lerp(lerp(lerp(uv231,uv231,_uv0Mask),uv232,_uv1Mask),uv233,_uv2Mask);
				float mask177 = ( ( clampResult175 * _Transformation ) * tex2Dlod( _TransformationMask, float4( uvMask_218, 0, 0.0) ).r );
				
				
				
				
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
			float3 lerpResult86 = lerp( v.vertex.xyz , vvvvv , mask177);
			float3 lerpResult169 = lerp( v.ase_normal , nn , mask177);

				o.ase_texcoord7.xy = v.ase_texcoord.xy;
				o.ase_texcoord7.zw = v.texcoord1.xyzw.xy;
				o.ase_texcoord8.xy = v.ase_texcoord2.xy;
				o.ase_texcoord8.zw = v.ase_texcoord3.xy;
				o.ase_texcoord9 = v.vertex;
				v.vertex.xyz = lerpResult86;
				v.ase_normal = lerpResult169;

				float3 lwWNormal = TransformObjectToWorldNormal(v.ase_normal);
				float3 lwWorldPos = TransformObjectToWorld(v.vertex.xyz);
				float3 lwWTangent = TransformObjectToWorldDir(v.ase_tangent.xyz);
				float3 lwWBinormal = normalize(cross(lwWNormal, lwWTangent) * v.ase_tangent.w);
				o.tSpace0 = float4(lwWTangent.x, lwWBinormal.x, lwWNormal.x, lwWorldPos.x);
				o.tSpace1 = float4(lwWTangent.y, lwWBinormal.y, lwWNormal.y, lwWorldPos.y);
				o.tSpace2 = float4(lwWTangent.z, lwWBinormal.z, lwWNormal.z, lwWorldPos.z);
				float4 clipPos = TransformWorldToHClip(lwWorldPos);

				VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
                
         		// We either sample GI from lightmap or SH.
        	    // Lightmap UV and vertex SH coefficients use the same interpolator ("float2 lightmapUV" for lightmap or "half3 vertexSH" for SH)
                // see DECLARE_LIGHTMAP_OR_SH macro.
        	    // The following funcions initialize the correct variable with correct data
        	    OUTPUT_LIGHTMAP_UV(v.texcoord1, unity_LightmapST, o.lightmapUVOrVertexSH.xy);
        	    OUTPUT_SH(lwWNormal, o.lightmapUVOrVertexSH.xyz);

        	    half3 vertexLight = VertexLighting(vertexInput.positionWS, lwWNormal);
        	    half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
        	    o.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
        	    o.clipPos = vertexInput.positionCS;

        	#ifdef _MAIN_LIGHT_SHADOWS
        		o.shadowCoord = GetShadowCoord(vertexInput);
        	#endif
        		return o;
			}
		
			half4 frag (GraphVertexOutput IN ) : SV_Target
		    {
		    	UNITY_SETUP_INSTANCE_ID(IN);
		
				float3 WorldSpaceNormal = normalize(float3(IN.tSpace0.z,IN.tSpace1.z,IN.tSpace2.z));
				float3 WorldSpaceTangent = float3(IN.tSpace0.x,IN.tSpace1.x,IN.tSpace2.x);
				float3 WorldSpaceBiTangent = float3(IN.tSpace0.y,IN.tSpace1.y,IN.tSpace2.y);
				float3 WorldSpacePosition = float3(IN.tSpace0.w,IN.tSpace1.w,IN.tSpace2.w);
				float3 WorldSpaceViewDirection = SafeNormalize( _WorldSpaceCameraPos.xyz  - WorldSpacePosition );

				float2 uv198 = IN.ase_texcoord7.xy * float2( 1,1 ) + float2( 0,0 );
				float2 uv199 = IN.ase_texcoord7.zw * float2( 1,1 ) + float2( 0,0 );
				float2 uv200 = IN.ase_texcoord8.xy * float2( 1,1 ) + float2( 0,0 );
				float2 uv201 = IN.ase_texcoord8.zw * float2( 1,1 ) + float2( 0,0 );
				float2 uvDefault214 = lerp(lerp(lerp(uv198,uv198,_uv0),uv199,_uv1),uv200,_uv2);
				float2 uvDefault2217 = lerp(lerp(lerp(uv198,uv198,_uv0Model2),uv199,_uv1Model2),uv200,_uv2Model2);
				float4 temp_cast_0 = ((_LevelsX.z + (lerp(IN.ase_texcoord9.xyz.x,( IN.ase_texcoord9.xyz.x * -1.0 ),_InvertX) - _LevelsX.x) * (_LevelsX.w - _LevelsX.z) / (_LevelsX.y - _LevelsX.x))).xxxx;
				float clampResult107 = clamp( (CalculateContrast(_ContrastMaskX,temp_cast_0)).r , 0.0 , 1.0 );
				float4 temp_cast_1 = ((_LevelsY.z + (lerp(IN.ase_texcoord9.xyz.y,( IN.ase_texcoord9.xyz.y * -1.0 ),_InvertY) - _LevelsY.x) * (_LevelsY.w - _LevelsY.z) / (_LevelsY.y - _LevelsY.x))).xxxx;
				float clampResult137 = clamp( (CalculateContrast(_ContrastMaskY,temp_cast_1)).g , 0.0 , 1.0 );
				float4 temp_cast_2 = ((_LevelsZ.z + (lerp(IN.ase_texcoord9.xyz.z,( IN.ase_texcoord9.xyz.z * -1.0 ),_InvertZ) - _LevelsZ.x) * (_LevelsZ.w - _LevelsZ.z) / (_LevelsZ.y - _LevelsZ.x))).xxxx;
				float clampResult158 = clamp( (CalculateContrast(_ContrastMaskZ,temp_cast_2)).g , 0.0 , 1.0 );
				float clampResult175 = clamp( ( ( clampResult107 * _IntensityMaskX ) + ( clampResult137 * _IntensityMaskY ) + ( clampResult158 * _IntensityMaskZ ) ) , 0.0 , 1.0 );
				float2 temp_cast_3 = (_TillingMask).xx;
				float2 temp_cast_4 = (_OffsetMask).xx;
				float2 uv231 = IN.ase_texcoord7.xy * temp_cast_3 + temp_cast_4;
				float2 temp_cast_5 = (_TillingMask).xx;
				float2 temp_cast_6 = (_OffsetMask).xx;
				float2 uv232 = IN.ase_texcoord7.zw * temp_cast_5 + temp_cast_6;
				float2 temp_cast_7 = (_TillingMask).xx;
				float2 temp_cast_8 = (_OffsetMask).xx;
				float2 uv233 = IN.ase_texcoord8.xy * temp_cast_7 + temp_cast_8;
				float2 temp_cast_9 = (_TillingMask).xx;
				float2 uv230 = IN.ase_texcoord8.zw * temp_cast_9 + float2( 0,0 );
				float2 uvMask_218 = lerp(lerp(lerp(uv231,uv231,_uv0Mask),uv232,_uv1Mask),uv233,_uv2Mask);
				float mask177 = ( ( clampResult175 * _Transformation ) * tex2D( _TransformationMask, uvMask_218 ).r );
				float4 lerpResult174 = lerp( tex2D( _Albedo, uvDefault214 ) , tex2D( _Albedo2, uvDefault2217 ) , mask177);
				float4 Albedo__255 = ( lerpResult174 * _Color );
				float3 tex2DNode7 = UnpackNormalmapRGorAG( tex2D( _NormalMap, uvDefault214 ), 1.0f );
				float3 lerpResult184 = lerp( tex2DNode7 , UnpackNormalmapRGorAG( tex2D( _NormalMap2, uvDefault2217 ), 1.0f ) , mask177);
				float3 Normalki253 = lerpResult184;
				float3 tanToWorld0 = float3( WorldSpaceTangent.x, WorldSpaceBiTangent.x, WorldSpaceNormal.x );
				float3 tanToWorld1 = float3( WorldSpaceTangent.y, WorldSpaceBiTangent.y, WorldSpaceNormal.y );
				float3 tanToWorld2 = float3( WorldSpaceTangent.z, WorldSpaceBiTangent.z, WorldSpaceNormal.z );
				float3 tanNormal267 = Normalki253;
				float3 worldNormal267 = float3(dot(tanToWorld0,tanNormal267), dot(tanToWorld1,tanNormal267), dot(tanToWorld2,tanNormal267));
				float3 Normal2_g1 = worldNormal267;
				float lerpResult192 = lerp( tex2D( _SSS_thickness, uvDefault214 ).r , tex2D( _SSS_thickness2, uvDefault2217 ).r , mask177);
				float clampResult274 = clamp( ( lerpResult192 * _DepthSSS ) , 0.0 , 1.0 );
				float Depth2_g1 = clampResult274;
				float3 ColorSSS12_g1 = ( Albedo__255 * _SSScolor1 ).rgb;
				float3 ColorSSS22_g1 = ( Albedo__255 * _SSScolor2 ).rgb;
				float3 ColorSSS32_g1 = ( Albedo__255 * _SSScolor3 ).rgb;
				float LevelColor12_g1 = _LvlSSS1;
				float LevelColor22_g1 = _LvlSSS2;
				float lerpResult189 = lerp( tex2D( _AO, uvDefault214 ).r , tex2D( _AO2, uvDefault2217 ).r , mask177);
				float clampResult82 = clamp( ( lerpResult189 + 0.3 ) , 0.0 , 1.0 );
				float clampResult276 = clamp( ( lerpResult192 * _IntensitySSS * clampResult82 ) , 0.0 , 1.0 );
				float Intensity2_g1 = clampResult276;
				float3 lightDirection2_g1 = _MainLightPosition.xyz;
				float3 Out_2_g1 = float3( 0,0,0 );
				float3 localSSS2_g1 = SSS2_g1( Normal2_g1 , Depth2_g1 , ColorSSS12_g1 , ColorSSS22_g1 , ColorSSS32_g1 , LevelColor12_g1 , LevelColor22_g1 , _Transmission , Intensity2_g1 , lightDirection2_g1 , Out_2_g1 );
				float3 temp_output_266_0 = Out_2_g1;
				float4 lerpResult282 = Albedo__255 + lerp( ( float4( temp_output_266_0 , 0.0 ) ) , float4( 0,0,0,0 ) , 0.5);
				
				float3 lerpResult281 = lerp( temp_output_266_0 , float3( 0,0,0 ) , 0.5);
				
				float4 tex2DNode58 = tex2D( _Specular, uvDefault214 );
				float4 tex2DNode180 = tex2D( _Specular2, uvDefault2217 );
				float4 lerpResult182 = lerp( tex2DNode58 , tex2DNode180 , mask177);
				float3 tanNormal61 = tex2DNode7;
				float fresnelNdotV61 = dot( float3(dot(tanToWorld0,tanNormal61), dot(tanToWorld1,tanNormal61), dot(tanToWorld2,tanNormal61)), WorldSpaceViewDirection );
				float fresnelNode61 = ( 0.0 + 1.0 * pow( abs(1.0 - fresnelNdotV61), _powerFresnel ) );
				float clampResult164 = clamp( fresnelNode61 , 0.0 , 1.0 );
				float lerpResult166 = lerp( 1.0 , clampResult164 , _IntensityFresnel);
				float4 SpecOut__g264 = ( lerpResult182 * _Spec * lerpResult166 );
				
				float lerpResult187 = lerp( tex2DNode58.a , tex2DNode180.a , mask177);
				float Gloss259 = ( lerpResult187 * _Spec.a );
				
				
		        float3 Albedo = lerpResult282.rgb;
				float3 Normal = Normalki253;
				float3 Emission = ( temp_output_266_0 + lerp(0.0,mask177,_MaskInfo) );
				float3 Specular = SpecOut__g264.rgb;
				float Metallic = 0;
				float Smoothness = Gloss259;
				float Occlusion = lerpResult189;
				float Alpha = 1;
				float AlphaClipThreshold = 0;
		
				InputData inputData;
        		inputData.positionWS = WorldSpacePosition;

        #ifdef _NORMALMAP
        	    inputData.normalWS = normalize(TransformTangentToWorld(Normal, half3x3(WorldSpaceTangent, WorldSpaceBiTangent, WorldSpaceNormal)));
        #else
            #if !SHADER_HINT_NICE_QUALITY
                inputData.normalWS = WorldSpaceNormal;
            #else
        	    inputData.normalWS = normalize(WorldSpaceNormal);
            #endif
        #endif

        #if !SHADER_HINT_NICE_QUALITY
        	    // viewDirection should be normalized here, but we avoid doing it as it's close enough and we save some ALU.
        	    inputData.viewDirectionWS = WorldSpaceViewDirection;
        #else
        	    inputData.viewDirectionWS = normalize(WorldSpaceViewDirection);
        #endif

        	    inputData.shadowCoord = IN.shadowCoord;

        	    inputData.fogCoord = IN.fogFactorAndVertexLight.x;
        	    inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;
        	    inputData.bakedGI = SAMPLE_GI(IN.lightmapUVOrVertexSH.xy, IN.lightmapUVOrVertexSH.xyz, inputData.normalWS);

        		half4 color = LightweightFragmentPBR(
        			inputData, 
        			Albedo, 
        			Metallic, 
        			Specular, 
        			Smoothness, 
        			Occlusion, 
        			Emission, 
        			Alpha);

			#ifdef TERRAIN_SPLAT_ADDPASS
				color.rgb = MixFogColor(color.rgb, half3( 0, 0, 0 ), IN.fogFactorAndVertexLight.x );
			#else
				color.rgb = MixFog(color.rgb, IN.fogFactorAndVertexLight.x);
			#endif

        #if _AlphaClip
        		clip(Alpha - AlphaClipThreshold);
        #endif

		#if ASE_LW_FINAL_COLOR_ALPHA_MULTIPLY
				color.rgb *= color.a;
		#endif
        		return color;
		    }
			ENDHLSL
		}

		
		Pass
		{
			
			Name "ShadowCaster"
			Tags { "LightMode"="ShadowCaster" }

			ZWrite On
			ZTest LEqual

			HLSLPROGRAM
		    #pragma prefer_hlslcc gles
		
			#pragma multi_compile_instancing
		
		    #pragma vertex vert
			#pragma fragment frag
		
			#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Lighting.hlsl"
			#define _NORMALMAP 1

			//uniform float4 _ShadowBias;
			uniform float3 _LightDirection;
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
			uniform float _uv2Mask;
			uniform float _uv1Mask;
			uniform float _uv0Mask;
			uniform float _TillingMask;
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
					
			struct GraphVertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				uint id : SV_VertexID;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct GraphVertexOutput
			{
				float4 clipPos : SV_POSITION;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			GraphVertexOutput vert (GraphVertexInput v)
			{
				GraphVertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				float4 temp_cast_0 = ((_LevelsX.z + (lerp(v.vertex.xyz.x,( v.vertex.xyz.x * -1.0 ),_InvertX) - _LevelsX.x) * (_LevelsX.w - _LevelsX.z) / (_LevelsX.y - _LevelsX.x))).xxxx;
				float clampResult107 = clamp( (CalculateContrast(_ContrastMaskX,temp_cast_0)).r , 0.0 , 1.0 );
				float4 temp_cast_1 = ((_LevelsY.z + (lerp(v.vertex.xyz.y,( v.vertex.xyz.y * -1.0 ),_InvertY) - _LevelsY.x) * (_LevelsY.w - _LevelsY.z) / (_LevelsY.y - _LevelsY.x))).xxxx;
				float clampResult137 = clamp( (CalculateContrast(_ContrastMaskY,temp_cast_1)).g , 0.0 , 1.0 );
				float4 temp_cast_2 = ((_LevelsZ.z + (lerp(v.vertex.xyz.z,( v.vertex.xyz.z * -1.0 ),_InvertZ) - _LevelsZ.x) * (_LevelsZ.w - _LevelsZ.z) / (_LevelsZ.y - _LevelsZ.x))).xxxx;
				float clampResult158 = clamp( (CalculateContrast(_ContrastMaskZ,temp_cast_2)).g , 0.0 , 1.0 );
				float clampResult175 = clamp( ( ( clampResult107 * _IntensityMaskX ) + ( clampResult137 * _IntensityMaskY ) + ( clampResult158 * _IntensityMaskZ ) ) , 0.0 , 1.0 );
				float2 temp_cast_3 = (_TillingMask).xx;
				float2 temp_cast_4 = (_OffsetMask).xx;
				float2 uv231 = v.ase_texcoord.xy * temp_cast_3 + temp_cast_4;
				float2 temp_cast_5 = (_TillingMask).xx;
				float2 temp_cast_6 = (_OffsetMask).xx;
				float2 uv232 = v.ase_texcoord1.xy * temp_cast_5 + temp_cast_6;
				float2 temp_cast_7 = (_TillingMask).xx;
				float2 temp_cast_8 = (_OffsetMask).xx;
				float2 uv233 = v.ase_texcoord2.xy * temp_cast_7 + temp_cast_8;
				float2 temp_cast_9 = (_TillingMask).xx;
				float2 uvMask_218 = lerp(lerp(lerp(uv231,uv231,_uv0Mask),uv232,_uv1Mask),uv233,_uv2Mask);
				float mask177 = ( ( clampResult175 * _Transformation ) * tex2Dlod( _TransformationMask, float4( uvMask_218, 0, 0.0) ).r );

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
			float3 lerpResult86 = lerp( v.vertex.xyz , vvvvv , mask177);
			float3 lerpResult169 = lerp( v.ase_normal , nn , mask177);

				v.vertex.xyz = lerpResult86;
				v.ase_normal = lerpResult169;

				float3 positionWS = TransformObjectToWorld(v.vertex.xyz);
				float3 normalWS = TransformObjectToWorldDir(v.ase_normal);

				float invNdotL = 1.0 - saturate(dot(_LightDirection, normalWS));
				float scale = invNdotL * _ShadowBias.y;

				positionWS = normalWS * scale.xxx + positionWS;
				float4 clipPos = TransformWorldToHClip(positionWS);

				clipPos.z += _ShadowBias.x;
				#if UNITY_REVERSED_Z
					clipPos.z = min(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
				#else
					clipPos.z = max(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
				#endif
				o.clipPos = clipPos;
				return o;
			}
		
			half4 frag (GraphVertexOutput IN ) : SV_Target
		    {
		    	UNITY_SETUP_INSTANCE_ID(IN);

				

				float Alpha = 1;
				float AlphaClipThreshold = AlphaClipThreshold;
				
				#if _AlphaClip
					clip(Alpha - AlphaClipThreshold);
				#endif
				return Alpha;
		    }
			ENDHLSL
		}
		
		
		Pass
		{
			
			Name "DepthOnly"
			Tags { "LightMode"="DepthOnly" }

			ZWrite On
			ColorMask 0

			HLSLPROGRAM
			#pragma prefer_hlslcc gles
    
			#pragma multi_compile_instancing

			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Lighting.hlsl"
			#define _NORMALMAP 1

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
			uniform float _uv2Mask;
			uniform float _uv1Mask;
			uniform float _uv0Mask;
			uniform float _TillingMask;
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

			struct GraphVertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				uint id : SV_VertexID;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct GraphVertexOutput
			{
				float4 clipPos : SV_POSITION;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			GraphVertexOutput vert (GraphVertexInput v)
			{
				GraphVertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				float4 temp_cast_0 = ((_LevelsX.z + (lerp(v.vertex.xyz.x,( v.vertex.xyz.x * -1.0 ),_InvertX) - _LevelsX.x) * (_LevelsX.w - _LevelsX.z) / (_LevelsX.y - _LevelsX.x))).xxxx;
				float clampResult107 = clamp( (CalculateContrast(_ContrastMaskX,temp_cast_0)).r , 0.0 , 1.0 );
				float4 temp_cast_1 = ((_LevelsY.z + (lerp(v.vertex.xyz.y,( v.vertex.xyz.y * -1.0 ),_InvertY) - _LevelsY.x) * (_LevelsY.w - _LevelsY.z) / (_LevelsY.y - _LevelsY.x))).xxxx;
				float clampResult137 = clamp( (CalculateContrast(_ContrastMaskY,temp_cast_1)).g , 0.0 , 1.0 );
				float4 temp_cast_2 = ((_LevelsZ.z + (lerp(v.vertex.xyz.z,( v.vertex.xyz.z * -1.0 ),_InvertZ) - _LevelsZ.x) * (_LevelsZ.w - _LevelsZ.z) / (_LevelsZ.y - _LevelsZ.x))).xxxx;
				float clampResult158 = clamp( (CalculateContrast(_ContrastMaskZ,temp_cast_2)).g , 0.0 , 1.0 );
				float clampResult175 = clamp( ( ( clampResult107 * _IntensityMaskX ) + ( clampResult137 * _IntensityMaskY ) + ( clampResult158 * _IntensityMaskZ ) ) , 0.0 , 1.0 );
				float2 temp_cast_3 = (_TillingMask).xx;
				float2 temp_cast_4 = (_OffsetMask).xx;
				float2 uv231 = v.ase_texcoord.xy * temp_cast_3 + temp_cast_4;
				float2 temp_cast_5 = (_TillingMask).xx;
				float2 temp_cast_6 = (_OffsetMask).xx;
				float2 uv232 = v.ase_texcoord1.xy * temp_cast_5 + temp_cast_6;
				float2 temp_cast_7 = (_TillingMask).xx;
				float2 temp_cast_8 = (_OffsetMask).xx;
				float2 uv233 = v.ase_texcoord2.xy * temp_cast_7 + temp_cast_8;
				float2 temp_cast_9 = (_TillingMask).xx;
				float2 uvMask_218 = lerp(lerp(lerp(uv231,uv231,_uv0Mask),uv232,_uv1Mask),uv233,_uv2Mask);
				float mask177 = ( ( clampResult175 * _Transformation ) * tex2Dlod( _TransformationMask, float4( uvMask_218, 0, 0.0) ).r );
				
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
			float3 lerpResult86 = lerp( v.vertex.xyz , vvvvv , mask177);
			float3 lerpResult169 = lerp( v.ase_normal , nn , mask177);

				v.vertex.xyz = lerpResult86;
				v.ase_normal = lerpResult169;
				o.clipPos = TransformObjectToHClip(v.vertex.xyz);
				return o;
			}

			half4 frag (GraphVertexOutput IN ) : SV_Target
		    {
		    	UNITY_SETUP_INSTANCE_ID(IN);

				

				float Alpha = 1;
				float AlphaClipThreshold = AlphaClipThreshold;
				
				#if _AlphaClip
					clip(Alpha - AlphaClipThreshold);
				#endif
				return Alpha;
		    }
			ENDHLSL
		}
		
		
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
