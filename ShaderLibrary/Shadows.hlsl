#pragma once

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
// #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Shadow/ShadowSamplingTent.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GlobalSamplers.hlsl"

TEXTURE2D_X(_ShadowMapTexture);
SAMPLER_CMP(sampler_LinearClampCompare);
float4 _ShadowOffsets[4];
float4 _ShadowMapTexture_TexelSize;

float4 TransformWorldToShadowCoord(float3 positionWS)
{
#if defined(SHADOWS_SCREEN)// && !defined(_SURFACE_TYPE_TRANSPARENT)
    float4 shadowCoord = float4(ComputeNormalizedDeviceCoordinatesWithZ(positionWS, GetWorldToHClipMatrix()), 1.0);
#else
    #ifdef UNITY_NO_SCREENSPACE_SHADOWS
        // half cascadeIndex = ComputeCascadeIndex(positionWS);
    #else
    #endif
        half cascadeIndex = half(0.0);

    float4 shadowCoord = float4(mul(unity_WorldToShadow[cascadeIndex], float4(positionWS, 1.0)).xyz, 0.0);
#endif
    return shadowCoord;
}

half SampleScreenSpaceShadowmap(float4 shadowCoord)
{
    shadowCoord.xy /= max(0.00001, shadowCoord.w); // Prevent division by zero.

    // The stereo transform has to happen after the manual perspective divide
    shadowCoord.xy = UnityStereoTransformScreenSpaceTex(shadowCoord.xy);

#if defined(USING_STEREO_MATRICES)
    half attenuation = SAMPLE_TEXTURE2D_X(_ShadowMapTexture, sampler_PointClamp, shadowCoord.xy).x;
#else
    half attenuation = half(SAMPLE_TEXTURE2D(_ShadowMapTexture, sampler_PointClamp, shadowCoord.xy).x);
#endif

    return attenuation;
}

bool IsDirectionalLight()
{
    return !_WorldSpaceLightPos0.w;
}

float3 ApplyShadowBias(float3 positionWS, float3 normalWS, float3 lightDirection)
{

    if (unity_LightShadowBias.z != 0.0)
    {
        float shadowCos = dot(normalWS, lightDirection);
        float shadowSine = sqrt(1-shadowCos*shadowCos);
        float normalBias = unity_LightShadowBias.z * shadowSine;

        // normal bias is negative since we want to apply an inset normal offset
        // positionWS = lightDirection * unity_LightShadowBias.z + positionWS;
        positionWS -= normalWS * normalBias;
    }

    return positionWS;
}

float4 ApplyShadowClamping(float4 positionCS)
{

#if !(defined(SHADOWS_CUBE) && defined(SHADOWS_CUBE_IN_DEPTH_TEX))
    #if defined(UNITY_REVERSED_Z)
        // We use max/min instead of clamp to ensure proper handling of the rare case
        // where both numerator and denominator are zero and the fraction becomes NaN.
        positionCS.z += max(-1, min(unity_LightShadowBias.x / positionCS.w, 0));
    #else
        positionCS.z += saturate(unity_LightShadowBias.x/positionCS.w);
    #endif
#endif

    #if UNITY_REVERSED_Z
        float clamped = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
    #else
        float clamped = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
    #endif

    // The current implementation of vertex clamping in Universal RP is the same as in Unity Built-In RP.
    // We follow the same convention in Universal RP where it's only enabled for Directional Lights
    // (see: Shadows.cpp::RenderShadowMaps() #L2161-L2162)
    // (see: Shadows.cpp::RenderShadowMaps() #L2086-L2102)
    // (see: Shadows.cpp::PrepareStateForShadowMap() #L1685-L1686)
    positionCS.z = lerp(positionCS.z, clamped, unity_LightShadowBias.y);

    return positionCS;
}

// real SampleShadowmapFilteredHighQuality(TEXTURE2D_SHADOW_PARAM(ShadowMap, sampler_ShadowMap), float4 shadowCoord, float4 shadowmapSize)
// {
//     float fetchesWeights[16];
//     float2 fetchesUV[16];
//     SampleShadow_ComputeSamples_Tent_7x7(shadowmapSize, shadowCoord, fetchesWeights, fetchesUV);

//     return fetchesWeights[0] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[0].xy, shadowCoord.z))
//                 + fetchesWeights[1] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[1].xy, shadowCoord.z))
//                 + fetchesWeights[2] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[2].xy, shadowCoord.z))
//                 + fetchesWeights[3] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[3].xy, shadowCoord.z))
//                 + fetchesWeights[4] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[4].xy, shadowCoord.z))
//                 + fetchesWeights[5] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[5].xy, shadowCoord.z))
//                 + fetchesWeights[6] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[6].xy, shadowCoord.z))
//                 + fetchesWeights[7] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[7].xy, shadowCoord.z))
//                 + fetchesWeights[8] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[8].xy, shadowCoord.z))
//                 + fetchesWeights[9] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[9].xy, shadowCoord.z))
//                 + fetchesWeights[10] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[10].xy, shadowCoord.z))
//                 + fetchesWeights[11] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[11].xy, shadowCoord.z))
//                 + fetchesWeights[12] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[12].xy, shadowCoord.z))
//                 + fetchesWeights[13] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[13].xy, shadowCoord.z))
//                 + fetchesWeights[14] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[14].xy, shadowCoord.z))
//                 + fetchesWeights[15] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[15].xy, shadowCoord.z));
// }


float UnityComputeShadowFadeDistance(float3 wpos, float z)
{
    float sphereDist = distance(wpos, unity_ShadowFadeCenterAndType.xyz);
    return lerp(z, sphereDist, unity_ShadowFadeCenterAndType.w);
}

half UnityComputeShadowFade(float fadeDist)
{
    return saturate(fadeDist * _LightShadowData.z + _LightShadowData.w);
}

half GetMainLightShadowFade(float3 positionWS)
{
    float zDist = dot(_WorldSpaceCameraPos - positionWS, UNITY_MATRIX_V[2].xyz);
    float fadeDist = UnityComputeShadowFadeDistance(positionWS, zDist);
    return UnityComputeShadowFade(fadeDist);
}

half UnityMixRealtimeAndBakedShadows(half realtimeShadowAttenuation, half bakedShadowAttenuation, half fade)
{
    #if !defined(SHADOWS_DEPTH) && !defined(SHADOWS_SCREEN) && !defined(SHADOWS_CUBE)
        #if defined(LIGHTMAP_ON) && defined (LIGHTMAP_SHADOW_MIXING) && !defined (SHADOWS_SHADOWMASK)
            //In subtractive mode when there is no shadow we kill the light contribution as direct as been baked in the lightmap.
            return 0.0;
        #else
            return bakedShadowAttenuation;
        #endif
    #endif

    #if defined(LIGHTMAP_SHADOW_MIXING)
        //Subtractive or shadowmask mode
        realtimeShadowAttenuation = saturate(realtimeShadowAttenuation + fade);
        return min(realtimeShadowAttenuation, bakedShadowAttenuation);
    #endif


    //In distance shadowmask or realtime shadow fadeout we lerp toward the baked shadows (bakedShadowAttenuation will be 1 if no baked shadows)
    return lerp(realtimeShadowAttenuation, bakedShadowAttenuation, fade);
}
