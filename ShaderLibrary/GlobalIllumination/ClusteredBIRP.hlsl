#ifdef _CBIRP
#define UNITY_PI 1 // specular is multiplied later with pi
#include "Packages/z3y.clusteredbirp/Shaders/CBIRP.hlsl"
#undef UNITY_PI

void ComputeCBIRPLights(inout half3 diffuse, inout half3 specular, uint3 cluster, half4 shadowmask, FragmentData fragData, ShadingData shading, SurfaceDescription surf)
{
    half clampedRoughness = max(surf.Roughness * surf.Roughness, 0.002);

    CBIRP_CLUSTER_START_LIGHT(cluster)

        CBIRP::Light light = CBIRP::Light::DecodeLight(index);

        float3 positionToLight = light.positionWS - fragData.positionWS;
        float distanceSquare = dot(positionToLight, positionToLight);

        UNITY_BRANCH
        if (distanceSquare < light.range)
        {
            light.range = 1.0 / light.range;
            float3 L = normalize(positionToLight);
            half NoL = saturate(dot(shading.normalWS, L));
            // float attenuation = GetSquareFalloffAttenuation(distanceSquare, light.range);
            float attenuation = CBIRP::GetSquareFalloffAttenuationCustom(distanceSquare, light.range);

            // UNITY_BRANCH
            if (light.spot)
            {
                attenuation *= CBIRP::GetSpotAngleAttenuation(light.direction, L, light.spotScale, light.spotOffset);
            }

            // UNITY_BRANCH
            // if (light.ies)
            // {
            //     attenuation *= PhotometricAttenuation(L, light.direction);
            // }
            Light l = (Light)0;
            l.enabled = true;

            #ifdef LIGHTMAP_ON
            if (light.hasShadowmask)
            {
                attenuation *= shadowmask[light.shadowmaskID];
                l.specularOnly = light.specularOnly;
            }
            #endif


            l.color = light.color;
            l.direction = L;
            l.distanceAttenuation = attenuation;
            l.shadowAttenuation = 1.0;

            ShadeLight(diffuse, specular, l, shading);


        }
    CBIRP_CLUSTER_END_LIGHT
}
#endif
