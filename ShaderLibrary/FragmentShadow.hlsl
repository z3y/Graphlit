#pragma fragment frag

void frag(Varyings input)
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    
    SurfaceDescription surface = SurfaceDescriptionFunction(input);

    #if defined(_ALPHATEST_ON)
        half alpha = AlphaClip(surface.Alpha, surface.Cutoff);
    #elif defined(_SURFACE_TYPE_TRANSPARENT)
        half alpha = surface.Alpha;
    #else
        half alpha = half(1.0);
    #endif

    #if defined(_SURFACE_TYPE_TRANSPARENT)
        #if defined(SHADOWS_DEPTH)
            if(UNITY_MATRIX_P._m33 != 0.0) // thanks liltoon
        #endif
        {
        // half dither = Unity_Dither(alpha, input.positionCS.xy / _ScreenParams.xy);
        // if (dither < 0.0) discard;
        }

    #endif
}