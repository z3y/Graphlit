#pragma once

void MainLightData(float4 ShadowCoord, float2 LightmapUV, float3 PositionWS, out float3 Color, out float3 Direction, out float DistanceAttenuation, out float ShadowAttenuation)
{

	#ifdef UNIVERSALRP
	#else
		struct FakeVertexInput
		{
		float4 _ShadowCoord;
		} i;

		i._ShadowCoord = ShadowCoord;

		Direction = SafeNormalize(UnityWorldSpaceLightDir(PositionWS));
		Color = _LightColor0.rgb;
	#endif
	DistanceAttenuation = 1.0;
	ShadowAttenuation = 1.0;

	#ifdef PREVIEW
		Direction = normalize(float3(1,1,0));
		Color = 1.0;
	#else
		#ifdef UNIVERSALRP

			Light urpLight = GetMainLight(ShadowCoord);
			Color = urpLight.color;
			Direction = urpLight.direction;
			DistanceAttenuation = urpLight.distanceAttenuation;
			ShadowAttenuation = urpLight.shadowAttenuation;
		#else
			{
				ShadowAttenuation = UNITY_SHADOW_ATTENUATION(i, PositionWS.xyz);
			}
			{
				UNITY_LIGHT_DISTANCE_ATTENUATION(distanceAttenuation, i, PositionWS.xyz);
				DistanceAttenuation = distanceAttenuation;
			}


			#if defined(HANDLE_SHADOWS_BLENDING_IN_GI) && defined(SHADOWS_SCREEN) && defined(LIGHTMAP_ON)
				half bakedAtten = UnitySampleBakedOcclusion(LightmapUV, PositionWS);
				float zDist = dot(_WorldSpaceCameraPos -  PositionWS, UNITY_MATRIX_V[2].xyz);
				float fadeDist = UnityComputeShadowFadeDistance(PositionWS, zDist);
				ShadowAttenuation = UnityMixRealtimeAndBakedShadows(ShadowAttenuation, bakedAtten, UnityComputeShadowFade(fadeDist));
			#endif

			#if defined(UNITY_PASS_FORWARDBASE) && !defined(SHADOWS_SCREEN) && !defined(SHADOWS_SHADOWMASK)
				ShadowAttenuation = 1.0;
			#endif

			#if defined(LIGHTMAP_SHADOW_MIXING) && defined(LIGHTMAP_ON)
				Color *= UnityComputeForwardShadows(LightmapUV.xy, PositionWS, i._ShadowCoord);
			#endif
		#endif
	#endif
}