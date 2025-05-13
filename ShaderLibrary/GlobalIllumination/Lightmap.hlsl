#pragma once

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/BSDF.hlsl"

#ifdef _BICUBIC_LIGHTMAP
#define BICUBIC_LIGHTMAP
#endif
#ifdef _BICUBIC_SHADOWMASK
#define BICUBIC_SHADOWMASK
#endif

// #define BICUBIC_DIRECTIONAL_LIGHTMAP

#ifdef _BAKERY_MONOSH
#define BAKERY_MONOSH
#endif
#ifdef _LIGHTMAPPED_SPECULAR
#define LIGHTMAP_SPECULAR
#endif

void SampleLightmap(out half3 illuminance, out half3 specular, float2 lightmapUV, float3 normalWS, float3 viewDirectionWS, half perceptualRoughness)
{
    illuminance = 0;
    specular = 0;
    half3 f0 = 0.5 * 0.5 * 0.16;

    float4 texelSize;
    unity_Lightmap.GetDimensions(texelSize.x, texelSize.y);
    texelSize.zw = 1.0 / texelSize.xy;

    half4 decodeInstructions = half4(LIGHTMAP_HDR_MULTIPLIER, LIGHTMAP_HDR_EXPONENT, 0.0h, 0.0h);
    #ifdef BICUBIC_LIGHTMAP
        half4 lightmap = SampleTexture2DBicubic(TEXTURE2D_ARGS(unity_Lightmap, sampler_BilinearClamp), lightmapUV, texelSize, 1.0, 0);
    #else
        half4 lightmap = SAMPLE_TEXTURE2D_LOD(unity_Lightmap, sampler_BilinearClamp, lightmapUV, 0);
    #endif
    illuminance = DecodeLightmap(lightmap, decodeInstructions);

    #ifdef DIRLIGHTMAP_COMBINED

        #ifdef BICUBIC_DIRECTIONAL_LIGHTMAP
            half4 directionalLightmap = SampleTexture2DBicubic(TEXTURE2D_ARGS(unity_LightmapInd, sampler_BilinearClamp), lightmapUV, texelSize, 1.0, 0);
        #else
            half4 directionalLightmap = SAMPLE_TEXTURE2D_LOD(unity_LightmapInd, sampler_BilinearClamp, lightmapUV, 0);
        #endif

        #ifdef BAKERY_MONOSH
            half3 L0 = illuminance;
            half3 nL1 = directionalLightmap.rgb * 2.0 - 1.0;
            half3 L1x = nL1.x * L0 * 2.0;
            half3 L1y = nL1.y * L0 * 2.0;
            half3 L1z = nL1.z * L0 * 2.0;
            #ifdef BAKERY_SHNONLINEAR
                float lumaL0 = dot(L0, 1);
                float lumaL1x = dot(L1x, 1);
                float lumaL1y = dot(L1y, 1);
                float lumaL1z = dot(L1z, 1);
                float lumaSH = shEvaluateDiffuseL1Geomerics(lumaL0, float3(lumaL1x, lumaL1y, lumaL1z), normalWS);

                half3 sh = L0 + normalWS.x * L1x + normalWS.y * L1y + giInput.normalWS.z * L1z;
                float regularLumaSH = dot(sh, 1);
                sh *= lerp(1, lumaSH / regularLumaSH, saturate(regularLumaSH * 16));
                illuminance = sh;
            #else
                illuminance = L0 + normalWS.x * L1x + normalWS.y * L1y + normalWS.z * L1z;
            #endif

            #ifdef LIGHTMAP_SPECULAR
                float3 dominantDir = nL1;
                float3 directionality = normalize(dominantDir);
                float3 halfVector = SafeNormalize(directionality + viewDirectionWS);
                half NoH = saturate(dot(normalWS, halfVector));
                half spec = D_GGX(NoH, max(perceptualRoughness * perceptualRoughness, HALF_MIN_SQRT));
                half3 sh = L0 + dominantDir.x * L1x + dominantDir.y * L1y + dominantDir.z * L1z;
                half LoH = saturate(dot(directionality, halfVector));
                specular = max(spec * sh, 0.0);
            #endif
        #else
            half halfLambert = dot(normalWS, directionalLightmap.xyz - 0.5) + 0.5;
            illuminance = illuminance * halfLambert / max(1e-4, directionalLightmap.w);
        #endif

    #endif
}

#if UNITY_LIGHT_PROBE_PROXY_VOLUME
half4 LPPV_SampleProbeOcclusion(float3 positionWS)
{
    float transformToLocal = unity_ProbeVolumeParams.y;
    float texelSizeX = unity_ProbeVolumeParams.z;

    //The SH coefficients textures and probe occlusion are packed into 1 atlas.
    //-------------------------
    //| ShR | ShG | ShB | Occ |
    //-------------------------

    float3 position = (transformToLocal == 1.0f) ? mul(unity_ProbeVolumeWorldToObject, float4(positionWS, 1.0)).xyz : positionWS;

    //Get a tex coord between 0 and 1
    float3 uv = (position - unity_ProbeVolumeMin.xyz) * unity_ProbeVolumeSizeInv.xyz;

    // Sample fourth texture in the atlas
    // We need to compute proper U coordinate to sample.
    // Clamp the coordinate otherwize we'll have leaking between ShB coefficients and Probe Occlusion(Occ) info
    uv.x = max(uv.x * 0.25f + 0.75f, 0.75f + 0.5f * texelSizeX);

    return SAMPLE_TEXTURE3D(unity_ProbeVolumeSH, samplerunity_ProbeVolumeSH, uv);
}
#endif

half4 SampleShadowMask(float2 lightmapUV)
{
    #if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
        #ifdef BICUBIC_SHADOWMASK
            float4 texelSize;
            unity_Lightmap.GetDimensions(texelSize.x, texelSize.y);
            texelSize.zw = 1.0 / texelSize.xy;
            half4 rawOcclusionMask = SampleTexture2DBicubic(TEXTURE2D_ARGS(unity_ShadowMask, sampler_BilinearClamp), lightmapUV, texelSize, 1.0, 0);
        #else
            half4 rawOcclusionMask = SAMPLE_TEXTURE2D(unity_ShadowMask, sampler_BilinearClamp, lightmapUV);
        #endif
        return rawOcclusionMask;
    #endif

    #ifdef UNIVERSALRP
        #if !defined(LIGHTMAP_ON)
            return unity_ProbesOcclusion;
        #endif
    #endif

    return 1;
}

#ifndef UNIVERSALRP
half UnitySampleBakedOcclusion(float2 lightmapUV, float3 positionWS)
{
    // #if !(defined(SHADOWS_SHADOWMASK) || defined(LIGHTMAP_SHADOW_MIXING))
    // return 1;
    // #endif
    #if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
        half4 rawOcclusionMask = SampleShadowMask(lightmapUV);
        return saturate(dot(rawOcclusionMask, unity_OcclusionMaskSelector));
    #elif defined(LIGHTPROBE_SH)
        //In forward dynamic objects can only get baked occlusion from LPPV, light probe occlusion is done on the CPU by attenuating the light color.
        half attenuation = 1.0f;
        #if defined(UNITY_INSTANCING_ENABLED) && defined(UNITY_USE_SHCOEFFS_ARRAYS)
            // ...unless we are doing instancing, and the attenuation is packed into SHC array's .w component.
            attenuation = unity_SHC.w;
        #endif

        #if UNITY_LIGHT_PROBE_PROXY_VOLUME && !defined(LIGHTMAP_ON)
            half4 rawOcclusionMask = attenuation.xxxx;
            if (ProbeVolumeEnabled())
            {
                rawOcclusionMask = LPPV_SampleProbeOcclusion(positionWS);
            }
            return saturate(dot(rawOcclusionMask, unity_OcclusionMaskSelector));
        #endif

        return attenuation;
    #endif
    return 1.0;
}
#endif

half3 SubtractMainLightFromLightmap(half3 bakedGI, half3 normalWS, half3 lightColor, float3 lightDirection, half shadowAttenuation)
{
    #if defined(LIGHTMAP_ON) && defined(LIGHTMAP_SHADOW_MIXING)
    // Let's try to make realtime shadows work on a surface, which already contains
    // baked lighting and shadowing from the main sun light.
    half3 shadowColor = unity_ShadowColor.rgb;
    half shadowStrength = _LightShadowData.x;

    // Summary:
    // 1) Calculate possible value in the shadow by subtracting estimated light contribution from the places occluded by realtime shadow:
    //      a) preserves other baked lights and light bounces
    //      b) eliminates shadows on the geometry facing away from the light
    // 2) Clamp against user defined ShadowColor.
    // 3) Pick original lightmap value, if it is the darkest one.


    // 1) Gives good estimate of illumination as if light would've been shadowed during the bake.
    //    Preserves bounce and other baked lights
    //    No shadows on the geometry facing away from the light
    half contributionTerm = saturate(dot(lightDirection, normalWS));
    half3 lambert = lightColor * contributionTerm;
    half3 estimatedLightContributionMaskedByInverseOfShadow = lambert * (1.0 - shadowAttenuation);
    half3 subtractedLightmap = bakedGI - estimatedLightContributionMaskedByInverseOfShadow;

    // 2) Allows user to define overall ambient of the scene and control situation when realtime shadow becomes too dark.
    half3 realtimeShadow = max(subtractedLightmap, shadowColor);
    realtimeShadow = lerp(realtimeShadow, bakedGI, shadowStrength);

    // 3) Pick darkest color
    return min(bakedGI, realtimeShadow);
    #else
    return bakedGI;
    #endif
}
