#pragma fragment frag

void frag(Varyings varyings)
{
    UNITY_SETUP_INSTANCE_ID(varyings);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(varyings);
    
    SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(varyings);

    #if defined(_ALPHATEST_ON)
        if (surfaceDescription.Alpha < surfaceDescription.Cutoff) discard;
    #endif

    #if defined(_ALPHAPREMULTIPLY_ON) || defined(_ALPHAMODULATE_ON) || defined(_ALPHAFADE_ON)
        half dither = Unity_Dither(surfaceDescription.Alpha, varyings.positionCS.xy);
        if (dither < 0.0) discard;
    #endif
}