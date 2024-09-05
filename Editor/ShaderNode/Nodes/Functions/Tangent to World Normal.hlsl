void TangentToWorldNormal(out float3 NormalWS, float3 NormalTS, float3x3 TangentSpaceTransform)
{
    NormalWS = SafeNormalize(mul(NormalTS, TangentSpaceTransform));
}