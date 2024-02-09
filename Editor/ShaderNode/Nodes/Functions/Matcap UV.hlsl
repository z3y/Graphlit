float2 __RotateUV(float2 _uv, float _radian, float2 _piv, float _time)
{
    float RotateUV_ang = _radian;
    float RotateUV_cos = cos(_time*RotateUV_ang);
    float RotateUV_sin = sin(_time*RotateUV_ang);
    return (mul(_uv - _piv, float2x2( RotateUV_cos, -RotateUV_sin, RotateUV_sin, RotateUV_cos)) + _piv);
}

void Matcap_UV(float3 NormalWS, float3 ViewDirectionWS, out float2 MatcapUV)
{
    float3 normalVS = mul((float3x3)UNITY_MATRIX_V, NormalWS);

    //  https://github.com/unity3d-jp/UnityChanToonShaderVer2_Project
    float3 NormalBlend_MatcapUV_Detail = normalVS * float3(-1, -1, 1);
    float3 NormalBlend_MatcapUV_Base = (mul(UNITY_MATRIX_V, float4(ViewDirectionWS, 0)).rgb*float3(-1, -1, 1)) + float3(0, 0, 1);

    float3 noSknewViewNormal = NormalBlend_MatcapUV_Base *
        dot(NormalBlend_MatcapUV_Base, NormalBlend_MatcapUV_Detail) / NormalBlend_MatcapUV_Base.b - NormalBlend_MatcapUV_Detail;

    #if !defined(PREVIEW) && !defined(SHADER_API_MOBILE)
        normalVS = noSknewViewNormal;
    #endif

    MatcapUV = mad(normalVS.xy, 0.5, 0.5);
}