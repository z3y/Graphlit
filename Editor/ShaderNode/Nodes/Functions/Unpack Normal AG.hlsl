
void UnpackNormalAGScale(out float3 Out, float4 Normal, float Scale = 1)
{
    real3 normal;
    normal.xy = Normal.ag * 2.0 - 1.0;
    normal.xy *= Scale;
    normal.z = max(1.0e-16, sqrt(1.0 - saturate(dot(normal.xy, normal.xy))));
    Out = normal;
}