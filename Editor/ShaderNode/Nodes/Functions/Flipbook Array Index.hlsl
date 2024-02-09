void FlipbookArrayIndex(out float Index, Texture2DArray Texture, half Speed = 1)
{
    uint3 dimensions;
    Texture.GetDimensions(dimensions.x, dimensions.y, dimensions.z);
    uint indexLoop = frac(_Time.x * Speed) * dimensions.z;

    Index = indexLoop;
}