#pragma fragment frag

float4 frag(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    FragmentData fragment = FragmentData::Create(input);

    SurfaceDescription surface = SurfaceDescriptionFunction(input);


    #if defined(_ALPHATEST_ON)
        half alpha = AlphaClip(surface.Alpha, surface.Cutoff);
    #elif defined(_SURFACE_TYPE_TRANSPARENT)
        half alpha = surface.Alpha;
    #else
        half alpha = half(1.0);
    #endif

    half3 diffuseColor = surface.Color;
    #if defined(_ALPHAMODULATE_ON)
        diffuseColor = AlphaModulate(diffuseColor, alpha);
    #endif
    #if defined(_ALPHAPREMULTIPLY_ON)
        diffuseColor *= alpha;
    #endif

    float4 color = float4(diffuseColor, alpha);

    color.rgb = MixFog(color.rgb, InitializeInputDataFog(float4(fragment.positionWS, 1), 0));

    #if defined(_SURFACE_TYPE_TRANSPARENT)
        bool isTransparent = true;
    #else
        bool isTransparent = false;
    #endif
    
    color.a = OutputAlpha(color.a, isTransparent);

    return color;
}