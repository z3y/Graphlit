#ifdef PREVIEW
    #ifdef PREVIEW3D
        #define _WorldSpaceCameraPos float3(0, 0, -2.23)
    #else
        #define _WorldSpaceCameraPos float3(0, 0, -1)
    #endif

    float4 _GraphTime;
    #define _Time _GraphTime
#endif