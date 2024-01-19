struct FragmentData
{
    float3 positionWS;
    float3 positionOS;
    float3 normalWS;
    float3 normalOS;
    float3 tangentOS;
    float3 tangentWS;
    float3 bitangentWS;
    float3 bitangentOS;
    float3 viewDirectionWS;

    static FragmentData Create(Varyings varyings)
    {
        FragmentData data = (FragmentData)0;

        #ifdef PREVIEW
            _WorldSpaceCameraPos.xyz = float3(0,0,0);
        #endif

        #ifdef UNPACK_POSITIONWS
            data.positionWS = UNPACK_POSITIONWS;
        #endif

        #ifdef UNPACK_NORMALWS
            data.normalWS = UNPACK_NORMALWS;
        #endif

        #ifdef UNPACK_TANGENTWS
            float4 tangentWS = UNPACK_TANGENTWS;
        #else
            float4 tangentWS = 0;
        #endif

        float crossSign = (tangentWS.w > 0.0 ? 1.0 : -1.0) * unity_WorldTransformParams.w;
        data.bitangentWS = crossSign * cross(data.normalWS.xyz, tangentWS.xyz);

        float3 unnormalizedNormalWS = data.normalWS;
        float renormFactor = 1.0 / length(unnormalizedNormalWS);

        data.positionOS = mul(unity_WorldToObject, float4(data.positionWS, 1.0)).xyz;
        data.normalOS = normalize(mul(data.normalWS, (float3x3)UNITY_MATRIX_M));
        data.tangentOS = normalize(mul(tangentWS.xyz, (float3x3)UNITY_MATRIX_M));
        data.bitangentOS = normalize(mul(data.bitangentWS, (float3x3)UNITY_MATRIX_M));

        data.normalWS *= renormFactor;
        data.tangentWS = tangentWS.xyz * renormFactor;
        data.bitangentWS *= renormFactor;

        data.viewDirectionWS = normalize(_WorldSpaceCameraPos.xyz - data.positionWS);

        return data;
    }
};