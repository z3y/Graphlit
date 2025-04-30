#pragma fragment frag

half4 frag(Varyings varyings) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(varyings);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(varyings);
    
    SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(varyings);

    #if defined(_ALPHATEST_ON)
        if (surfaceDescription.Alpha < surfaceDescription.Cutoff) discard;
    #endif

    // #if defined(_ALPHAPREMULTIPLY_ON)
        // surfaceDescription.Color *= surfaceDescription.Alpha;
        // surfaceDescription.Alpha = lerp(surfaceDescription.Alpha, 1.0, surfaceDescription.Metallic);
    // #endif

    // #if defined(_ALPHAMODULATE_ON)
        // surfaceDescription.Color = lerp(1.0, surfaceDescription.Color, surfaceDescription.Alpha);
    // #endif

    #ifdef RETURN_COLOR
        return surfaceDescription.Color;
    #else
        half4 col;
        col.rgb = surfaceDescription.Color;
        col.a = surfaceDescription.Alpha;

        #if !defined(_ALPHATEST_ON) && !defined(_ALPHAPREMULTIPLY_ON) && !defined(_ALPHAMODULATE_ON) && !defined(_ALPHAFADE_ON)
            col.a = 1.0;
        #endif

        #ifdef _ALPHATEST_ON
            col.a = 1.0;
        #endif

        #ifdef UNIVERSALRP
        // todo: urp fog
        #else
        UNITY_APPLY_FOG(varyings.fogCoord, col);
        #endif

        return col;
    #endif
}