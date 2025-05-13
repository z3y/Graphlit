#pragma once

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

#ifdef UNIVERSALRP
    #if UNITY_LIGHT_PROBE_PROXY_VOLUME
    #undef UNITY_LIGHT_PROBE_PROXY_VOLUME
    #define UNITY_LIGHT_PROBE_PROXY_VOLUME 0
    #endif
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
#else
    #include "Input.hlsl"

    #if UNITY_REVERSED_Z
        // TODO: workaround. There's a bug where SHADER_API_GL_CORE gets erroneously defined on switch.
        #if (defined(SHADER_API_GLCORE) && !defined(SHADER_API_SWITCH)) || defined(SHADER_API_GLES3)
            //GL with reversed z => z clip range is [near, -far] -> remapping to [0, far]
            #define UNITY_Z_0_FAR_FROM_CLIPSPACE(coord) max((coord - _ProjectionParams.y)/(-_ProjectionParams.z-_ProjectionParams.y)*_ProjectionParams.z, 0)
        #else
            //D3d with reversed Z => z clip range is [near, 0] -> remapping to [0, far]
            //max is required to protect ourselves from near plane not being correct/meaningful in case of oblique matrices.
            #define UNITY_Z_0_FAR_FROM_CLIPSPACE(coord) max(((1.0-(coord)/_ProjectionParams.y)*_ProjectionParams.z),0)
        #endif
    #elif UNITY_UV_STARTS_AT_TOP
        //D3d without reversed z => z clip range is [0, far] -> nothing to do
        #define UNITY_Z_0_FAR_FROM_CLIPSPACE(coord) (coord)
    #else
        //Opengl => z clip range is [-near, far] -> remapping to [0, far]
        #define UNITY_Z_0_FAR_FROM_CLIPSPACE(coord) max(((coord + _ProjectionParams.y)/(_ProjectionParams.z+_ProjectionParams.y))*_ProjectionParams.z, 0)
    #endif

    #if defined(SHADER_API_D3D11) || defined(SHADER_API_PSSL) || defined(SHADER_API_METAL) || defined(SHADER_API_GLCORE) || defined(SHADER_API_GLES3) || defined(SHADER_API_VULKAN) || defined(SHADER_API_SWITCH) // D3D11, D3D12, XB1, PS4, iOS, macOS, tvOS, glcore, gles3, webgl2.0, Switch
    // Real-support for depth-format cube shadow map.
    #define SHADOWS_CUBE_IN_DEPTH_TEX
    #endif

    #include "ShaderVariableFunctions.hlsl"
#endif

SamplerState sampler_BilinearClamp;

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"

#ifndef UNIVERSALRP
#include "Shadows.hlsl"
#include "UdonRP.hlsl"
#else
#define Light URPLight
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"
#undef Light
float3 _LightDirection; // just like that ?
float3 _LightPosition;
#endif

#include "ShadingData.hlsl"

#include "GlobalIllumination/LightProbe.hlsl"
#include "GlobalIllumination/Lightmap.hlsl"
#include "GlobalIllumination/RealtimeLight.hlsl"
#include "GlobalIllumination/ReflectionProbe.hlsl"
// #include "GlobalIllumination/SurfaceDescription.hlsl"

#include "GraphFunctions.hlsl"
#include "BlendModes.hlsl"

float4 GetFlatNormal()
{
    // todo: find proper defines for normal map packing
    #if defined(UNIVERSALRP) && defined(TARGET_ANDROID)
    return float4(0.5, 0.5, 1, 0.5);
    #else
    return float4(0.5, 0.5, 1, 1);
    #endif
}

// untested keywords
// DYNAMICLIGHTMAP_ON
