
#ifdef POINT
    #define UNITY_LIGHT_DISTANCE_ATTENUATION(destName, input, worldPos) \
    unityShadowCoord3 lightCoord = mul(unity_WorldToLight, unityShadowCoord4(worldPos, 1)).xyz; \
    fixed destName = tex2D(_LightTexture0, dot(lightCoord, lightCoord).rr).r;
#endif

#ifdef SPOT
    #define UNITY_LIGHT_DISTANCE_ATTENUATION(destName, input, worldPos) \
    DECLARE_LIGHT_COORD(input, worldPos); \
    fixed destName = (lightCoord.z > 0) * UnitySpotCookie(lightCoord) * UnitySpotAttenuate(lightCoord.xyz);
#endif

#ifdef DIRECTIONAL
    #define UNITY_LIGHT_DISTANCE_ATTENUATION(destName, input, worldPos) fixed destName = 1.0;
#endif

#ifdef POINT_COOKIE
    #define UNITY_LIGHT_DISTANCE_ATTENUATION(destName, input, worldPos) \
        DECLARE_LIGHT_COORD(input, worldPos); \
        fixed destName = tex2D(_LightTextureB0, dot(lightCoord, lightCoord).rr).r * texCUBE(_LightTexture0, lightCoord).w;
#endif

#ifdef DIRECTIONAL_COOKIE
    #define UNITY_LIGHT_DISTANCE_ATTENUATION(destName, input, worldPos) \
    DECLARE_LIGHT_COORD(input, worldPos); \
    fixed destName = tex2D(_LightTexture0, lightCoord).w;
#endif