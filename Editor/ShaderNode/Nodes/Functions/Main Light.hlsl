#pragma once

void MainLightData(float4 ShadowCoord, float2 LightmapUV, float3 PositionWS, out float3 Color, out float3 Direction, out float DistanceAttenuation, out float ShadowAttenuation)
{
    Light light = GetMainLight(PositionWS, ShadowCoord, LightmapUV);

    Color = light.color;
    Direction = light.direction;
    DistanceAttenuation = light.distanceAttenuation;
    ShadowAttenuation = light.shadowAttenuation;
}