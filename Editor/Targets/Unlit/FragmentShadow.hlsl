#pragma fragment frag

void frag(Varyings varyings)
{
    UNITY_SETUP_INSTANCE_ID(varyings);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(varyings);
    
    SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(varyings);

    #if defined(_ALPHATEST_ON)
        if (surfaceDescription.Alpha < surfaceDescription.Cutoff) discard;
    #endif
}