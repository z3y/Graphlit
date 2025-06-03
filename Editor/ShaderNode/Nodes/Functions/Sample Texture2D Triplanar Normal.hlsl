// Planar/Triplanar convention for Unity in world space
void GetTriplanarCoordinate_2(float3 position, out float2 uvXZ, out float2 uvXY, out float2 uvZY)
{
    // Caution: This must follow the same rule as what is use for SurfaceGradient triplanar
    // TODO: Currently the normal mapping looks wrong without SURFACE_GRADIENT option because we don't handle corretly the tangent space
    uvXZ = float2(position.x, position.z);
    uvXY = float2(position.x, position.y);
    uvZY = float2(position.z, position.y);
}

void SampleTexture2DTripanarNormal(Texture2D Texture, SamplerState Sampler, float3 Normal, float3 Position, out half3 NormalWS, half NormalScale = 1, half Blend = 5)
{
    float3 weight = max(pow(abs(Normal), Blend), 0);
    weight /= (weight.x + weight.y + weight.z).xxx;
    weight = saturate(weight);

    float2 uvX, uvY, uvZ;
    GetTriplanarCoordinate_2(Position, uvY, uvZ, uvX);
    uvY += (1.0 / 3.0);
    uvZ += (1.0 / 3.0) * 2.0;

    half3 normalTS_X = half3(0,0,1), normalTS_Y = half3(0,0,1), normalTS_Z = half3(0,0,1);

    UNITY_BRANCH
    if (weight.x > 0)
    {
        normalTS_X =  UnpackNormalScale(SAMPLE_TEXTURE2D(Texture, Sampler, uvX), NormalScale);
    }

    UNITY_BRANCH
    if (weight.y > 0)
    {
        normalTS_Y = UnpackNormalScale(SAMPLE_TEXTURE2D(Texture, Sampler, uvY), NormalScale);
    }

    UNITY_BRANCH
    if (weight.z > 0)
    {
        normalTS_Z = UnpackNormalScale(SAMPLE_TEXTURE2D(Texture, Sampler, uvZ), NormalScale);
    }

    // UDN blend from bgolus https://bgolus.medium.com/normal-mapping-for-a-triplanar-shader-10bf39dca05a#6d4d
    #ifdef QUALITY_LOW // use UDN
        // Swizzle world normals into tangent space and apply UDN blend.
        // These should get normalized, but it's very a minor visual
        // difference to skip it until after the blend.
        normalTS_X = half3(normalTS_X.xy + Normal.zy, Normal.x);
        normalTS_Y = half3(normalTS_Y.xy + Normal.xz, Normal.y);
        normalTS_Z = half3(normalTS_Z.xy + Normal.xy, Normal.z);

    #else // use RNM
        // Get absolute value of normal to ensure positive tangent "z" for blend
        half3 absVertNormal = abs(Normal);
        // Swizzle world normals to match tangent space and apply RNM blend
        normalTS_X = BlendNormalRNM(half3(Normal.zy, absVertNormal.x), normalTS_X);
        normalTS_Y = BlendNormalRNM(half3(Normal.xz, absVertNormal.y), normalTS_Y);
        normalTS_Z = BlendNormalRNM(half3(Normal.xy, absVertNormal.z), normalTS_Z);

        // Get the sign (-1 or 1) of the surface normal
        half3 axisSign = Normal < 0 ? -1 : 1;
        // Reapply sign to Z
        normalTS_X.z *= axisSign.x;
        normalTS_Y.z *= axisSign.y;
        normalTS_Z.z *= axisSign.z;
    #endif

    // Swizzle tangent normals to match world orientation and triblend
    // normalized in the shader
    NormalWS = normalTS_X.zyx * weight.x + normalTS_Y.xzy * weight.y + normalTS_Z.xyz * weight.z;
    NormalWS = SafeNormalize(NormalWS);
}