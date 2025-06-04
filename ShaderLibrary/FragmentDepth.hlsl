// #pragma fragment frag

half frag(Varyings varyings) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(varyings);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(varyings);
    
    SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(varyings);

    #if defined(_ALPHATEST_ON)
        if (surfaceDescription.Alpha < surfaceDescription.Cutoff) discard;
    #endif

    #if defined(_ALPHAPREMULTIPLY_ON) || defined(_ALPHAMODULATE_ON) || defined(_ALPHAFADE_ON)
        #if defined(SHADOWS_DEPTH)
            if(UNITY_MATRIX_P._m33 != 0.0) // thanks liltoon
        #endif
        {
        // half dither = Unity_Dither(surfaceDescription.Alpha, varyings.positionCS.xy / _ScreenParams.xy);
        // if (dither < 0.0) discard;
        }

    #endif

    return varyings.positionCS.z;
}