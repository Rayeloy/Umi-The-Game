Shader "CorvoShader/SimpleWaterAusfA" 
{
    Properties {
		_Color ("Color", Color) = (0, 0, 0, 0.5)
		[NoScaleOffset]
        _MainTex ( "Water BASE", 2D ) = "white" {}
        _TextureSize ( "Texture Size", Range (0, 100000) ) = 1000

        _WatSpeed ( "Water Speed", Range (0, 10) ) = 0.5
        _WatRot ( "Water Direction", Range (0, 360) ) = 0

		[NoScaleOffset] [Normal]
        _refMap ( "Reflection MAP", 2D ) = "bump" {}
		_Glossiness ("Reflection", Range(0,1)) = 0.9
    }
    SubShader {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "IgnoreProjector"="True" }
		Blend SrcAlpha OneMinusSrcAlpha
		//Lighting Off//bho
		//ZTest GEqual//bho
		Cull Off
        LOD 200

	    CGPROGRAM
	    #pragma surface surf Standard alpha fullforwardshadows

	 
		fixed4 _Color;
	    sampler2D _MainTex;
		float _TextureSize;
	    sampler2D _refMap;
	    half _Glossiness;
	    float _WatSpeed;
	    float _WatRot;

	 
	    struct Input 
	    {
	        float2 uv_MainTex;
	        float2 uv_refMap;
	    };
 
        void surf( Input IN, inout SurfaceOutputStandard o ) 
        {
            fixed2 scrollMain = IN.uv_MainTex;
            fixed2 scrollRef1 = IN.uv_refMap;
            fixed2 scrollRef2 = IN.uv_refMap;
			float _WatSpeedx=_WatSpeed*500/_TextureSize;
			fixed x1s = sin(_WatRot*0.0174533)* _Time.y*_WatSpeedx/100 ;//onda
            fixed y1s = cos(_WatRot*0.0174533)* _Time.y*_WatSpeedx/100 ;
            fixed refX1 = sin((_WatRot+25)*0.0174533) * _Time.y*_WatSpeedx;//riflesso
            fixed refY1 = cos((_WatRot+25)*0.0174533) * _Time.y*_WatSpeedx;
            fixed refX2 = sin((_WatRot-25)*0.0174533) * _Time.y*_WatSpeedx;//riflesso
            fixed refY2 = cos((_WatRot-25)*0.0174533) * _Time.y*_WatSpeedx;
            scrollMain += fixed2( x1s*0.01, y1s*0.01 );
            scrollRef1 += fixed2( refX1*0.00003, refY1*0.00003 );
            scrollRef2 += fixed2( refX2*0.00003, refY2*0.00003 );
            half4 c = tex2D( _MainTex, scrollMain*_TextureSize );
			//fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Normal = UnpackNormal (tex2D (_refMap, scrollRef1*_TextureSize))+ UnpackNormal (tex2D (_refMap, scrollRef2*_TextureSize));
            o.Alpha = _Color.a;
			o.Smoothness = _Glossiness;
			o.Emission =_Color.rgb;
        }

        ENDCG

    }
    FallBack "Diffuse"
}