#ifdef PREVIEW
    bool _Preview3D;
    uint _PreviewID;

    // #ifdef PREVIEW3D
    //     #define _WorldSpaceCameraPos float3(0, 0, -2.23)
    // #else
    //     #define _WorldSpaceCameraPos float3(0, 0, -1)
    // #endif

    #define _WorldSpaceCameraPos float3(_Preview3D ? float3(0, 0, -2.23) : float3(0, 0, -1))

    float4 _GraphTime;
    #define _Time _GraphTime
#endif