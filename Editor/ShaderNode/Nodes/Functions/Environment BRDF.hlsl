half3 EnvironmentBRDFApproximation_1(half perceptualRoughness, half NoV, half3 f0)
{
	// original code from https://blog.selfshadow.com/publications/s2013-shading-course/lazarov/s2013_pbs_black_ops_2_notes.pdf
	half g = 1 - perceptualRoughness;
	half4 t = half4(1 / 0.96, 0.475, (0.0275 - 0.25 * 0.04) / 0.96, 0.25);
	t *= half4(g, g, g, g);
	t += half4(0.0, 0.0, (0.015 - 0.75 * 0.04) / 0.96, 0.75);
	half a0 = t.x * min(t.y, exp2(-9.28 * NoV)) + t.z;
	half a1 = t.w;
	return saturate(lerp(a0, a1, f0));
}

void EnvironmentBRDFNode(out float3 BRDF, out half3 energyCompensation, float3 normalWS, float3 viewDirectionWS, half metallic, half roughness, half reflectance = 0.5, half3 albedo = 1)
{
	half NoV = abs(dot(normalWS, viewDirectionWS)) + 1e-5f;
  	half3 f0 = 0.16 * reflectance * reflectance * (1.0 - metallic) + albedo * metallic;

    #if defined(QUALITY_LOW) || defined(PREVIEW)
		energyCompensation = 1.0;
		BRDF = EnvironmentBRDFApproximation_1(roughness, NoV, f0);
	#else
        float2 dfg = SAMPLE_TEXTURE2D_LOD(_DFG, sampler_BilinearClamp, float2(NoV, roughness), 0).rg;

		BRDF = lerp(dfg.xxx, dfg.yyy, f0);
		energyCompensation = 1.0 + f0 * (1.0 / dfg.y - 1.0);
	#endif
}