#pragma once

struct ShadingData
{
    half NoV;
    float3 normalWS;
    float3 reflectVector;
    half3 f0;
    half perceptualRoughness;
    float3 viewDirectionWS;
    half3 energyCompensation;
};
