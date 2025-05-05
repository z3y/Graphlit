#ifdef UNITY_SAMPLE_FULL_SH_PER_PIXEL
#undef UNITY_SAMPLE_FULL_SH_PER_PIXEL
#endif
#define UNITY_SAMPLE_FULL_SH_PER_PIXEL 1

float4 GetFlatNormal()
{
    // todo: find proper defines for normal map packing
    #if defined(UNIVERSALRP) && defined(TARGET_ANDROID)
    return float4(0.5, 0.5, 1, 0.5);
    #else
    return float4(0.5, 0.5, 1, 1);
    #endif
}

#ifdef UNIVERSALRP
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
    // #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
    // #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ParallaxMapping.hlsl"
    // #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"

#else // built in stuff
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
        
    #include "UnityCG/ShaderVariablesMatrixDefsLegacyUnity.hlsl"

    // fix macro redefinition warnings
    #undef GLOBAL_CBUFFER_START
    #undef SAMPLE_DEPTH_TEXTURE
    #undef SAMPLE_DEPTH_TEXTURE_LOD
    #undef UNITY_MATRIX_P
    #undef UNITY_MATRIX_MVP
    #undef UNITY_MATRIX_MV
    #undef UNITY_MATRIX_T_MV
    #undef UNITY_MATRIX_IT_MV

    #define pos positionCS
    #define vertex positionOS
    #define normal normalOS

    #include "UnityShaderVariables.cginc"
    half4 _LightColor0;
    half4 _SpecColor;
    #include "UnityShaderUtilities.cginc"
#endif

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

#ifdef PREVIEW
    #include "Packages/com.z3y.graphlit/Editor/Targets/Graph.hlsl"
#endif

#ifdef UNIVERSALRP
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
    float3 _LightDirection; // just like that ?
    float3 _LightPosition;
    #include "URPProbes.hlsl"
    
    // copy for version compatibility
    half IsDirectionalLight_Local()
    {
        return round(_ShadowBias.z) == 1.0 ? 1 : 0;
    }
    float4 ApplyShadowClamping_Local(float4 positionCS)
    {
        #if UNITY_REVERSED_Z
            float clamped = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
        #else
            float clamped = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
        #endif

        // The current implementation of vertex clamping in Universal RP is the same as in Unity Built-In RP.
        // We follow the same convention in Universal RP where it's only enabled for Directional Lights
        // (see: Shadows.cpp::RenderShadowMaps() #L2161-L2162)
        // (see: Shadows.cpp::RenderShadowMaps() #L2086-L2102)
        // (see: Shadows.cpp::PrepareStateForShadowMap() #L1685-L1686)
        positionCS.z = lerp(positionCS.z, clamped, IsDirectionalLight_Local());

        return positionCS;
    }

#else
    #include "UnityCG/UnityCG.hlsl"
    #include "AutoLight.cginc"
    #include "LightAttenuation.hlsl"
#endif

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

#include "Packages/com.z3y.graphlit/ShaderLibrary/GraphFunctions.hlsl"
#include "BlendModes.hlsl"