void Fresnel(out half fresnel, float3 viewDirectionWS, float3 normalWS, half power = 1)
{
    half Dot = 1.0 - saturate(dot(normalWS, viewDirectionWS));
    fresnel = pow(Dot, power);
}