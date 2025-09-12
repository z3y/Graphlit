void EnvironmentReflectionNode(out half3 Specular, float3 normalWS, float3 positionWS, float3 viewDirectionWS, half roughness, half3 BRDF = 1, half energyCompensation = 1)
{
	#ifdef PREVIEW
	Specular = .15;
	return;
	#endif

	#ifdef UNITY_PASS_FORWARDBASE
		half roughness2 = roughness * roughness;
		float3 reflectVector = reflect(-viewDirectionWS, normalWS);

		#if !defined(QUALITY_LOW)
			reflectVector = lerp(reflectVector, normalWS, roughness2);
		#endif
		float3 vertexNormalWS = normalWS;

		// #if !defined(_GLOSSYREFLECTIONS_OFF) // probably shouldnt need a hardcoded define
			Specular = CalculateIrradianceFromReflectionProbes(reflectVector, positionWS, roughness, 0, vertexNormalWS);

			Specular *= BRDF * energyCompensation;
		// #endif
	#else
		Specular = 0;
	#endif
}