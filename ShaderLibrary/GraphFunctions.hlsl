#pragma once

Texture2D nullTexture;
SamplerState null_LinearRepeat;

float Unity_Dither(float In, float2 ScreenPosition)
{
    float2 uv = ScreenPosition * _ScreenParams.xy;
    const half4 DITHER_THRESHOLDS[4] =
    {
        half4(1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0),
        half4(13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0),
        half4(4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0),
        half4(16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0)
    };

    return In - DITHER_THRESHOLDS[uint(uv.x) % 4][uint(uv.y) % 4];
}

TEXTURE2D_X(_CameraOpaqueTexture);
SAMPLER(sampler_CameraOpaqueTexture);
float4 _CameraOpaqueTexture_TexelSize;

float4 SampleSceneColor(float2 uv)
{
    #if defined(PREVIEW) || defined(SHADER_API_MOBILE)
    return 0;
    #endif
    return SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv);
}

TEXTURE2D_X_FLOAT(_CameraDepthTexture);
SAMPLER(sampler_CameraDepthTexture);
float SampleSceneDepth(float2 uv)
{
    #if defined(PREVIEW) || defined(SHADER_API_MOBILE)
    return 0;
    #endif
    return SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, uv).r;
}

// void AlphaTransparentBlend(inout half alpha, inout half3 albedo, half metallic)
// {
//     if (USE_ALPHAMULTIPLY)
//     {
//         albedo = lerp(1.0, albedo, alpha);
//     }
//     if (USE_ALPHAPREMULTIPLY)
//     {
//         albedo.rgb *= alpha;
//     }

//     #if defined(_SURFACE_TYPE_TRANSPARENT)
//     bool isTransparent = true;
//     #else
//     bool isTransparent = false;
//     #endif
    
//     alpha = OutputAlpha(alpha, isTransparent);
// }

void BlendFinalColor(out half3 Color, out half Alpha, half3 diffuse = 1, half3 specular = 0, half3 emission = 0, half3 albedo = 1, half roughness = 0, half metallic = 0, half alpha = 1)
{
    Color = diffuse;

    #ifndef UNITY_PASS_SHADOWCASTER
        if (USE_ALPHAMULTIPLY)
        {
            albedo = lerp(1.0, albedo, alpha);
        }
        if (USE_ALPHAPREMULTIPLY)
        {
            albedo *= alpha;
        }

        #if defined(_SURFACE_TYPE_TRANSPARENT)
        bool isTransparent = true;
        #else
        bool isTransparent = false;
        #endif
        
        alpha = OutputAlpha(alpha, isTransparent);

        Color = albedo * (1.0 - metallic) * diffuse;
        Color += specular;

        #if defined(UNITY_PASS_FORWARDBASE)
            Color += emission;
        #endif
    #else
        if (USE_ALPHAPREMULTIPLY)
        {
            alpha = lerp(alpha, 1.0, metallic);
        }
    #endif
    Alpha = alpha;
}