#ifndef UNITY_PBS_USE_BRDF1
    #define QUALITY_LOW
#endif

#include "Filament.hlsl"

struct GIInput
{
    half NoV;
    float3 reflectVector;
    half3 f0;
    half3 brdf;
    half3 energyCompensation;
    float3 normalWS; // this is the normal after the normal map is applied
    half specularAO;

    static GIInput New()
    {
        GIInput giInput = (GIInput)0;
        return giInput;
    }
};

struct GIOutput
{
    half3 directDiffuse;
    half3 directSpecular;
    half3 indirectDiffuse;
    half3 indirectSpecular;
    half3 indirectOcclusion;

    static GIOutput New()
    {
        GIOutput o = (GIOutput)0;
        o.directDiffuse = 0;
        o.directSpecular = 0;
        o.indirectSpecular = 0;
        o.indirectDiffuse = 0;
        o.indirectOcclusion = 1;
        
        return o;
    }
};

void ApplyAlphaClip(inout half alpha, half clipThreshold)
{
    #if defined(_ALPHATEST_ON)
        clip(alpha - clipThreshold);
    #endif
}

struct Light
{
    // properties
    float3 direction;
    half3 color;
    half attenuation;
    
    // calculated values
    float3 halfVector;
    half NoL;
    half LoH;
    half NoH;

    static Light GetUnityLight(Varyings varyings)
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
            light.color *= UnityComputeForwardShadows(varyings.lightmapUV.xy, positionWS, varyings._ShadowCoord);
        #endif

        return light;
    }

    // universal for any light
    void ComputeData(FragmentData fragData, GIInput giInput)
    {
        NoL = saturate(dot(giInput.normalWS, direction));
        halfVector = SafeNormalize(direction + fragData.viewDirectionWS);
        LoH = saturate(dot(direction, halfVector));
        NoH = saturate(dot(giInput.normalWS, halfVector));
    }
};

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

float shEvaluateDiffuseL1Geomerics(float L0, float3 L1, float3 n)
{
    // average energy
    float R0 = L0;
    
    // avg direction of incoming light
    float3 R1 = 0.5f * L1;
    
    // directional brightness
    float lenR1 = length(R1);
    
    // linear angle between normal and direction 0-1
    //float q = 0.5f * (1.0f + dot(R1 / lenR1, n));
    //float q = dot(R1 / lenR1, n) * 0.5 + 0.5;
    float q = dot(normalize(R1), n) * 0.5 + 0.5;
    q = saturate(q); // Thanks to ScruffyRuffles for the bug identity.
    
    // power for q
    // lerps from 1 (linear) to 3 (cubic) based on directionality
    float p = 1.0f + 2.0f * lenR1 / R0;
    
    // dynamic range constant
    // should vary between 4 (highly directional) and 0 (ambient)
    float a = (1.0f - lenR1 / R0) / (1.0f + lenR1 / R0);
    
    return R0 * (a + (1.0f - a) * (p + 1.0f) * pow(q, p));
}

#define LIGHT_FUNC void LightCustom(Light light, FragmentData fragData, GIInput giInput, SurfaceDescription surf, inout GIOutput giOutput)
#define LIGHT_DEFAULT LightDefault(light, fragData, giInput, surf, giOutput)
void LightDefault(Light light, FragmentData fragData, GIInput giInput, SurfaceDescription surf, inout GIOutput giOutput)
{
    UNITY_BRANCH
    if (light.attenuation * light.NoL > 0)
    {
        half3 lightColor = light.NoL * light.attenuation * light.color;

        #ifdef UNITY_PASS_FORWARDBASE
            #if !(defined(QUALITY_LOW) || defined(LIGHTMAP_ON))
                lightColor *= Filament::Fd_Burley(surf.Roughness, giInput.NoV, light.NoL, light.LoH);
            #endif
        #endif

        giOutput.directDiffuse += lightColor;

        #ifndef _SPECULARHIGHLIGHTS_OFF
            half clampedRoughness = max(surf.Roughness * surf.Roughness, 0.002);

            half3 F = Filament::F_Schlick(light.LoH, giInput.f0) * giInput.energyCompensation;
            half D = Filament::D_GGX(light.NoH, clampedRoughness);
            half V = Filament::V_SmithGGXCorrelated(giInput.NoV, light.NoL, clampedRoughness);

            giOutput.directSpecular += max(0.0, (D * V) * F) * lightColor * UNITY_PI * giInput.energyCompensation;
        #endif

    }
}
