
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
    float3 viewDirectionOS;
    float3 viewDirectionTS;
    float3x3 tangentSpaceTransform;
    bool frontFace;
    float4 positionCSR;
    float2 grabScreenPosition;
    float2 screenPosition;
    float2 lightmapUV;

    static FragmentData Create(Varyings varyings)
    {
        FragmentData output = (FragmentData)1;

        #ifdef UNPACK_POSITIONWS
            output.positionWS = UNPACK_POSITIONWS;
        #endif

        #ifdef UNPACK_NORMALWS
            output.normalWS = UNPACK_NORMALWS;
        #endif

        #ifdef UNPACK_TANGENTWS
            float4 tangentWS = UNPACK_TANGENTWS;
        #else
            float4 tangentWS = 0;
        #endif

        float crossSign = (tangentWS.w > 0.0 ? 1.0 : -1.0) * unity_WorldTransformParams.w;
        output.bitangentWS = crossSign * cross(output.normalWS.xyz, tangentWS.xyz);

        float3 unnormalizedNormalWS = output.normalWS;
        float renormFactor = 1.0 / length(unnormalizedNormalWS);

        output.positionOS = mul(unity_WorldToObject, float4(output.positionWS, 1.0)).xyz;
        output.normalOS = normalize(mul(output.normalWS, (float3x3)UNITY_MATRIX_M));
        output.tangentOS = TransformWorldToObjectDir(tangentWS.xyz);
        output.bitangentOS = TransformWorldToObjectDir(output.bitangentWS);

        output.normalWS *= renormFactor;
        output.tangentWS = tangentWS.xyz * renormFactor;
        output.bitangentWS *= renormFactor;

        output.tangentSpaceTransform = float3x3(output.tangentWS, output.bitangentWS, output.normalWS);

        output.viewDirectionWS = normalize(_WorldSpaceCameraPos.xyz - output.positionWS);
        output.viewDirectionOS = TransformWorldToObjectDir(output.viewDirectionWS);
        output.viewDirectionTS = mul(output.tangentSpaceTransform, output.viewDirectionWS);

        #if defined(VARYINGS_NEED_FACE) && defined(SHADER_STAGE_FRAGMENT)
        output.frontFace = IS_FRONT_VFACE(varyings.cullFace, true, false);
        #endif

        #ifdef UNPACK_POSITIONCSR
            output.positionCSR = UNPACK_POSITIONCSR;
            float4 grabPos = ComputeGrabScreenPos(output.positionCSR);
            float4 screenPos = ComputeScreenPos(output.positionCSR);
            output.grabScreenPosition = grabPos.xy / grabPos.w;
            output.screenPosition = screenPos.xy / screenPos.w;
        #endif

        #ifdef LIGHTMAP_ON
            output.lightmapUV = varyings.lightmapUV;
        #endif

        return output;
    }
};