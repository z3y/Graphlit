// open lit

#include "Main Light.hlsl"

void ShadeSH9ToonDouble(float3 lightDirection, out float3 shMax, out float3 shMin)
{
    #if !defined(LIGHTMAP_ON)
        float3 N = lightDirection * 0.666666;
        float4 vB = N.xyzz * N.yzzx;
        // L0 L2
        float3 res = float3(unity_SHAr.w,unity_SHAg.w,unity_SHAb.w);
        res.r += dot(unity_SHBr, vB);
        res.g += dot(unity_SHBg, vB);
        res.b += dot(unity_SHBb, vB);
        res += unity_SHC.rgb * (N.x * N.x - N.y * N.y);
        // L1
        float3 l1;
        l1.r = dot(unity_SHAr.rgb, N);
        l1.g = dot(unity_SHAg.rgb, N);
        l1.b = dot(unity_SHAb.rgb, N);
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


void ComputeLightDirection(float3 mainLightDir, float3 mainLightCol, out float3 lightDirection, out float3 lightDirectionForSH9)
{
    float3 mainDir = mainLightDir * OpenLitLuminance(mainLightCol);
    #if !defined(LIGHTMAP_ON)
        float3 sh9Dir = unity_SHAr.xyz * 0.333333 + unity_SHAg.xyz * 0.333333 + unity_SHAb.xyz * 0.333333;
        float3 sh9DirAbs = float3(sh9Dir.x, abs(sh9Dir.y), sh9Dir.z);
		UNITY_FLATTEN
        if (!any(unity_SHC.xyz))
        {
            sh9Dir = 0;
            sh9DirAbs = 0;
        }
    #else
        float3 sh9Dir = 0;
        float3 sh9DirAbs = 0;
    #endif

    lightDirection = normalize(sh9DirAbs + mainDir);
    // lightDirectionForSH9 = sh9Dir + mainDir;
    lightDirectionForSH9 = sh9Dir;
    lightDirectionForSH9 = dot(lightDirectionForSH9, lightDirectionForSH9) < 0.000001 ? 0 : normalize(lightDirectionForSH9);
}

void FlatLightNode(float4 ShadowCoord, float2 LightmapUV, float3 PositionWS, out float3 Color, out float3 Direction, out float DistanceAttenuation, out float ShadowAttenuation, out float3 shMin, out float3 shMax, float Min = 0, float Max = 1, float MonochromeLighting = 0, bool applyShadow = true)
{
    shMin = 0;
    shMax = 0;
	#ifdef PREVIEW
		Color = 1.0;
		Direction = normalize(float3(1,1,1));
        DistanceAttenuation = 1;
        ShadowAttenuation = 1;
	#else
		float3 lightCol;
		float3 lightDir;
        MainLightData(ShadowCoord, LightmapUV, PositionWS, lightCol, lightDir, DistanceAttenuation, ShadowAttenuation);
		float3 lightDirectionForSH9;
		ComputeLightDirection(lightDir, lightCol, Direction, lightDirectionForSH9);

		#if defined(UNITY_PASS_FORWARDBASE) || defined(OUTLINE_PASS)
			ShadeSH9ToonDouble(lightDirectionForSH9, shMax, shMin);
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
