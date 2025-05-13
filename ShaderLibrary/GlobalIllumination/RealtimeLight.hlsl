#pragma once

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/BSDF.hlsl"

struct Light
{
    float3 direction;
    half3 color;

    float distanceAttenuation;
    half shadowAttenuation;

    bool specularOnly;
    bool enabled;
    uint layerMask;
};

float GetSquareFalloffAttenuation(float distanceSquare, float lightInvRadius2)
{
    float factor = distanceSquare * lightInvRadius2;
    float smoothFactor = saturate(1.0 - factor * factor);
    return (smoothFactor * smoothFactor) / (distanceSquare + 1.0);
}
float4 GetSquareFalloffAttenuation(float4 distanceSquare, float4 lightInvRadius2)
{
    float4 factor = distanceSquare * lightInvRadius2;
    float4 smoothFactor = saturate(1.0 - factor * factor);
    return (smoothFactor * smoothFactor) / (distanceSquare + 1.0);
}

#ifdef UNIVERSALRP
void CopyUniversalLight(inout Light light, URPLight urpLight)
{
    light.direction = urpLight.direction;
    light.color = urpLight.color;
    light.distanceAttenuation = urpLight.distanceAttenuation;
    light.shadowAttenuation = urpLight.shadowAttenuation;
    light.layerMask = urpLight.layerMask;

    light.enabled = true;
    light.specularOnly = false;
}
#endif

Light GetMainLight(float3 positionWS, float4 shadowCoord, float2 lightmapUV)
{
    Light light;
    ZERO_INITIALIZE(Light, light);
    light.enabled = true;
    light.specularOnly = false;


    #ifdef UNIVERSALRP
        URPLight urpLight = GetMainLight(shadowCoord, positionWS, SampleShadowMask(lightmapUV));
        CopyUniversalLight(light, urpLight);
    #else
        light.direction = _WorldSpaceLightPos0.xyz - positionWS * _WorldSpaceLightPos0.w;
        #ifdef UNITY_PASS_FORWARDBASE
            light.enabled = any(_LightColor0.rgb) > 0;
        #else
            light.direction = SafeNormalize(light.direction);
        #endif

        light.color = _LightColor0.rgb;

        #ifdef SHADOWS_SCREEN
        light.shadowAttenuation = SampleScreenSpaceShadowmap(shadowCoord);
        #else
        light.shadowAttenuation = 1.0;
        #endif

        half shadowMaskAttenuation = UnitySampleBakedOcclusion(lightmapUV, positionWS);

        #ifdef SHADOWS_SCREEN
            half shadowFade = GetMainLightShadowFade(positionWS);
        #else
            half shadowFade = half(1.0);
        #endif

        light.shadowAttenuation = UnityMixRealtimeAndBakedShadows(light.shadowAttenuation, shadowMaskAttenuation, shadowFade);

        light.distanceAttenuation = 1;

        half4 cookieTexture = SampleUdonRPDirectionalCookie(positionWS);
        light.color *= cookieTexture.rgb;
    #endif

    #ifdef SHADOWS_SHADOWMASK
        // bakery custom lighting mode specular only
        #ifdef UNIVERSALRP
        // urp doesnt give light alpha
        #else
        light.specularOnly = !_LightColor0.a;
        #endif
    #endif

    return light;
}

uint GetAdditionalLightCount()
{
    #ifdef VERTEXLIGHT_ON
        return dot(unity_4LightAtten0 > 0.0, 1.0);
    #endif

    return 0;
}

Light GetAdditionalLight(float3 positionWS, uint i)
{
    Light light;
    ZERO_INITIALIZE(Light, light);
    light.enabled = false;

    #ifdef VERTEXLIGHT_ON
    light.specularOnly = false;

    float3 positionToLight = float3(unity_4LightPosX0[i], unity_4LightPosY0[i], unity_4LightPosZ0[i]) - positionWS;

    light.direction = normalize(positionToLight);
    light.color = unity_LightColor[i].rgb;

    float4 toLightX = unity_4LightPosX0 - positionWS.x;
    float4 toLightY = unity_4LightPosY0 - positionWS.y;
    float4 toLightZ = unity_4LightPosZ0 - positionWS.z;

    float4 lengthSq = 0.0;
    lengthSq += toLightX * toLightX;
    lengthSq += toLightY * toLightY;
    lengthSq += toLightZ * toLightZ;

    // https://forum.unity.com/threads/point-light-in-v-f-shader.499717/
    // float4 range = 5.0 * (1.0 / sqrt(unity_4LightAtten0));
    // float4 attenUV = sqrt(lengthSq) / range;
    // float4 attenuation = saturate(1.0 / (1.0 + 25.0 * attenUV * attenUV) * saturate((1 - attenUV) * 5.0));
    float distanceSquare = dot(positionToLight, positionToLight);
    float range = unity_4LightAtten0[i] / 25.0;

    float attenuation = GetSquareFalloffAttenuation(distanceSquare, range);

    light.distanceAttenuation = attenuation;
    light.shadowAttenuation = 1.0;

    light.enabled = true;
    #endif

    return light;
}


void ShadeLight(inout half3 diffuse, inout half3 specular, Light light, ShadingData shading)
{
    half NoL = saturate(dot(shading.normalWS, light.direction));

    UNITY_BRANCH
    if (light.enabled && light.distanceAttenuation * NoL > 0)
    {
        half3 lightColor = NoL * light.distanceAttenuation * light.shadowAttenuation * light.color;

        float3 halfVector = SafeNormalize(light.direction + shading.viewDirectionWS);
        half LoV = saturate(dot(light.direction, shading.viewDirectionWS));
        half LoH = saturate(dot(light.direction, halfVector));
        half NoH = saturate(dot(shading.normalWS, halfVector));

        lightColor *= DisneyDiffuseNoPI(shading.NoV, NoL, LoV, shading.perceptualRoughness);

        diffuse += lightColor * !light.specularOnly;
#ifndef _SPECULARHIGHLIGHTS_OFF
        half roughness = max(shading.perceptualRoughness * shading.perceptualRoughness, HALF_MIN_SQRT);

        real3 F = F_Schlick(shading.f0, LoH);
        real D = D_GGX(NoH, roughness);
        real V = V_SmithJointGGX(NoL, shading.NoV, roughness);

        specular += max(0.0, (D * V) * F) * lightColor * PI * shading.energyCompensation;
#endif
    }
}