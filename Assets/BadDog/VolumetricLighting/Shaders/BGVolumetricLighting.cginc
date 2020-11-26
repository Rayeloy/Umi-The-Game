#ifndef __BGVOLUMETRICLIGHTING__
#define __BGVOLUMETRICLIGHTING__

uniform sampler _MainTex;
uniform float4 _MainTex_ST;
uniform float4 _MainTex_TexelSize;

sampler _VolumetricLightingTex;

uniform sampler2D _CameraDepthTexture;

float2 _MainLightViewPosition;
half _DepthThreshold;
half _LightingRadius;
half _SampleNum;
half _SampleDensity;
half _LightingSampleWeight;
half _LightingDecay;
half _LightingIntensity;
half4 _LightingColor;

struct VertexInput
{
    float4 vertex : POSITION;
    float4 texcoord : TEXCOORD0;
};

struct VertexOutput
{
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
};

VertexOutput VertDefault(VertexInput v)
{
    VertexOutput o;
    o.pos = UnityObjectToClipPos(v.vertex);
    o.uv = v.texcoord.xy;
#if UNITY_UV_STARTS_AT_TOP
	if (_MainTex_TexelSize.y < 0)
		o.uv.y = 1.0 - o.uv.y;
#endif
    return o;
}

half4 FragPrefilter(VertexOutput i) : SV_Target
{
	float2 uv = i.uv;

	half3 mainColor = tex2D(_MainTex, uv).rgb;

	// luminance = max(r, g, b)
	half lum = max(mainColor.x, max(mainColor.y, mainColor.z));

	// depth
	float depth = tex2D(_CameraDepthTexture, uv).r;
	depth = Linear01Depth(depth);

	half isSky = depth > _DepthThreshold;
	depth = lerp(0, 1, isSky);
	lum *= isSky;

	float2 distance = _MainLightViewPosition.xy - i.uv;
	distance.y *= _ScreenParams.y / _ScreenParams.x;
	float distanceDecay = saturate(_LightingRadius - length(distance));

	depth *= distanceDecay;

	return half4(depth, depth, depth, lum);
}

half4 FragRadialBlur(VertexOutput i) : SV_Target
{
	float2 uv = i.uv;

	half3 finalColor = half3(0, 0, 0);

	float2 uvDelta = uv - _MainLightViewPosition;
	uvDelta *= 1.0f / _SampleNum * _SampleDensity;

	half lightingDecay = 1.0f;

	for (int i = 0; i < _SampleNum; i++)
	{
		uv -= uvDelta;

		half4 color = tex2D(_MainTex, uv);
		color.rgb *= lightingDecay * (_LightingSampleWeight/ _SampleNum) * color.a;

		finalColor.rgb += color.rgb;

		lightingDecay *= _LightingDecay;
	}

	return float4(finalColor.rgb, 1);
}

half4 FragComposite(VertexOutput i) : SV_Target
{
	half3 mainColor = tex2D(_MainTex, i.uv).rgb;

	half3 volumetricColor = tex2D(_VolumetricLightingTex, i.uv).rgb;
	volumetricColor = volumetricColor * _LightingColor.rgb * _LightingIntensity;

#ifdef UNITY_COLORSPACE_GAMMA
	mainColor = GammaToLinearSpace(mainColor);
	volumetricColor = GammaToLinearSpace(volumetricColor);
#endif

	half3 finalColor = mainColor + volumetricColor;

#ifdef UNITY_COLORSPACE_GAMMA
	finalColor = LinearToGammaSpace(finalColor);
#endif

	return half4(finalColor, 1);
}

half4 FragDebug(VertexOutput i) : SV_Target
{
	return half4(0, 0, 0, 1);
}

#endif // __BGVOLUMETRICLIGHTING__
