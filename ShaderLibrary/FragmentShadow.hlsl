#pragma fragment frag

void frag(Varyings input)
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    
    SurfaceDescription surface = SurfaceDescriptionFunction(input);

    #if defined(_ALPHATEST_ON)
        if (surface.Alpha < surface.Cutoff) discard;
    #endif

    #if defined(_SURFACE_TYPE_TRANSPARENT)
        #if defined(SHADOWS_DEPTH)
            if(UNITY_MATRIX_P._m33 != 0.0) // thanks liltoon
        #endif
        {
        half dither = Unity_Dither(surface.Alpha, input.positionCS.xy / _ScreenParams.xy);
        if (dither < 0.0) discard;
        }

    #endif
}