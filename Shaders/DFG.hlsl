// from CoreRP
// Unity Companion License
real4 IntegrateGGXAndDisneyDiffuseFGD_F82(real NdotV, real roughness, uint sampleCount = 4096)
{
    // Note that our LUT covers the full [0, 1] range.
    // Therefore, we don't really want to clamp NdotV here (else the lerp slope is wrong).
    // However, if NdotV is 0, the integral is 0, so that's not what we want, either.
    // Our runtime NdotV bias is quite large, so we use a smaller one here instead.
    NdotV     = max(NdotV, REAL_EPS);
    real3 V   = real3(sqrt(1 - NdotV * NdotV), 0, NdotV);
    real4 acc = real4(0.0, 0.0, 0.0, 0.0);

    real3x3 localToWorld = k_identity3x3;

	const float COS_THETA_MAX = 1.0 / 7.0;
    const float COS_THETA_FACTOR = 1.0 / (COS_THETA_MAX * pow(1.0 - COS_THETA_MAX, 6.0));

    for (uint i = 0; i < sampleCount; ++i)
    {
        real2 u = Hammersley2d(i, sampleCount);

        real VdotH;
        real NdotL;
        real weightOverPdf;

        real3 L; // Unused
        ImportanceSampleGGX(u, V, localToWorld, roughness, NdotV,
                            L, VdotH, NdotL, weightOverPdf);

        if (NdotL > 0.0)
        {
            // Integral{BSDF * <N,L> dw} =
            // Integral{(F0 + (1 - F0) * (1 - <V,H>)^5) * (BSDF / F) * <N,L> dw} =
            // (1 - F0) * Integral{(1 - <V,H>)^5 * (BSDF / F) * <N,L> dw} + F0 * Integral{(BSDF / F) * <N,L> dw}=
            // (1 - F0) * x + F0 * y = lerp(x, y, F0)

            acc.x += weightOverPdf * pow(1 - VdotH, 5);
            acc.y += weightOverPdf;
            acc.a += weightOverPdf * pow(1 - VdotH, 6) * VdotH; // f82
        }

        // for Disney we still use a Cosine importance sampling, true Disney importance sampling imply a look up table
        ImportanceSampleLambert(u, localToWorld, L, NdotL, weightOverPdf);

        if (NdotL > 0.0)
        {
            real LdotV = dot(L, V);
            real disneyDiffuse = DisneyDiffuseNoPI(NdotV, NdotL, LdotV, RoughnessToPerceptualRoughness(roughness));

            acc.z += disneyDiffuse * weightOverPdf;
        }
    }

    acc /= sampleCount;

    // Remap from the [0.5, 1.5] to the [0, 1] range.
    acc.z -= 0.5;

    return acc;
}

void DFG(float2 UV, out float4 Out)
{
	Out = 0;
	float NdotV = UV.x * UV.x;
	// float NdotV = UV.x;
	float perceptualRoughness = UV.y;

	float4 dfg = IntegrateGGXAndDisneyDiffuseFGD_F82(NdotV, PerceptualRoughnessToRoughness(perceptualRoughness));

	Out.rgb = dfg.rga;

    Out = saturate(Out);
}