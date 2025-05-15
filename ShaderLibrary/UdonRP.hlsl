#pragma once

// #define _UDONRP_ENVIRONMENT_PROBE

#ifdef _UDONRP_DIRECTIONAL_COOKIE
float4x4 _UdonRPWorldToDirectionalLight;
TEXTURE2D(_UdonRPDirectionalCookie);
SAMPLER(sampler_UdonRPDirectionalCookie);
#endif

#ifdef _UDONRP_ENVIRONMENT_PROBE
TEXTURECUBE(_UdonRPGlossyEnvironmentCubeMap);
#define _GlossyEnvironmentCubeMap _UdonRPGlossyEnvironmentCubeMap
#define ENABLE_ENVIRONMENT_PROBE
#define _GlossyEnvironmentCubeMap_HDR float4(1, 1, 0, 0)
#endif

half4 SampleUdonRPDirectionalCookie(float3 positionWS)
{
    #ifdef _UDONRP_DIRECTIONAL_COOKIE
        float2 cookieUV = mul(_UdonRPWorldToDirectionalLight, float4(positionWS, 1)).xy;
        half4 cookieTexture = SAMPLE_TEXTURE2D(_UdonRPDirectionalCookie, sampler_UdonRPDirectionalCookie, cookieUV);
        return cookieTexture;
    #else
        return 1.0;
    #endif
}