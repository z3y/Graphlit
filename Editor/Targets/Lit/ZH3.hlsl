// https://gist.github.com/pema99/f735ca33d1299abe0e143ee94fc61e73

// Paper: ZH3: Quadratic Zonal Harmonics, i3D 2024. https://torust.me/ZH3.pdf
// Code based on paper and demo https://www.shadertoy.com/view/Xfj3RK

// L0 radiance = L0 irradiance * PI / Y_0 / AHat_0
// PI / (sqrt(1 / PI) / 2) / PI = 2 * sqrt(PI)
const static float L0IrradianceToRadiance = 2 * sqrt(UNITY_PI);

// L1 radiance = L1 irradiance * PI / Y_1 / AHat_1
// PI / (sqrt(3 / PI) / 2) / ((2 * PI) / 3) = sqrt(3 * PI)
const static float L1IrradianceToRadiance = sqrt(3 * UNITY_PI);

const static float4 L0L1IrradianceToRadiance = float4(L0IrradianceToRadiance, L1IrradianceToRadiance, L1IrradianceToRadiance, L1IrradianceToRadiance);

float SHEvalLinearL0L1_ZH3Hallucinate(float4 sh, float3 normal)
{
    float4 radiance = sh * L0L1IrradianceToRadiance;

    float3 zonalAxis = float3(radiance.w, radiance.y, radiance.z);
    float l1Length = length(zonalAxis);
    zonalAxis /= l1Length;

    float ratio = l1Length / radiance.x;
    float zonalL2Coeff = radiance.x * ratio * (0.08 + 0.6 * ratio); // Curve-fit.

    float fZ = dot(zonalAxis, normal);
    float zhNormal = sqrt(5.0f / (16.0f * UNITY_PI)) * (3.0f * fZ * fZ - 1.0f);

    float result = dot(sh, float4(1, float3(normal.y, normal.z, normal.x)));
    result += 0.25f * zhNormal * zonalL2Coeff;
    return result;
}

// Evaluate irradiance in direction normal from the linear SH sh,
// hallucinating the ZH3 coefficient and then using that and linear SH
// for reconstruction.
float3 SHEvalLinearL0L1_ZH3Hallucinate(float3 normal)
{
    float3 shL0 = float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w) +
        float3(unity_SHBr.z, unity_SHBg.z, unity_SHBb.z) / 3.0;
    float3 shL1_1 = float3(unity_SHAr.y, unity_SHAg.y, unity_SHAb.y);
    float3 shL1_2 = float3(unity_SHAr.z, unity_SHAg.z, unity_SHAb.z);
    float3 shL1_3 = float3(unity_SHAr.x, unity_SHAg.x, unity_SHAb.x);

    float3 result = 0.0;
    float4 a = float4(shL0.r, shL1_1.r, shL1_2.r, shL1_3.r);
    float4 b = float4(shL0.g, shL1_1.g, shL1_2.g, shL1_3.g);
    float4 c = float4(shL0.b, shL1_1.b, shL1_2.b, shL1_3.b);
    result.r = SHEvalLinearL0L1_ZH3Hallucinate(a, normal);
    result.g = SHEvalLinearL0L1_ZH3Hallucinate(b, normal);
    result.b = SHEvalLinearL0L1_ZH3Hallucinate(c, normal);
    return result;
}

// Evaluate irradiance in direction normal from the linear SH sh,
// computing a shared luminance axis from the linear components,
// hallucinating the ZH3 coefficients along that axis,
// and then using ZH3 and linear SH for reconstruction in the direction normal.
float3 SHEvalLinearL0L1_ZH3Hallucinate_LumAxis(float3 direction)
{
    // Get linear SH coefficients from Unity shader uniforms (without L2 folded into L0)
    float3 shL0 = float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w) +
        float3(unity_SHBr.z, unity_SHBg.z, unity_SHBb.z) / 3.0;
    float3 shL1_1 = float3(unity_SHAr.y, unity_SHAg.y, unity_SHAb.y);
    float3 shL1_2 = float3(unity_SHAr.z, unity_SHAg.z, unity_SHAb.z);
    float3 shL1_3 = float3(unity_SHAr.x, unity_SHAg.x, unity_SHAb.x);
    float3 sh[4] = { shL0, shL1_1, shL1_2, shL1_3 };

    // Deconvolve irradiance -> radiance
    float3 radianceSH[4];
    for (int i = 0; i < 3; i++)
    {
        radianceSH[0][i] = sh[0][i] * L0IrradianceToRadiance;
        radianceSH[1][i] = sh[1][i] * L1IrradianceToRadiance;
        radianceSH[2][i] = sh[2][i] * L1IrradianceToRadiance;
        radianceSH[3][i] = sh[3][i] * L1IrradianceToRadiance;
    }

    // Use the zonal axis from the luminance SH.
    const float3 lumCoeffs = float3(0.2126f, 0.7152f, 0.0722f); // sRGB luminance.
    float3 zonalAxis = normalize(float3(dot(radianceSH[3], lumCoeffs), dot(radianceSH[1], lumCoeffs), dot(radianceSH[2], lumCoeffs)));
    float3 ratio = 0.0;
    ratio.r = abs(dot(float3(radianceSH[3].r, radianceSH[1].r, radianceSH[2].r), zonalAxis));
    ratio.g = abs(dot(float3(radianceSH[3].g, radianceSH[1].g, radianceSH[2].g), zonalAxis));
    ratio.b = abs(dot(float3(radianceSH[3].b, radianceSH[1].b, radianceSH[2].b), zonalAxis));
    ratio /= radianceSH[0];
    float3 zonalL2Coeff = radianceSH[0] * (0.08f * ratio + 0.6f * ratio * ratio); // Curve-fit; Section 3.4.3
    float fZ = dot(zonalAxis, direction);
    float zhDir = sqrt(5.0f / (16.0f * UNITY_PI)) * (3.0f * fZ * fZ - 1.0f);

    // Evaluate irradiance from linear SH in the given direction.
    float4 shDir = float4(1, direction.y, direction.z, direction.x);
    float3 result = float3(0.0, 0.0, 0.0);
    result += sh[0] * shDir[0];
    result += sh[1] * shDir[1];
    result += sh[2] * shDir[2];
    result += sh[3] * shDir[3];

    // Add irradiance from the ZH3 term. zonalL2Coeff is the ZH3 coefficient for a radiance signal, so we need to
    // multiply by 1/4 (the L2 zonal scale for a normalized clamped cosine kernel) to evaluate irradiance.
    result += 0.25f * zonalL2Coeff * zhDir;

    return result;
}

float3 ShadeSH9_ZH3Hallucinate(float4 normal)
{
    float3 res = SHEvalLinearL0L1_ZH3Hallucinate(normal.xyz);
    res += SHEvalLinearL2(normal);
    return res;
}

float3 ShadeSH9_ZH3Hallucinate_LumAxis(float4 normal)
{
    float3 res = SHEvalLinearL0L1_ZH3Hallucinate_LumAxis(normal.xyz);
    res += SHEvalLinearL2(normal);
    return res;
}