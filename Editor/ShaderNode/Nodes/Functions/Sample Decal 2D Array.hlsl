void SampleDecal2DArray(out half4 Out, float2 UV, float Index, Texture2DArray Texture, SamplerState Sampler)
{
    float2 dx = ddx(UV);
    float2 dy = ddy(UV);

    UNITY_BRANCH if (!any(abs(UV - 0.5) > 0.5))
    {
        Out = SAMPLE_TEXTURE2D_GRAD(Texture, Sampler, float3(UV, Index), dx, dy);
    }
    else
    {
        Out = 0;
    }
}