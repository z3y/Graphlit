#pragma fragment frag

void frag(Varyings varyings)
{
    UNITY_SETUP_INSTANCE_ID(varyings);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(varyings);
    
    SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(varyings);
    half alpha = surfaceDescription.Alpha;

    // if (alpha < 0.5) discard;
}