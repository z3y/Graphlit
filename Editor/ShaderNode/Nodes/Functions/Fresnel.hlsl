void Fresnel(out half fresnel_0, float3 viewDirectionWS_2, float3 normalWS_1, half power_3 = 1)
{
    half Dot = 1.0 - saturate(dot(normalWS_1, viewDirectionWS_2));
    fresnel_0 = pow(Dot, power_3);
}