#pragma fragment frag

#include "Packages/com.z3y.graphlit/ShaderLibrary/GraphFunctions.hlsl"

#ifdef UNIVERSALRP
    #ifdef UNITY_COLORSPACE_GAMMA
    #define unity_ColorSpaceDielectricSpec half4(0.220916301, 0.220916301, 0.220916301, 1.0 - 0.220916301)
    #else // Linear values
    #define unity_ColorSpaceDielectricSpec half4(0.04, 0.04, 0.04, 1.0 - 0.04) // standard dielectric reflectivity coef at incident angle (= 4%)
    #endif
#endif

half3 LightmappingAlbedo(half3 diffuse, half3 specular, half roughness)
{
    half3 res = diffuse;
    res += specular * roughness * 0.5;
    return res;
}

half OneMinusReflectivityFromMetallic(half metallic)
{
    half oneMinusDielectricSpec = unity_ColorSpaceDielectricSpec.a;
    return oneMinusDielectricSpec - metallic * oneMinusDielectricSpec;
}

half3 DiffuseAndSpecularFromMetallic (half3 albedo, half metallic, out half3 specColor, out half oneMinusReflectivity)
{
    specColor = lerp (unity_ColorSpaceDielectricSpec.rgb, albedo, metallic);
    oneMinusReflectivity = OneMinusReflectivityFromMetallic(metallic);
    return albedo * oneMinusReflectivity;
}

half4 frag(Varyings varyings) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(varyings);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(varyings);
    
    SurfaceDescription desc = SurfaceDescriptionFunction(varyings);
    #if !defined(_ALPHATEST_ON) && !defined(_ALPHAPREMULTIPLY_ON) && !defined(_ALPHAMODULATE_ON) && !defined(_ALPHAFADE_ON)
        desc.Alpha = 1.0;
    #endif

    UnityMetaInput o;
    ZERO_INITIALIZE(UnityMetaInput, o);

    half3 specColor;
    half oneMinisReflectivity;
    half3 diffuseColor = DiffuseAndSpecularFromMetallic(desc.Albedo, desc.Metallic, specColor, oneMinisReflectivity);

    #ifdef EDITOR_VISUALIZATION
        o.Albedo = diffuseColor;
        o.VizUV = varyings.vizUV;
        o.LightCoord = varyings.lightCoord;
    #else
        o.Albedo = LightmappingAlbedo(diffuseColor, specColor, desc.Roughness);
    #endif
    
    // o.SpecularColor = specColor;
    o.Emission = desc.Emission;

    #if defined(_ALPHATEST_ON)
        clip(desc.Alpha - desc.Cutoff);
    #endif
    
    return UnityMetaFragment(o);
}