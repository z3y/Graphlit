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

#ifndef LIGHT_ATTENUATION_MULTIPLIER
#define LIGHT_ATTENUATION_MULTIPLIER 1.0
#endif

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

float GetCotanHalfSpotAngle(float2 uv, float3 lightCoord)
{
    // float cotanHalfSpotAngle_x = (2.0 * p.z * (uv.x - 0.5)) / p.x;
    // float cotanHalfSpotAngle_y = (2.0 * p.z * (uv.y - 0.5)) / p.y;
    uv = saturate(uv);
    float2 p = lightCoord.xy;
    // p += p == 0 ? 0.001 : 0;
    float2 cotanHalfSpotAngle_xy = (2.0 * lightCoord.z * (uv - 0.5)) / p;

    return 0.5 * (cotanHalfSpotAngle_xy.x + cotanHalfSpotAngle_xy.y);
}

bool IsDefaultCookie()
{
    #ifdef SPOT
    return abs(_LightTexture0[int2(30, 30)].a - 0.853) < 0.01;
    #endif
    return true;
}

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

        light.distanceAttenuation = 1;

        #ifdef UNITY_PASS_FORWARDADD
            float4 lightCoord = mul(unity_WorldToLight, float4(positionWS, 1));
        #endif

        #if defined(SPOT) || defined(POINT)
            float3 lightZ = float3(unity_WorldToLight[0][2], unity_WorldToLight[1][2], unity_WorldToLight[2][2]);

            float distanceSquare = dot(positionToLight, positionToLight);
            half range = length(lightZ);

            #ifndef SQUARE_FALLOFF_ATTENUATION
                #if defined(POINT)
                    light.distanceAttenuation = SAMPLE_TEXTURE2D(_LightTexture0, sampler_LightTexture0, dot(lightCoord.xyz, lightCoord.xyz).xx);
                #elif defined(SPOT)
                    light.distanceAttenuation = SAMPLE_TEXTURE2D(_LightTextureB0, sampler_LightTextureB0, dot(lightCoord.xyz, lightCoord.xyz).xx);
                #endif
            #else
                light.distanceAttenuation = GetSquareFalloffAttenuation(distanceSquare, range * range);
            #endif

            light.distanceAttenuation *= LIGHT_ATTENUATION_MULTIPLIER;
            #ifdef SPOT
                float2 spotUV = lightCoord.xy / lightCoord.w + 0.5;

                #if 1
                half cotanHalfSpotAngle = GetCotanHalfSpotAngle(spotUV, lightCoord.xyz);
                half outerAngle = degrees(atan(1.0 / cotanHalfSpotAngle) * 2.0);
                float spotScale, spotOffset;
                GetSpotScaleOffset(outerAngle, 80, spotScale, spotOffset);

                float3 spotForward = normalize(unity_WorldToLight[2].xyz);
                light.distanceAttenuation *= GetSpotAngleAttenuation(spotForward, light.direction, spotScale, spotOffset);
                #else
                #endif
                half4 cookieTex = SAMPLE_TEXTURE2D(_LightTexture0, sampler_LightTexture0, spotUV);
                light.color *= IsDefaultCookie() ? 1.0 : cookieTex.rgb;
            #endif
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

        #ifndef UNITY_PASS_FORWARDADD
        half4 cookieTexture = SampleUdonRPDirectionalCookie(positionWS);
        light.color *= cookieTexture.rgb;
        #endif

        #ifdef DIRECTIONAL_COOKIE
            light.color *= SAMPLE_TEXTURE2D(_LightTexture0, sampler_LightTexture0, lightCoord.xy).rgb;
        #endif
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

half2 GetAtAb(half roughness, half anisotropy)
{
    half at = max(roughness * (1.0 + anisotropy), 0.001);
    half ab = max(roughness * (1.0 - anisotropy), 0.001);

    return half2(at, ab);
}

// https://renderwonk.com/publications/wp-generalization-adobe/gen-adobe.pdf
half3 F_SchlickHoffman(float cosTheta, half3 f0, half3 f82)
{
    float COS_THETA_MAX = 1.0 / 7.0;
    float COS_THETA_FACTOR = 1.0 / (COS_THETA_MAX * pow(1.0 - COS_THETA_MAX, 6.0));

    half exponent = 5;
    half3 f90 = 1;
    float x = cosTheta;
    float3 a = lerp(f0, f90, pow(1.0 - COS_THETA_MAX, exponent)) * (1.0 - f82) * COS_THETA_FACTOR;
    return lerp(f0, f90, pow(1.0 - x, exponent)) - a * x * pow(1.0 - x, 6);
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

        half3 Fd = lightColor;
        #if !defined(QUALITY_LOW) && !defined(_CBIRP)
            Fd *= DisneyDiffuseNoPI(shading.NoV, NoL, LoV, shading.perceptualRoughness);
        #endif

        diffuse += Fd * !light.specularOnly;
#ifndef _SPECULARHIGHLIGHTS_OFF
    #if defined(QUALITY_LOW) || defined(_CBIRP)
        half roughness2 = shading.specularRoughness * shading.specularRoughness;
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
        // real3 F = F_Schlick(shading.f0, LoH);
        real3 F = F_SchlickHoffman(LoH, shading.f0, lerp(1.0, shading.f82, shading.metallic));
        F *= lerp(shading.f82, 1.0, shading.metallic);
        real D = D_GGX(NoH, shading.specularRoughness);
        real V = V_SmithJointGGX(NoL, shading.NoV, shading.specularRoughness);

        #ifdef _ANISOTROPY
            float3 l = light.direction;
            float3 t = shading.tangentWS;
            float3 b = shading.bitangentWS;
            float3 v = shading.viewDirectionWS;

            half ToV = dot(t, v);
            half BoV = dot(b, v);
            half ToL = dot(t, l);
            half BoL = dot(b, l);
            half ToH = dot(t, halfVector);
            half BoH = dot(b, halfVector);
            half2 atab = GetAtAb(shading.specularRoughness, shading.anisotropy);
            D = D_GGXAniso(ToH, BoH, NoH, atab.x, atab.y);
            V = V_SmithJointGGXAniso(ToV, BoV, shading.NoV, ToL, BoL, NoL, atab.x, atab.y);
        #endif

        half3 response = max(0.0, (D * V) * F);
        half3 throughput = 1.0 - dot(F, 1.0 / 3.0);

        #if defined(_COAT)
            real3 coatF = F_Schlick(shading.coatf0, LoH);
            real coatD = D_GGX(NoH, shading.coatSpecularRoughness);
            real coatV = V_SmithJointGGX(NoL, shading.coatNoV, shading.coatSpecularRoughness);

            half3 coatThroughput = 1.0 - dot(coatF, 1.0 / 3.0) * shading.coatWeight;
            half3 coatResponse = max(0.0, (coatD * coatV) * coatF) * shading.coatWeight;
            response = coatResponse + response * coatThroughput;
            throughput = coatThroughput * throughput;
        #endif

        specular += response * lightColor;
        
    #endif
#endif
    }
}