void EnvironmentBRDFNode(out float3 BRDF, out half3 energyCompensation, float3 normalWS, float3 viewDirectionWS, half metallic, half roughness, half reflectance = 0.5, half3 albedo = 1)
{
    half NoV = abs(dot(normalWS, viewDirectionWS)) + 1e-5f;
  	half3 f0 = 0.16 * reflectance * reflectance * (1.0 - metallic) + albedo * metallic;

    half3 invBrdf = 0;
    EnvironmentBRDF(NoV, roughness, f0, BRDF, invBrdf, energyCompensation);
}