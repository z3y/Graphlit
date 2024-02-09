void Flipbook(out half4 Out, Texture2DArray Texture, SamplerState Sampler, float2 UV, half Speed = 1)
{
    uint3 dimensions;
    Texture.GetDimensions(dimensions.x, dimensions.y, dimensions.z);
    uint indexLoop = frac(_Time.x * Speed) * dimensions.z;

    Out = SAMPLE_TEXTURE2D_ARRAY(Texture, Sampler, UV, indexLoop);
}