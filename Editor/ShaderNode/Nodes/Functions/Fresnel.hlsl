void Fresnel(out half Fresnel, float3 NormalWS, float3 ViewDirectionWS, half Power = 1)
{
    half Dot = 1.0 - saturate(dot(NormalWS, ViewDirectionWS));
    Fresnel = pow(Dot, Power);
}