// scale applied before reconstruction
real3 UnpackNormalAG_Override(real4 packedNormal, real scale = 1.0)
{
    real3 normal;
    normal.xy = packedNormal.ag * 2.0 - 1.0;
    normal.xy *= scale;
    normal.z = max(1.0e-16, sqrt(1.0 - saturate(dot(normal.xy, normal.xy))));
    return normal;
}

// Unpack normal as DXT5nm (1, y, 0, x) or BC5 (x, y, 0, 1)
real3 UnpackNormalmapRGorAG_Override(real4 packedNormal, real scale = 1.0)
{
    // Convert to (?, y, 0, x)
    packedNormal.a *= packedNormal.r;
    return UnpackNormalAG_Override(packedNormal, scale);
}

real3 UnpackNormalScale_Override(real4 packedNormal, real bumpScale)
{
#if defined(UNITY_ASTC_NORMALMAP_ENCODING)
    return UnpackNormalAG(packedNormal, bumpScale);
#elif defined(UNITY_NO_DXT5nm)
    return UnpackNormalRGB(packedNormal, bumpScale);
#else
    return UnpackNormalmapRGorAG_Override(packedNormal, bumpScale);
#endif
}

void UnpackNormalScale(out float3 Out, float4 Normal, float Scale = 1)
{
    Out = UnpackNormalScale_Override(Normal, Scale);
}