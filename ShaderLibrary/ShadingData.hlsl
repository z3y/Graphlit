#pragma once

struct ShadingData
{
    half NoV;
    float3 normalWS;
    float3 reflectVector;
    float3 coatReflectVector;
    half3 f0;
    half3 f90;
    half3 f82;
    half3 diffuseColor;
    half perceptualRoughness;
    half specularRoughness;
    half metallic;
    float3 viewDirectionWS;
    float3 tangentWS;
    float3 bitangentWS;
    half anisotropy;
    half3 coatf0;
    half coatWeight;
    half coatSpecularRoughness;
};
