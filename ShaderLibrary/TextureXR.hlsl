#pragma once

// Stereo-related bits
#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)

    #define SLICE_ARRAY_INDEX   unity_StereoEyeIndex

    #define TEXTURE2D_X(textureName)                                        TEXTURE2D_ARRAY(textureName)
    #define TYPED_TEXTURE2D_X(type, textureName)                            TYPED_TEXTURE2D_ARRAY(type, textureName)
    #define TEXTURE2D_X_PARAM(textureName, samplerName)                     TEXTURE2D_ARRAY_PARAM(textureName, samplerName)
    #define TEXTURE2D_X_ARGS(textureName, samplerName)                      TEXTURE2D_ARRAY_ARGS(textureName, samplerName)
    #define TEXTURE2D_X_HALF(textureName)                                   TEXTURE2D_ARRAY_HALF(textureName)
    #define TEXTURE2D_X_FLOAT(textureName)                                  TEXTURE2D_ARRAY_FLOAT(textureName)

    // We need to redeclare these macros for XR reasons to actually utilise the Texture2DArrays
    // TODO: add MSAA support, which is not being used anywhere in URP at the moment
    #undef FRAMEBUFFER_INPUT_X_HALF
    #undef FRAMEBUFFER_INPUT_X_FLOAT
    #undef FRAMEBUFFER_INPUT_X_INT
    #undef FRAMEBUFFER_INPUT_X_UINT
    #undef LOAD_FRAMEBUFFER_X_INPUT

    #if defined(SHADER_API_METAL) && defined(UNITY_NEEDS_RENDERPASS_FBFETCH_FALLBACK)

        #define RENDERPASS_DECLARE_FALLBACK_X(T, idx)                                                   \
        Texture2DArray<T> _UnityFBInput##idx; float4 _UnityFBInput##idx##_TexelSize;                    \
        inline T ReadFBInput_##idx(bool var, uint2 coord) {                                             \
        [branch]if(var) { return hlslcc_fbinput_##idx; }                                                \
        else { return _UnityFBInput##idx.Load(uint4(coord, SLICE_ARRAY_INDEX, 0)); }                    \
        }

        #define FRAMEBUFFER_INPUT_X_HALF(idx)                               cbuffer hlslcc_SubpassInput_f_##idx { half4 hlslcc_fbinput_##idx; bool hlslcc_fbfetch_##idx; };    \
                                                                            RENDERPASS_DECLARE_FALLBACK_X(half4, idx)

        #define FRAMEBUFFER_INPUT_X_FLOAT(idx)                              cbuffer hlslcc_SubpassInput_f_##idx { float4 hlslcc_fbinput_##idx; bool hlslcc_fbfetch_##idx; };   \
                                                                            RENDERPASS_DECLARE_FALLBACK_X(float4, idx)

        #define FRAMEBUFFER_INPUT_X_INT(idx)                                cbuffer hlslcc_SubpassInput_f_##idx { int4 hlslcc_fbinput_##idx; bool hlslcc_fbfetch_##idx; };    \
                                                                            RENDERPASS_DECLARE_FALLBACK_X(int4, idx)

        #define FRAMEBUFFER_INPUT_X_UINT(idx)                               cbuffer hlslcc_SubpassInput_f_##idx { uint4 hlslcc_fbinput_##idx; bool hlslcc_fbfetch_##idx; };   \
                                                                            RENDERPASS_DECLARE_FALLBACK_X(uint4, idx)

        #define LOAD_FRAMEBUFFER_X_INPUT(idx, v2fname)                      ReadFBInput_##idx(hlslcc_fbfetch_##idx, uint2(v2fname.xy))

    #elif !defined(PLATFORM_SUPPORTS_NATIVE_RENDERPASS)
        #define FRAMEBUFFER_INPUT_X_HALF(idx)                               TEXTURE2D_X_HALF(_UnityFBInput##idx); float4 _UnityFBInput##idx##_TexelSize
        #define FRAMEBUFFER_INPUT_X_FLOAT(idx)                              TEXTURE2D_X_FLOAT(_UnityFBInput##idx); float4 _UnityFBInput##idx##_TexelSize
        #define FRAMEBUFFER_INPUT_X_INT(idx)                                TEXTURE2D_X_INT(_UnityFBInput##idx); float4 _UnityFBInput##idx##_TexelSize
        #define FRAMEBUFFER_INPUT_X_UINT(idx)                               TEXTURE2D_X_UINT(_UnityFBInput##idx); float4 _UnityFBInput##idx##_TexelSize
        #define LOAD_FRAMEBUFFER_X_INPUT(idx, v2fname)                      _UnityFBInput##idx.Load(uint4(v2fname.xy, SLICE_ARRAY_INDEX, 0))
    #else
        #define FRAMEBUFFER_INPUT_X_HALF(idx)                               FRAMEBUFFER_INPUT_HALF(idx)
        #define FRAMEBUFFER_INPUT_X_FLOAT(idx)                              FRAMEBUFFER_INPUT_FLOAT(idx)
        #define FRAMEBUFFER_INPUT_X_INT(idx)                                FRAMEBUFFER_INPUT_INT(idx)
        #define FRAMEBUFFER_INPUT_X_UINT(idx)                               FRAMEBUFFER_INPUT_UINT(idx)
        #define LOAD_FRAMEBUFFER_X_INPUT(idx, v2fname)                      LOAD_FRAMEBUFFER_INPUT(idx, v2fname)
    #endif

    #define LOAD_TEXTURE2D_X(textureName, unCoord2)                         LOAD_TEXTURE2D_ARRAY(textureName, unCoord2, SLICE_ARRAY_INDEX)
    #define LOAD_TEXTURE2D_X_LOD(textureName, unCoord2, lod)                LOAD_TEXTURE2D_ARRAY_LOD(textureName, unCoord2, SLICE_ARRAY_INDEX, lod)
    #define SAMPLE_TEXTURE2D_X(textureName, samplerName, coord2)            SAMPLE_TEXTURE2D_ARRAY(textureName, samplerName, coord2, SLICE_ARRAY_INDEX)
    #define SAMPLE_TEXTURE2D_X_LOD(textureName, samplerName, coord2, lod)   SAMPLE_TEXTURE2D_ARRAY_LOD(textureName, samplerName, coord2, SLICE_ARRAY_INDEX, lod)
    #define GATHER_TEXTURE2D_X(textureName, samplerName, coord2)            GATHER_TEXTURE2D_ARRAY(textureName, samplerName, coord2, SLICE_ARRAY_INDEX)
    #define GATHER_RED_TEXTURE2D_X(textureName, samplerName, coord2)        GATHER_RED_TEXTURE2D(textureName, samplerName, float3(coord2, SLICE_ARRAY_INDEX))
    #define GATHER_GREEN_TEXTURE2D_X(textureName, samplerName, coord2)      GATHER_GREEN_TEXTURE2D(textureName, samplerName, float3(coord2, SLICE_ARRAY_INDEX))
    #define GATHER_BLUE_TEXTURE2D_X(textureName, samplerName, coord2)       GATHER_BLUE_TEXTURE2D(textureName, samplerName, float3(coord2, SLICE_ARRAY_INDEX))
    #define GATHER_ALPHA_TEXTURE2D_X(textureName, samplerName, coord2)      GATHER_ALPHA_TEXTURE2D(textureName, samplerName, float3(coord2, SLICE_ARRAY_INDEX))

#else
    #define SLICE_ARRAY_INDEX       0

    #define TEXTURE2D_X(textureName)                                        TEXTURE2D(textureName)
    #define TYPED_TEXTURE2D_X(type, textureName)                            TYPED_TEXTURE2D(type, textureName)
    #define TEXTURE2D_X_PARAM(textureName, samplerName)                     TEXTURE2D_PARAM(textureName, samplerName)
    #define TEXTURE2D_X_ARGS(textureName, samplerName)                      TEXTURE2D_ARGS(textureName, samplerName)
    #define TEXTURE2D_X_HALF(textureName)                                   TEXTURE2D_HALF(textureName)
    #define TEXTURE2D_X_FLOAT(textureName)                                  TEXTURE2D_FLOAT(textureName)

    #define FRAMEBUFFER_INPUT_X_HALF(idx)                                   FRAMEBUFFER_INPUT_HALF(idx)
    #define FRAMEBUFFER_INPUT_X_FLOAT(idx)                                  FRAMEBUFFER_INPUT_FLOAT(idx)
    #define FRAMEBUFFER_INPUT_X_INT(idx)                                    FRAMEBUFFER_INPUT_INT(idx)
    #define FRAMEBUFFER_INPUT_X_UINT(idx)                                   FRAMEBUFFER_INPUT_UINT(idx)
    #define LOAD_FRAMEBUFFER_X_INPUT(idx, v2fname)                          LOAD_FRAMEBUFFER_INPUT(idx, v2fname)

    #define LOAD_TEXTURE2D_X(textureName, unCoord2)                         LOAD_TEXTURE2D(textureName, unCoord2)
    #define LOAD_TEXTURE2D_X_LOD(textureName, unCoord2, lod)                LOAD_TEXTURE2D_LOD(textureName, unCoord2, lod)
    #define SAMPLE_TEXTURE2D_X(textureName, samplerName, coord2)            SAMPLE_TEXTURE2D(textureName, samplerName, coord2)
    #define SAMPLE_TEXTURE2D_X_LOD(textureName, samplerName, coord2, lod)   SAMPLE_TEXTURE2D_LOD(textureName, samplerName, coord2, lod)
    #define GATHER_TEXTURE2D_X(textureName, samplerName, coord2)            GATHER_TEXTURE2D(textureName, samplerName, coord2)
    #define GATHER_RED_TEXTURE2D_X(textureName, samplerName, coord2)        GATHER_RED_TEXTURE2D(textureName, samplerName, coord2)
    #define GATHER_GREEN_TEXTURE2D_X(textureName, samplerName, coord2)      GATHER_GREEN_TEXTURE2D(textureName, samplerName, coord2)
    #define GATHER_BLUE_TEXTURE2D_X(textureName, samplerName, coord2)       GATHER_BLUE_TEXTURE2D(textureName, samplerName, coord2)
    #define GATHER_ALPHA_TEXTURE2D_X(textureName, samplerName, coord2)      GATHER_ALPHA_TEXTURE2D(textureName, samplerName, coord2)
#endif