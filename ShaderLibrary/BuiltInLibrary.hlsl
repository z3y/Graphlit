
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
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

#ifdef PREVIEW
    #include "Packages/com.enlit/Editor/Targets/Graph.hlsl"
#endif

#include "UnityCG/UnityCG.hlsl"
#include "AutoLight.cginc"

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

#include "Packages/com.enlit/ShaderLibrary/GraphFunctions.hlsl"
#include "BlendModes.hlsl"