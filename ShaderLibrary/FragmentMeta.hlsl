#pragma fragment frag

float4 frag(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    FragmentData fragment = FragmentData::Create(input);

    SurfaceDescription surface = SurfaceDescriptionFunction(input);

    half3 albedo = surface.Albedo;
    half alpha = surface.Alpha;
    half3 emission = surface.Emission;
    half metallic = surface.Metallic;
    half reflectance = 0.5;
    half perceptualRoughness = surface.Roughness;

    half3 diffuse = albedo * (1.0 - metallic);
    half dielectricSpecularF0 = 0.16 * reflectance * reflectance;
    half3 specular = albedo * metallic + dielectricSpecularF0 * (1.0 - metallic);

    UnityMetaInput meta;
    ZERO_INITIALIZE(UnityMetaInput, meta);
    meta.Albedo = diffuse + specular * max(perceptualRoughness * perceptualRoughness, HALF_MIN_SQRT) * 0.5;
    meta.Emission = emission;
    #ifdef EDITOR_VISUALIZATION
        meta.VizUV = input.VizUV;
        meta.LightCoord = input.LightCoord;
    #endif

    return UnityMetaFragment(meta);
}