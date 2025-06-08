#pragma once

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ImageBasedLighting.hlsl"

#ifndef _REFLECTION_PROBE_BOX_PROJECTION
#define _REFLECTION_PROBE_BOX_PROJECTION
#endif

#ifdef UNIVERSALRP
#define ENABLE_ENVIRONMENT_PROBE
#endif

TEXTURE2D(_DFG);
SAMPLER(sampler_DFG);

half ComputeCoatAffectedRoughness(half specularRoughness, half coatRoughness, half coatWeight)
{
    half rB4 = pow(specularRoughness, 4.0);
    half rC4 = pow(coatRoughness, 4.0);
    half modified = pow(rB4 + 2.0 * rC4, 0.25);
    half clamped = min(1.0, modified);
    return lerp(specularRoughness, clamped, coatWeight);
}

bool IsBoxProjection(float4 cubemapPositionWS)
{
    bool boxProjection = cubemapPositionWS.w > 0.0f;
    #ifdef _UDONRP_ENVIRONMENT_PROBE
    // hack to force box projection when its disabled because unity then sets min/max values not tied to renderer bounds
    boxProjection = any(cubemapPositionWS);
    #endif
    return boxProjection;
}

half3 BoxProjectedCubemapDirection(half3 reflectionWS, float3 positionWS, float4 cubemapPositionWS, float4 boxMin, float4 boxMax)
{
    UNITY_FLATTEN
    if (IsBoxProjection(cubemapPositionWS))
    {
        float3 boxMinMax = (reflectionWS > 0.0f) ? boxMax.xyz : boxMin.xyz;
        half3 rbMinMax = half3(boxMinMax - positionWS) / reflectionWS;

        half fa = half(min(min(rbMinMax.x, rbMinMax.y), rbMinMax.z));

        half3 worldPos = half3(positionWS - cubemapPositionWS.xyz);

        half3 result = worldPos + reflectionWS * fa;
        return result;
    }
    else
    {
        return reflectionWS;
    }
}

#include "ContactHardening.hlsl"

float CalculateProbeWeight(float3 positionWS, float4 probeBoxMin, float4 probeBoxMax)
{
    float blendDistance = probeBoxMax.w;
    float3 weightDir = min(positionWS - probeBoxMin.xyz, probeBoxMax.xyz - positionWS) / blendDistance;
    return saturate(min(weightDir.x, min(weightDir.y, weightDir.z)));
}

half CalculateProbeVolumeSqrMagnitude(float4 probeBoxMin, float4 probeBoxMax)
{
    half3 maxToMin = half3(probeBoxMax.xyz - probeBoxMin.xyz);
    return dot(maxToMin, maxToMin);
}

half GetHorizonOcclusion(float3 reflectVector, float3 vertexNormalWS)
{
    float horizon = min(1.0 + dot(reflectVector, vertexNormalWS), 1.0);
    return saturate(horizon * horizon);
}

half3 CalculateIrradianceFromReflectionProbes(half3 reflectVector, float3 positionWS, half perceptualRoughness, float2 normalizedScreenSpaceUV, float3 vertexNormalWS)
{
    half3 irradiance = half3(0.0h, 0.0h, 0.0h);
    half mip = PerceptualRoughnessToMipmapLevel(perceptualRoughness);
#if USE_FORWARD_PLUS
    float totalWeight = 0.0f;
    uint probeIndex;
    ClusterIterator it = ClusterInit(normalizedScreenSpaceUV, positionWS, 1);
    [loop] while (ClusterNext(it, probeIndex) && totalWeight < 0.99f)
    {
        probeIndex -= URP_FP_PROBES_BEGIN;

        float weight = CalculateProbeWeight(positionWS, urp_ReflProbes_BoxMin[probeIndex], urp_ReflProbes_BoxMax[probeIndex]);
        weight = min(weight, 1.0f - totalWeight);

        half3 sampleVector = reflectVector;
#ifdef _REFLECTION_PROBE_BOX_PROJECTION
        sampleVector = BoxProjectedCubemapDirection(reflectVector, positionWS, urp_ReflProbes_ProbePosition[probeIndex], urp_ReflProbes_BoxMin[probeIndex], urp_ReflProbes_BoxMax[probeIndex]);
#endif // _REFLECTION_PROBE_BOX_PROJECTION

        uint maxMip = (uint)abs(urp_ReflProbes_ProbePosition[probeIndex].w) - 1;
        half probeMip = min(mip, maxMip);
        float2 uv = saturate(PackNormalOctQuadEncode(sampleVector) * 0.5 + 0.5);

        float mip0 = floor(probeMip);
        float mip1 = mip0 + 1;
        float mipBlend = probeMip - mip0;
        float4 scaleOffset0 = urp_ReflProbes_MipScaleOffset[probeIndex * 7 + (uint)mip0];
        float4 scaleOffset1 = urp_ReflProbes_MipScaleOffset[probeIndex * 7 + (uint)mip1];

        half3 irradiance0 = half4(SAMPLE_TEXTURE2D_LOD(urp_ReflProbes_Atlas, sampler_LinearClamp, uv * scaleOffset0.xy + scaleOffset0.zw, 0.0)).rgb;
        half3 irradiance1 = half4(SAMPLE_TEXTURE2D_LOD(urp_ReflProbes_Atlas, sampler_LinearClamp, uv * scaleOffset1.xy + scaleOffset1.zw, 0.0)).rgb;
        irradiance += weight * lerp(irradiance0, irradiance1, mipBlend);
        totalWeight += weight;
    }
#else
    half probe0Volume = CalculateProbeVolumeSqrMagnitude(unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
    half probe1Volume = CalculateProbeVolumeSqrMagnitude(unity_SpecCube1_BoxMin, unity_SpecCube1_BoxMax);

    half volumeDiff = probe0Volume - probe1Volume;
    float importanceSign = unity_SpecCube1_BoxMin.w;

    // A probe is dominant if its importance is higher
    // Or have equal importance but smaller volume
    bool probe0Dominant = importanceSign > 0.0f || (importanceSign == 0.0f && volumeDiff < -0.0001h);
    bool probe1Dominant = importanceSign < 0.0f || (importanceSign == 0.0f && volumeDiff > 0.0001h);

    float desiredWeightProbe0 = CalculateProbeWeight(positionWS, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
    float desiredWeightProbe1 = CalculateProbeWeight(positionWS, unity_SpecCube1_BoxMin, unity_SpecCube1_BoxMax);

    // Subject the probes weight if the other probe is dominant
    float weightProbe0 = probe1Dominant ? min(desiredWeightProbe0, 1.0f - desiredWeightProbe1) : desiredWeightProbe0;
    float weightProbe1 = probe0Dominant ? min(desiredWeightProbe1, 1.0f - desiredWeightProbe0) : desiredWeightProbe1;


    #ifdef _UDONRP_ENVIRONMENT_PROBE
    weightProbe0 = any(unity_SpecCube0_ProbePosition) ? weightProbe0 : 0;
    weightProbe1 = any(unity_SpecCube1_ProbePosition) ? weightProbe1 : 0;
    #endif

    float totalWeight = weightProbe0 + weightProbe1;

    // If either probe 0 or probe 1 is dominant the sum of weights is guaranteed to be 1.
    // If neither is dominant this is not guaranteed - only normalize weights if totalweight exceeds 1.
    weightProbe0 /= max(totalWeight, 1.0f);
    weightProbe1 /= max(totalWeight, 1.0f);

    // Sample the first reflection probe
    #ifdef ENABLE_ENVIRONMENT_PROBE
    if (weightProbe0 > 0.01f)
    #endif
    {
        half3 reflectVector0 = reflectVector;
        half perceptualRoughness0 = perceptualRoughness;
        half mip0 = mip;
#ifdef _REFLECTION_PROBE_BOX_PROJECTION
        BoxProjectedCubemapDirection(reflectVector0, perceptualRoughness0, positionWS, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
        mip0 = PerceptualRoughnessToMipmapLevel(perceptualRoughness0);
#endif

        half4 encodedIrradiance = half4(SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, reflectVector0, mip0));
        
        half3 probe0 = DecodeHDREnvironment(encodedIrradiance, unity_SpecCube0_HDR) * GetHorizonOcclusion(reflectVector, vertexNormalWS);
        #ifndef ENABLE_ENVIRONMENT_PROBE
            irradiance = probe0;
        #else
            irradiance += weightProbe0 * probe0;
        #endif
    }
    
    // Sample the second reflection probe
    #ifdef ENABLE_ENVIRONMENT_PROBE
    if (weightProbe1 > 0.01f)
    #else
    if (unity_SpecCube0_BoxMin.w < 0.99999)
    #endif
    {
        half3 reflectVector1 = reflectVector;
        half perceptualRoughness1 = perceptualRoughness;
        half mip1 = mip;
#ifdef _REFLECTION_PROBE_BOX_PROJECTION
        BoxProjectedCubemapDirection(reflectVector1, perceptualRoughness1, positionWS, unity_SpecCube1_ProbePosition, unity_SpecCube1_BoxMin, unity_SpecCube1_BoxMax);
        mip1 = PerceptualRoughnessToMipmapLevel(perceptualRoughness1);
#endif
        half4 encodedIrradiance = half4(SAMPLE_TEXTURECUBE_LOD(unity_SpecCube1, samplerunity_SpecCube0, reflectVector1, mip1));
        half3 probe1 = DecodeHDREnvironment(encodedIrradiance, unity_SpecCube1_HDR) * GetHorizonOcclusion(reflectVector, vertexNormalWS);
        #ifndef ENABLE_ENVIRONMENT_PROBE
            irradiance = lerp(probe1, irradiance, unity_SpecCube0_BoxMin.w);
        #else
            irradiance += weightProbe1 * probe1;
        #endif
    }
#endif

#ifdef ENABLE_ENVIRONMENT_PROBE
    // Use any remaining weight to blend to environment reflection cube map
    if (totalWeight < 0.99f)
    {
        half4 encodedIrradiance = half4(SAMPLE_TEXTURECUBE_LOD(_GlossyEnvironmentCubeMap, samplerunity_SpecCube0, reflectVector, mip));

        irradiance += (1.0f - totalWeight) * DecodeHDREnvironment(encodedIrradiance, _GlossyEnvironmentCubeMap_HDR) * GetHorizonOcclusion(reflectVector, vertexNormalWS);
    }
#endif

    return irradiance;
}

// https://blog.selfshadow.com/publications/s2013-shading-course/lazarov/s2013_pbs_black_ops_2_notes.pdf
half3 EnvironmentBRDFApproximation(half perceptualRoughness, half NoV, half3 f0)
{
    half g = 1 - perceptualRoughness;
    half4 t = half4(1 / 0.96, 0.475, (0.0275 - 0.25 * 0.04) / 0.96, 0.25);
    t *= half4(g, g, g, g);
    t += half4(0.0, 0.0, (0.015 - 0.75 * 0.04) / 0.96, 0.75);
    half a0 = t.x * min(t.y, exp2(-9.28 * NoV)) + t.z;
    half a1 = t.w;
    return saturate(lerp(a0, a1, f0));
}

void EnvironmentBRDF(half NoV, half perceptualRoughness, half3 f0, out half3 brdf, out half3 invBrdf, out half3 energyCompensation, float3 f82 = 1, half metallic = 0)
{
    #if defined(QUALITY_LOW)
        energyCompensation = 1.0;
        brdf = EnvironmentBRDFApproximation(perceptualRoughness, NoV, f0);
        invBrdf = 1.0 - brdf;
    #else   
        const float lutRes = 64;
        float2 coordLUT = Remap01ToHalfTexelCoord(float2(sqrt(NoV), perceptualRoughness), lutRes);
        float4 dfg = SAMPLE_TEXTURE2D_LOD(_DFG, sampler_BilinearClamp, coordLUT, 0);
        brdf = lerp(dfg.xxx, dfg.yyy, f0);
        invBrdf = 1.0 - brdf;
        energyCompensation = 1.0 + f0 * (1.0 / dfg.y - 1.0);

        // f82
        float f = 6.0 / 7.0;
        float3 schlick = lerp(f0, 1.0, pow(f, 5));
        brdf -= schlick * (7.0 / pow(f, 6)) * (1.0 - f82) * dfg.z * metallic;
        brdf *= lerp(f82, 1.0, metallic);
    #endif
}