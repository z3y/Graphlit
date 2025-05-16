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

float GetSpotAngleAttenuation(float3 spotForward, float3 l, float spotScale, float spotOffset)
{
    float cd = dot(-spotForward, l);
    float attenuation = saturate(cd * spotScale + spotOffset);
    return attenuation * attenuation;
}

void GetSpotScaleOffset(float outerAngle, float innerAnglePercent, out float spotScale, out float spotOffset)
{
    half innerAngle = outerAngle / 100 * innerAnglePercent;
    innerAngle = innerAngle / 360 * PI;
    outerAngle = outerAngle / 360 * PI;
    float cosOuter = cos(outerAngle);
    spotScale = 1.0 / max(cos(innerAngle) - cosOuter, 1e-4);
    spotOffset = -cosOuter * spotScale;
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
        float3 positionToLight = _WorldSpaceLightPos0.xyz - positionWS * _WorldSpaceLightPos0.w;
        light.direction = positionToLight;
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

        #if defined(SHADOWS_SCREEN) || defined(SHADOWS_DEPTH) || defined(SHADOWS_CUBE)
            half shadowFade = GetMainLightShadowFade(positionWS);
        #else
            half shadowFade = half(1.0);
        #endif


        #ifdef UNITY_PASS_FORWARDADD
            float4 lightCoord = mul(unity_WorldToLight, float4(positionWS, 1));
            float3 lightZ = float3(unity_WorldToLight[0][2], unity_WorldToLight[1][2], unity_WorldToLight[2][2]);

            float distanceSquare = dot(positionToLight, positionToLight);
            half range = length(lightZ);
            light.distanceAttenuation = GetSquareFalloffAttenuation(distanceSquare, range * range);
            #ifdef SPOT
                float2 spotUV = lightCoord.xy / lightCoord.w + 0.5;
                light.color *= SAMPLE_TEXTURE2D(_LightTexture0, sampler_LightTexture0, spotUV).a * (lightCoord.z > 0);
            #endif

        #else
        light.distanceAttenuation = 1;
        #endif

        #if defined(SHADOWS_DEPTH) || defined(SHADOWS_CUBE)
        UNITY_BRANCH
        if (shadowFade < (1.0f - 1e-2f))
        {
            #if defined(SHADOWS_DEPTH) && defined(SPOT)
                float4 spotShadowCoord = mul(unity_WorldToShadow[0], float4(positionWS, 1));
                light.shadowAttenuation = SampleShadowmap(TEXTURE2D_SHADOW_ARGS(_ShadowMapTexture, sampler_LinearClampCompare), spotShadowCoord, _ShadowMapTexture_TexelSize);
            #elif defined(SHADOWS_CUBE) && defined(POINT)
                light.shadowAttenuation = UnitySampleShadowmap(positionWS - _LightPositionRange.xyz);
            #endif
        }
        #endif

        light.shadowAttenuation = UnityMixRealtimeAndBakedShadows(light.shadowAttenuation, shadowMaskAttenuation, shadowFade);

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
        float NoH = saturate(dot(shading.normalWS, halfVector));

        #ifndef QUALITY_LOW
        lightColor *= DisneyDiffuseNoPI(shading.NoV, NoL, LoV, shading.perceptualRoughness);
        #endif

        diffuse += lightColor * !light.specularOnly;
#ifndef _SPECULARHIGHLIGHTS_OFF
        #ifdef QUALITY_LOW
            half roughness2 = shading.roughness * shading.roughness;
            float d = NoH * NoH * (roughness2 - 1) + 1.00001f;

            half LoH2 = LoH * LoH;
            half normalizationTerm = (shading.perceptualRoughness * shading.perceptualRoughness) * 4.0 + 2.0;
            half specularTerm = roughness2 / ((d * d) * max(0.1h, LoH2) * normalizationTerm);
        #if REAL_IS_HALF
            specularTerm = specularTerm - HALF_MIN;
            specularTerm = clamp(specularTerm, 0.0, 1000.0); // Prevent FP16 overflow on mobiles
        #endif
        specular += specularTerm * shading.f0 * lightColor * (1.0/PI);
    #else
        real3 F = F_Schlick(shading.f0, LoH);
        real D = D_GGX(NoH, shading.roughness);
        real V = V_SmithJointGGX(NoL, shading.NoV, shading.roughness);

        specular += max(0.0, (D * V) * F) * lightColor;
    #endif
#endif
    }
}

half SampleAddShadowMap(float4 shadowCoord)
{

}