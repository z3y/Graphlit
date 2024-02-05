void SharpUV(float4 TexelSize, float2 UV, out float2 SharpUV)
{
    UV = UV * TexelSize.zw;
    float2 c = max(0.0, abs(fwidth(UV)));
    UV = UV + abs(c);
    UV = floor(UV) + saturate(frac(UV) / c);
    UV = (UV - 0.5) * TexelSize.xy;
    SharpUV = UV;
}

// MIT License
// https://gitlab.com/s-ilent/pixelstandard/-/blob/master/Assets/Shaders/PixelStandard/UnityStandardInput.cginc?ref_type=heads