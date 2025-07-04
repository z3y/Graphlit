// #pragma fragment frag

half4 frag(Varyings varyings) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(varyings);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(varyings);
    
    SurfaceDescription surf = SurfaceDescriptionFunction(varyings);

    #if defined(_ALPHATEST_ON)
        if (surf.Alpha < surf.Cutoff) discard;
    #endif

    FragmentData fragData = FragmentData::Create(varyings);

    float3 normalWS;

    #ifdef UNLIT_TEMPLATE // todo: figure out something for unlit template
        #ifndef _NORMAL_DROPOFF_OFF
            #define _NORMAL_DROPOFF_OFF
        #endif
    #endif

    #if defined(_NORMAL_DROPOFF_OFF)
        normalWS = fragData.normalWS;
    #elif defined(_NORMAL_DROPOFF_WS)
        normalWS = surf.Normal;
    #elif defined(_NORMAL_DROPOFF_OS)
        normalWS = TransformObjectToWorldNormal(surf.Normal);
    #else // _NORMAL_DROPOFF_TS
        normalWS = SafeNormalize(mul(surf.Normal, fragData.tangentSpaceTransform));
    #endif

    return half4(normalWS, 0.0);
}