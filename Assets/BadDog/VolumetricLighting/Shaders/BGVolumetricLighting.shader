Shader "BadDog/BGVolumetricLighting"
{
    Properties
    {
        _MainTex ("", 2D) = "" {}
    }

    CGINCLUDE

		#include "UnityCG.cginc"
        #include "BGVolumetricLighting.cginc"

    ENDCG

    SubShader
    {
        ZTest Always Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
                #pragma vertex VertDefault
                #pragma fragment FragPrefilter
            ENDCG
        }

        Pass
        {
            CGPROGRAM
                #pragma vertex VertDefault
                #pragma fragment FragRadialBlur
            ENDCG
        }

        Pass
        {
            CGPROGRAM
                #pragma vertex VertDefault
                #pragma fragment FragComposite
            ENDCG
        }

		Pass
        {
            CGPROGRAM
                #pragma vertex VertDefault
                #pragma fragment FragDebug
            ENDCG
        }
    }
}
