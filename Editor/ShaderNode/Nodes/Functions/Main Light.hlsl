void MainLightData(float2 lightmapUV_8, float3 positionWS_0, float3 normalWS_1, out float3 direction_2, out float3 color_3, out float attenuation_6, float4 shadowCoord_7)
{
  float3 positionWS = positionWS_0;
  float2 lightmapUV = lightmapUV_8;

  struct FakeVertexInput
  {
    float4 _ShadowCoord;
  } i;

  i._ShadowCoord = shadowCoord_7;

  direction_2 = SafeNormalize(UnityWorldSpaceLightDir(positionWS));
  color_3 = _LightColor0.rgb;
  attenuation_6 = 1.0;

  #ifdef PREVIEW
  direction_2 = normalize(float3(1,1,0));
  color_3 = 1.0;
  #else 
  
    UNITY_LIGHT_ATTENUATION(attenuation, i, positionWS.xyz);

    #if defined(HANDLE_SHADOWS_BLENDING_IN_GI) && defined(SHADOWS_SCREEN) && defined(LIGHTMAP_ON)
        half bakedAtten = UnitySampleBakedOcclusion(lightmapUV, positionWS);
        float zDist = dot(_WorldSpaceCameraPos -  positionWS, UNITY_MATRIX_V[2].xyz);
        float fadeDist = UnityComputeShadowFadeDistance(positionWS, zDist);
        attenuation = UnityMixRealtimeAndBakedShadows(attenuation, bakedAtten, UnityComputeShadowFade(fadeDist));
    #endif

    #if defined(UNITY_PASS_FORWARDBASE) && !defined(SHADOWS_SCREEN) && !defined(SHADOWS_SHADOWMASK)
        attenuation = 1.0;
    #endif

    attenuation_6 = attenuation;

    #if defined(LIGHTMAP_SHADOW_MIXING) && defined(LIGHTMAP_ON)
        color_3 *= UnityComputeForwardShadows(lightmapUV.xy, positionWS, i._ShadowCoord);
    #endif
  #endif
}