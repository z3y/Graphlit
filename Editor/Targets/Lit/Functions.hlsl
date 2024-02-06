Texture2D<float4> _DFG;
SamplerState custom_bilinear_clamp_sampler;

#ifdef UNITY_PBS_USE_BRDF2
    #define QUALITY_LOW
#endif

#include "Filament.hlsl"

void AlphaTransparentBlend(inout half alpha, inout half3 albedo, half metallic)
{
    #if defined(_ALPHAPREMULTIPLY_ON)
        albedo.rgb *= alpha;
        alpha = lerp(alpha, 1.0, metallic);
    #endif

    #if defined(_ALPHAMODULATE_ON)
        albedo = lerp(1.0, albedo, alpha);
    #endif

    #if !defined(_ALPHAFADE_ON) && !defined(_ALPHATEST_ON) && !defined(_ALPHAPREMULTIPLY_ON) && !defined(_ALPHAMODULATE_ON)
        alpha = 1.0f;
    #endif
}
void ApplyAlphaClip(inout half alpha, half clipThreshold)
{
    #if defined(_ALPHATEST_ON)
        clip(alpha - clipThreshold);
    #endif
}

struct Light
{
    float3 direction;
    half3 color;
    half attenuation;

    static Light Initialize(Varyings varyings)
    {
        Light light = (Light)0;

        #if !defined(USING_LIGHT_MULTI_COMPILE)
            return light;
        #endif

        float3 positionWS = UNPACK_POSITIONWS;

        light.direction = Unity_SafeNormalize(UnityWorldSpaceLightDir(positionWS));
        light.color = _LightColor0.rgb;

        UNITY_LIGHT_ATTENUATION(attenuation, varyings, positionWS.xyz);

        #if defined(HANDLE_SHADOWS_BLENDING_IN_GI) && defined(SHADOWS_SCREEN) && defined(LIGHTMAP_ON)
            half bakedAtten = UnitySampleBakedOcclusion(varyings.lightmapUV, positionWS);
            float zDist = dot(_WorldSpaceCameraPos -  positionWS, UNITY_MATRIX_V[2].xyz);
            float fadeDist = UnityComputeShadowFadeDistance(positionWS, zDist);
            attenuation = UnityMixRealtimeAndBakedShadows(attenuation, bakedAtten, UnityComputeShadowFade(fadeDist));
        #endif

        #if defined(UNITY_PASS_FORWARDBASE) && !defined(SHADOWS_SCREEN) && !defined(SHADOWS_SHADOWMASK)
            attenuation = 1.0;
        #endif

        light.attenuation = attenuation;

        #if defined(LIGHTMAP_SHADOW_MIXING) && defined(LIGHTMAP_ON)
            light.color *= UnityComputeForwardShadows(varyings.lightmapUV.xy, positionWS, varyings._shadowCoord);
        #endif

        return light;
    }
};

void ShadeLight(Light light, float3 viewDirectionWS, float3 normalWS, half roughness, half NoV, half3 f0, half3 energyCompensation, inout half3 color, inout half3 specular)
{
    half lightNoL = saturate(dot(normalWS, light.direction));
    #if !defined(_FLATSHADING)
    UNITY_BRANCH
    if (light.attenuation * lightNoL > 0)
    {
    #endif
        float3 lightHalfVector = normalize(light.direction + viewDirectionWS);
        half lightLoH = saturate(dot(light.direction, lightHalfVector));
        half lightNoH = saturate(dot(normalWS, lightHalfVector));

        half3 lightColor = lightNoL * light.attenuation * light.color;

        #ifdef UNITY_PASS_FORWARDBASE
            #if !defined(QUALITY_LOW) && !defined(LIGHTMAP_ON)
                lightColor *= Filament::Fd_Burley(roughness, NoV, lightNoL, lightLoH);
            #endif
        #endif

        #if defined(_FLATSHADING)
            color += light.attenuation * light.color;
        #else
            color += lightColor;
        #endif

        #ifndef _SPECULARHIGHLIGHTS_OFF
            half clampedRoughness = max(roughness * roughness, 0.002);
            #ifdef _ANISOTROPY
                // half at = max(clampedRoughness * (1.0 + surfaceDescription.Anisotropy), 0.001);
                // half ab = max(clampedRoughness * (1.0 - surfaceDescription.Anisotropy), 0.001);

                // float3 l = light.direction;
                // float3 t = sd.tangentWS;
                // float3 b = sd.bitangentWS;
                // float3 v = viewDirectionWS;

                // half ToV = dot(t, v);
                // half BoV = dot(b, v);
                // half ToL = dot(t, l);
                // half BoL = dot(b, l);
                // half ToH = dot(t, lightHalfVector);
                // half BoH = dot(b, lightHalfVector);

                // half3 F = Filament::F_Schlick(lightLoH, sd.f0) * energyCompensation;
                // half D = Filament::D_GGX_Anisotropic(lightNoH, lightHalfVector, t, b, at, ab);
                // half V = Filament::V_SmithGGXCorrelated_Anisotropic(at, ab, ToV, BoV, ToL, BoL, NoV, lightNoL);
            #else
                half3 F = Filament::F_Schlick(lightLoH, f0) * energyCompensation;
                half D = Filament::D_GGX(lightNoH, clampedRoughness);
                half V = Filament::V_SmithGGXCorrelated(NoV, lightNoL, clampedRoughness);
            #endif

            specular += max(0.0, (D * V) * F) * lightColor;
        #endif
    #if !defined(_FLATSHADING)
    }
    #endif
}

// Bicubic from Core RP

float2 BSpline3MiddleLeft(float2 x)
{
    return 0.16666667 + x * (0.5 + x * (0.5 - x * 0.5));
}

float2 BSpline3MiddleRight(float2 x)
{
    return 0.66666667 + x * (-1.0 + 0.5 * x) * x;
}

float2 BSpline3Rightmost(float2 x)
{
    return 0.16666667 + x * (-0.5 + x * (0.5 - x * 0.16666667));
}

void BicubicFilter(float2 fracCoord, out float2 weights[2], out float2 offsets[2])
{
    float2 r  = BSpline3Rightmost(fracCoord);
    float2 mr = BSpline3MiddleRight(fracCoord);
    float2 ml = BSpline3MiddleLeft(fracCoord);
    float2 l  = 1.0 - mr - ml - r;

    weights[0] = r + mr;
    weights[1] = ml + l;
    offsets[0] = -1.0 + mr * rcp(weights[0]);
    offsets[1] =  1.0 + l * rcp(weights[1]);
}

// Unity doesnt set lightmap texel size propert for lightmaps
float4 TexelSizeFromTexture2D(Texture2D t)
{
    float4 texelSize;
    t.GetDimensions(texelSize.x, texelSize.y);
    texelSize.zw = 1.0 / texelSize.xy;
    return texelSize;
}

float4 SampleTexture2DBicubic(Texture2D tex, SamplerState smp, float2 coord, float4 texSize, float2 maxCoord)
{
    float2 xy = coord * texSize.xy + 0.5;
    float2 ic = floor(xy);
    float2 fc = frac(xy);

    float2 weights[2], offsets[2];
    BicubicFilter(fc, weights, offsets);

    return weights[0].y * (weights[0].x * tex.SampleLevel(smp, min((ic + float2(offsets[0].x, offsets[0].y) - 0.5) * texSize.zw, maxCoord), 0.0)  +
                           weights[1].x * tex.SampleLevel(smp, min((ic + float2(offsets[1].x, offsets[0].y) - 0.5) * texSize.zw, maxCoord), 0.0)) +
           weights[1].y * (weights[0].x * tex.SampleLevel(smp, min((ic + float2(offsets[0].x, offsets[1].y) - 0.5) * texSize.zw, maxCoord), 0.0)  +
                           weights[1].x * tex.SampleLevel(smp, min((ic + float2(offsets[1].x, offsets[1].y) - 0.5) * texSize.zw, maxCoord), 0.0));
}