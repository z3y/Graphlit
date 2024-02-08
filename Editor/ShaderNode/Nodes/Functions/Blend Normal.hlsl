
void UnpackNormalScale(out float3 Out, float3 Normal1, float3 Normal2)
{
    Out = BlendNormal(Normal1, Normal2);
}