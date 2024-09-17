void SpecularAONode(out half AO, float3 normalWS, float3 viewDirectionWS, half occlusion = 1, half roughness = 0)
{
	half NoV = abs(dot(normalWS, viewDirectionWS)) + 1e-5f;

	#ifdef QUALITY_LOW
		AO = occlusion;
	#else
		AO = clamp(pow(NoV + occlusion, exp2(-16.0 * roughness - 1.0)) - 1.0 + occlusion, 0.0, 1.0);
	#endif
}