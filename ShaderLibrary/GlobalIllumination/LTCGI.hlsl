

#ifdef _LTCGI

float3 DecodeLightmapLTCGI(float4 lm)
{
    return lm.rgb;
}
#define UNITY_HALF_PI HALF_PI
#define UNITY_PI PI
#define UNITY_TWO_PI TWO_PI
#define DecodeLightmap DecodeLightmapLTCGI
#include "Assets/_pi_/_LTCGI/Shaders/LTCGI.cginc"
#undef DecodeLightmap
#undef UNITY_HALF_PI
#undef UNITY_PI
#undef UNITY_TWO_PI



void GetLTCGIDiffuseAndSpecular(inout float3 diffuse, inout float3 specular, ShadingData shading, FragmentData fragment, SurfaceDescription surf)
{
    float2 untransformedLightmapUV = 0;
    #ifdef LIGHTMAP_ON
    untransformedLightmapUV = (fragment.lightmapUV.xy - unity_LightmapST.zw) / unity_LightmapST.xy;
    #endif
    float3 ltcgiDiffuse = 0;
    float3 ltcgiSpecular = 0;
    LTCGI_Contribution(fragment.positionWS.xyz, shading.normalWS, fragment.viewDirectionWS, surf.Roughness, untransformedLightmapUV, ltcgiDiffuse, ltcgiSpecular);
    #ifndef LTCGI_DIFFUSE_DISABLED
        diffuse += ltcgiDiffuse;
    #endif
    specular += ltcgiSpecular;
}
#endif