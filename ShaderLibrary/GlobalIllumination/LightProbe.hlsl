#pragma once

bool ProbeVolumeEnabled()
{
    #ifdef _VRC_LIGHTVOLUMES
        return false;
    #endif
    #if UNITY_LIGHT_PROBE_PROXY_VOLUME
        return unity_ProbeVolumeParams.x;
    #else
        return false;
    #endif
}

#include "ZH3.hlsl"

// #define ZH3

#ifdef _VRC_LIGHTVOLUMES
#include "Packages/red.sim.lightvolumes/Shaders/LightVolumes.cginc"
#endif

#ifndef UNIVERSALRP
half3 SHEvalLinearL0L1_SampleProbeVolume(float3 normalWS, float3 positionWS)
{
    float transformToLocal = unity_ProbeVolumeParams.y;
    float texelSizeX = unity_ProbeVolumeParams.z;

    //The SH coefficients textures and probe occlusion are packed into 1 atlas.
    //-------------------------
    //| ShR | ShG | ShB | Occ |
    //-------------------------

    float3 position = (transformToLocal == 1.0f) ? mul(unity_ProbeVolumeWorldToObject, float4(positionWS, 1.0)).xyz : positionWS;
    float3 texCoord = (position - unity_ProbeVolumeMin.xyz) * unity_ProbeVolumeSizeInv.xyz;
    texCoord.x = texCoord.x * 0.25f;

    // We need to compute proper X coordinate to sample.
    // Clamp the coordinate otherwize we'll have leaking between RGB coefficients
    float texCoordX = clamp(texCoord.x, 0.5f * texelSizeX, 0.25f - 0.5f * texelSizeX);

    // sampler state comes from SHr (all SH textures share the same sampler)
    texCoord.x = texCoordX;
    half4 SHAr = SAMPLE_TEXTURE3D(unity_ProbeVolumeSH, samplerunity_ProbeVolumeSH, texCoord);

    texCoord.x = texCoordX + 0.25f;
    half4 SHAg = SAMPLE_TEXTURE3D(unity_ProbeVolumeSH, samplerunity_ProbeVolumeSH, texCoord);

    texCoord.x = texCoordX + 0.5f;
    half4 SHAb = SAMPLE_TEXTURE3D(unity_ProbeVolumeSH, samplerunity_ProbeVolumeSH, texCoord);

    // Linear + constant polynomial terms
    half3 x1 = SHEvalLinearL0L1(normalWS, SHAr, SHAg, SHAb);

    return x1;
}
#else
half3 SHEvalLinearL0L1_SampleProbeVolume(float3 normalWS, float3 positionWS)
{
    return 0;
}
#endif

half3 SampleSH(float3 normalWS, float3 positionWS)
{
    #ifdef _VRC_LIGHTVOLUMES
        #define SH_SKIP_L2
        half3 lvL0;
        half3 lvL1r;
        half3 lvL1g;
        half3 lvL1b;
        LightVolumeSH(positionWS, lvL0, lvL1r, lvL1g, lvL1b);
        // return LightVolumeEvaluate(normalWS, lvL0, lvL1r, lvL1g, lvL1b);
        unity_SHAr.rgb = lvL1r;
        unity_SHAg.rgb = lvL1g;
        unity_SHAb.rgb = lvL1b;

        unity_SHAr.a = lvL0.r;
        unity_SHAg.a = lvL0.g;
        unity_SHAb.a = lvL0.b;
    #endif

    half3 res = 0;

    if (ProbeVolumeEnabled())
    {
        res = SHEvalLinearL0L1_SampleProbeVolume(normalWS, positionWS);
    }
    else
    {
        #ifdef ZH3
            res += SHEvalLinearL0L1_ZH3Hallucinate(normalWS, unity_SHAr, unity_SHAg, unity_SHAb);
        #else
            res += SHEvalLinearL0L1(normalWS, unity_SHAr, unity_SHAg, unity_SHAb);
        #endif
    }

    #ifndef SH_SKIP_L2
    res += SHEvalLinearL2(normalWS, unity_SHBr, unity_SHBg, unity_SHBb, unity_SHC);
    #endif

    res = max(0, res);

    return res;
}