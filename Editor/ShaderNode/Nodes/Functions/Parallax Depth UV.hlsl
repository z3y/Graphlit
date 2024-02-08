
void ParallaxDepthUV(float3 ViewDirectionTS, float Depth, float2 UV, out float2 DepthUV)
{
    DepthUV = UV + ViewDirectionTS.xy * Depth;
}