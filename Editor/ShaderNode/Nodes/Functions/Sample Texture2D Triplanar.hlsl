// Planar/Triplanar convention for Unity in world space
void GetTriplanarCoordinate_1(float3 position, out float2 uvXZ, out float2 uvXY, out float2 uvZY)
{
    // Caution: This must follow the same rule as what is use for SurfaceGradient triplanar
    // TODO: Currently the normal mapping looks wrong without SURFACE_GRADIENT option because we don't handle corretly the tangent space
    uvXZ = float2(position.x, position.z);
    uvXY = float2(position.x, position.y);
    uvZY = float2(position.z, position.y);
}

void SampleTexture2DTripanar(Texture2D Texture, SamplerState Sampler, float3 Normal, float3 Position, out half4 Out, half Blend = 5)
{
    float3 weight = max(pow(abs(Normal), Blend), 0);
    weight /= (weight.x + weight.y + weight.z).xxx;
    weight = saturate(weight);

    float2 uvX, uvY, uvZ;
    GetTriplanarCoordinate_1(Position, uvY, uvZ, uvX);
    uvY += (1.0 / 3.0);
    uvZ += (1.0 / 3.0) * 2.0;

    half4 result_X = 0, result_Y = 0, result_Z = 0;

    UNITY_BRANCH
    if (weight.x > 0)
    {
        result_X = SAMPLE_TEXTURE2D(Texture, Sampler, uvX);
    }

    UNITY_BRANCH
    if (weight.y > 0)
    {
        result_Y = SAMPLE_TEXTURE2D(Texture, Sampler, uvY);
    }

    UNITY_BRANCH
    if (weight.z > 0)
    {
        result_Z = SAMPLE_TEXTURE2D(Texture, Sampler, uvZ);
    }

    Out = result_X * weight.x + result_Y * weight.y + result_Z * weight.z;
}