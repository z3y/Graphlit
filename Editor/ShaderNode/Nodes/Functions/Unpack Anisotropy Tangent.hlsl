
void UnpackNormalAGScale(out float3 Out, float4 Tangent)
{
    half4 tangentMap = half4(1, Tangent.g, 1, Tangent.a);
    real3 normal;
    normal.xy = tangentMap.ag * 2.0 - 1.0;
    normal.z = max(1.0e-16, sqrt(1.0 - saturate(dot(normal.xy, normal.xy))));
    Out = normal;
}