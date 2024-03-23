void UVScaleCenter(float2 UV, float Scale, out float2 Out)
{
    float s = 1-Scale;
    UV *= s; 
    UV += 0.5 * Scale;
    Out = UV;
}