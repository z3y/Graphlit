
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
    
#include "UnityCG/ShaderVariablesMatrixDefsLegacyUnity.hlsl"
    
#undef GLOBAL_CBUFFER_START // dont need reg
#define GLOBAL_CBUFFER_START(name) CBUFFER_START(name)

// fix macro redefinition warnings
#undef SAMPLE_DEPTH_TEXTURE
#undef SAMPLE_DEPTH_TEXTURE_LOD
#undef UNITY_MATRIX_P
#undef UNITY_MATRIX_MVP
#undef UNITY_MATRIX_MV
#undef UNITY_MATRIX_T_MV
#undef UNITY_MATRIX_IT_MV

#include "UnityShaderVariables.cginc"

// global built in variables
half4 _LightColor0;
half4 _SpecColor;

#ifdef PREVIEW
    #include "Packages/com.z3y.myshadergraph/Editor/Targets/Graph.hlsl"
#endif

#include "UnityCG/UnityCG.hlsl"
#include "AutoLight.cginc"

#include "UnityShaderUtilities.cginc"

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"