void GeometricSpecularAA(half Roughness, out half RoughnessAA, float3 NormalWS, half Variance = 0.15, half Threshold = 0.1)
{
    float3 du = ddx(NormalWS);
    float3 dv = ddy(NormalWS);

    half variance = Variance * (dot(du, du) + dot(dv, dv));

    half roughness = Roughness * Roughness;
    half kernelRoughness = min(2.0 * variance, Threshold);
    half squareRoughness = saturate(roughness * roughness + kernelRoughness);

    RoughnessAA = sqrt(sqrt(squareRoughness));
}