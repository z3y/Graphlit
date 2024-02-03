void TransformTexture(float2 UV, float4 ScaleOffset, out float2 Out)
{
    Out = mad(UV, ScaleOffset.xy, ScaleOffset.zw);
}