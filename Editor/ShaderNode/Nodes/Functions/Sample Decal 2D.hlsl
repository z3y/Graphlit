void SampleDecal2D(out half4 Out, float2 UV, Texture2D Texture, SamplerState Sampler)
{
    float2 dx = ddx(UV);
    float2 dy = ddy(UV);

    UNITY_BRANCH if (!any(abs(UV - 0.5) > 0.5))
    {
        Out = SAMPLE_TEXTURE2D_GRAD(Texture, Sampler, UV, dx, dy);
    }
    else
    {
        Out = 0;
    }
}