#pragma fragment frag

#include "Packages/com.enlit/ShaderLibrary/GraphFunctions.hlsl"

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
    UNITY_INITIALIZE_OUTPUT(UnityMetaInput, o);

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
    
    o.SpecularColor = specColor;
    o.Emission = desc.Emission;

    #if defined(_ALPHATEST_ON)
        clip(desc.Alpha - desc.Cutoff);
    #endif
    
    return UnityMetaFragment(o);
}