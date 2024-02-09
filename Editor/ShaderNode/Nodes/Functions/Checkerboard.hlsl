void Checkerboard(float2 UV, out half3 Out, half3 Color0 = 0.3, half3 Color1 = 0.4, float2 Tiling = 8)
{
    float2 checkerUV = UV * Tiling;
    float checkerboard = floor(checkerUV.x) + floor(checkerUV.y);
    checkerboard = frac(checkerboard * 0.5);

    Out = lerp(Color0, Color1, checkerboard);
}