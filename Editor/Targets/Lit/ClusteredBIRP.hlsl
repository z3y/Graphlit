void ComputeCBIRPLights(uint3 cluster, half4 shadowmask, FragmentData fragData, GIInput giInput, SurfaceDescription surf, inout GIOutput giOutput)
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
            half NoL = saturate(dot(giInput.normalWS, L));
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

            #ifdef LIGHTMAP_ON
            if (light.hasShadowmask)
            {
                attenuation *= shadowmask[light.shadowmaskID];
            }
            #endif


            Light l = (Light)0;
            l.color = light.color;
            l.direction = L;
            l.attenuation = attenuation;

            l.ComputeData(fragData, giInput);

            LIGHT_IMPL(l, fragData, giInput, surf, giOutput);

            // diffuse += currentDiffuse * !light.specularOnly;


        }
    CBIRP_CLUSTER_END_LIGHT
}