// #pragma fragment frag

CBUFFER_START(GraphlitMetaPass)
uniform uint graplhit_MetaControl;
CBUFFER_END

float4 frag(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    FragmentData fragment = FragmentData::Create(input);

    SurfaceDescription surface = SurfaceDescriptionFunction(input);

    half3 albedo = surface.Albedo;
    half alpha = surface.Alpha;
    half3 emissionEDF = surface.Emission;
    half metallic = surface.Metallic;
    half perceptualRoughness = surface.Roughness;

    half3 coatTintedEmisionEDF = emissionEDF * surface.CoatColor;
    half3 coatf0 = IorToFresnel0(surface.CoatIOR);
    half3 coatedEmissionEDF = coatf0 * coatTintedEmisionEDF;
    emissionEDF = lerp(emissionEDF, coatedEmissionEDF, surface.CoatWeight);

    perceptualRoughness = ComputeCoatAffectedRoughness(perceptualRoughness, surface.CoatRoughness, surface.CoatWeight);
    half3 coatAttenuation = lerp(1.0, surface.CoatColor, surface.CoatWeight);
    albedo *= coatAttenuation;

    half3 diffuse = albedo * (1.0 - metallic);
    half dielectricSpecularF0 = IorToFresnel0(surface.IOR);
    half3 specular = albedo * metallic + dielectricSpecularF0 * (1.0 - metallic);

    UnityMetaInput meta;
    ZERO_INITIALIZE(UnityMetaInput, meta);
    meta.Albedo = diffuse + specular * perceptualRoughness * perceptualRoughness * 0.5;
    meta.Emission = emissionEDF;
    #ifdef EDITOR_VISUALIZATION
        meta.VizUV = input.VizUV;
        meta.LightCoord = input.LightCoord;
        if (graplhit_MetaControl == 1)
        {
            return float4(meta.Albedo, alpha);
        }
        if (graplhit_MetaControl == 2)
        {
            return float4(meta.Emission, alpha);
        }
    #endif

    return UnityMetaFragment(meta);
}