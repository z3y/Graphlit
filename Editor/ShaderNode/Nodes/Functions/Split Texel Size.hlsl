void SplitTexelSize(float4 TexelSize, out float Width, out float Height)
{
    Width = TexelSize.z;
    Height = TexelSize.w;
}