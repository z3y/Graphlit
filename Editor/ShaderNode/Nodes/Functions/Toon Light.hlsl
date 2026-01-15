// open lit

#include "Main Light.hlsl"

#if defined(UNIVERSAL_FORWARD) || defined(LIGHTPROBE_SH)
#define UNITY_SHOULD_SAMPLE_SH 1
#endif


#define OPENLIT_FALLBACK_DIRECTION  float4(0.001,0.002,0.001,0)
void ShadeSH9ToonDouble(float3 lightDirection, out float3 shMax, out float3 shMin, half3 L1r, half3 L1g, half3 L1b, half3 L0)
{
    #if !defined(LIGHTMAP_ON) && UNITY_SHOULD_SAMPLE_SH
        float3 N = lightDirection * 0.666666;
        float4 vB = N.xyzz * N.yzzx;
        // L0 L2
        float3 res = L0;
        res.r += dot(unity_SHBr, vB);
        res.g += dot(unity_SHBg, vB);
        res.b += dot(unity_SHBb, vB);
        res += unity_SHC.rgb * (N.x * N.x - N.y * N.y);
        // L1
        float3 l1;
        l1.r = dot(L1r.rgb, N);
        l1.g = dot(L1g.rgb, N);
        l1.b = dot(L1b.rgb, N);
        shMax = res + l1;
        shMin = res - l1;
        #if defined(UNITY_COLORSPACE_GAMMA)
            shMax = OpenLitLinearToSRGB(shMax);
            shMin = OpenLitLinearToSRGB(shMin);
        #endif
    #else
        shMax = 0.0;
        shMin = 0.0;
    #endif
}

float OpenLitLuminance(float3 rgb)
{
    #if defined(UNITY_COLORSPACE_GAMMA)
        return dot(rgb, float3(0.22, 0.707, 0.071));
    #else
        return dot(rgb, float3(0.0396819152, 0.458021790, 0.00609653955));
    #endif
}
float OpenLitGray(float3 rgb)
{
    return dot(rgb, float3(1.0/3.0, 1.0/3.0, 1.0/3.0));
}

float3 ComputeCustomLightDirection(float4 lightDirectionOverride)
{
    float3 customDir = length(lightDirectionOverride.xyz) * normalize(mul((float3x3)UNITY_MATRIX_M, lightDirectionOverride.xyz));
    return lightDirectionOverride.w ? customDir : lightDirectionOverride.xyz;
}

void ComputeLightDirection(float3 mainLightDir, float3 mainLightCol, out float3 lightDirection, out float3 lightDirectionForSH9,
    half3 L1r, half3 L1g, half3 L1b)
{
    float3 mainDir = mainLightDir * OpenLitLuminance(mainLightCol);
    #if !defined(LIGHTMAP_ON) && UNITY_SHOULD_SAMPLE_SH
        float3 sh9Dir = unity_SHAr.xyz * 0.333333 + unity_SHAg.xyz * 0.333333 + unity_SHAb.xyz * 0.333333;
        float3 sh9DirAbs = float3(sh9Dir.x, abs(sh9Dir.y), sh9Dir.z);
    #else
        float3 sh9Dir = 0;
        float3 sh9DirAbs = 0;
    #endif
    float3 customDir = ComputeCustomLightDirection(OPENLIT_FALLBACK_DIRECTION);

    lightDirection = normalize(sh9DirAbs + mainDir + customDir);
    lightDirectionForSH9 = sh9Dir + mainDir;
    // lightDirectionForSH9 = sh9Dir;
    lightDirectionForSH9 = dot(lightDirectionForSH9, lightDirectionForSH9) < 0.000001 ? 0 : normalize(lightDirectionForSH9);
}

void FlatLightNode(float4 ShadowCoord, float4 LightmapUV, float3 PositionWS, out float3 Color, out float3 Direction, out float DistanceAttenuation, out float ShadowAttenuation, out float3 shMin, out float3 shMax, float Min = 0, float Max = 1, float MonochromeLighting = 0, bool applyShadow = true)
{
    shMin = 0;
    shMax = 0;
	#if defined(PREVIEW) || defined(LIGHTMAP_ON)
		Color = 1.0;
		Direction = normalize(float3(1,1,0));
        DistanceAttenuation = 1;
        ShadowAttenuation = 1;
	#else

        half3 L0 = float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
        half3 L1r = unity_SHAr.rgb;
        half3 L1g = unity_SHAg.rgb;
        half3 L1b = unity_SHAb.rgb;
    #ifdef _VRC_LIGHTVOLUMES
        LightVolumeSH(PositionWS, L0, L1r, L1g, L1b);
    #endif
    
		float3 lightCol;
		float3 lightDir;
        MainLightData(ShadowCoord, LightmapUV.xy, PositionWS, lightCol, lightDir, DistanceAttenuation, ShadowAttenuation);
		float3 lightDirectionForSH9;
		ComputeLightDirection(lightDir, lightCol, Direction, lightDirectionForSH9, L1r, L1g, L1b);

		#if defined(UNITY_PASS_FORWARDBASE) || defined(OUTLINE_PASS) || defined(UNIVERSAL_FORWARD)
			ShadeSH9ToonDouble(lightDirectionForSH9, shMax, shMin, L1r, L1g, L1b, L0);
		#endif

        if (applyShadow)
        {
            lightCol *= DistanceAttenuation * ShadowAttenuation;
        }
        else
        {
            lightCol *= DistanceAttenuation;
        }

		Color = shMax + (lightCol * sqrt(0.5));

        half lightRenorm = max(max(Color.r, Color.g), Color.b);
        if (lightRenorm > Max)
        {
            Color /= lightRenorm / Max;
        }

		Color = min(Max, Color);
		Color = max(Min, Color);
        Color = lerp(Color, OpenLitGray(Color), MonochromeLighting);
	#endif
}
