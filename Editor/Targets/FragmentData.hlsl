
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
    float4 lightmapUV;
    float4 shadowCoords;
    float3 positionNDC;

    float4 uv0;
    float4 uv1;
    float4 uv2;
    float4 uv3;

    static FragmentData Create(Varyings varyings)
    {
        FragmentData output = (FragmentData)0;

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
        #ifdef UNPACK_NORMALWS
        float renormFactor = 1.0 / length(unnormalizedNormalWS);
        #else
        float renormFactor = 1.0;
        #endif

        // output.positionOS = mul(unity_WorldToObject, float4(output.positionWS, 1.0)).xyz;
        output.positionOS = TransformWorldToObject(output.positionWS);
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

        // todo: find functions for urp
        #ifdef UNPACK_POSITIONCSR
            output.positionCSR = UNPACK_POSITIONCSR;
            float4 grabPos = ComputeGrabScreenPos(output.positionCSR);
            float4 screenPos = ComputeScreenPos(output.positionCSR);
            output.grabScreenPosition = grabPos.xy / grabPos.w;
            output.screenPosition = screenPos.xy / screenPos.w;
        #endif

        output.positionNDC = ComputeNormalizedDeviceCoordinatesWithZ(output.positionWS, GetWorldToHClipMatrix());

        #ifdef LIGHTMAP_ON
            output.lightmapUV.xy = varyings.lightmapUV.xy;
        #endif
        #ifdef DYNAMICLIGHTMAP_ON
            output.lightmapUV.zw = varyings.lightmapUV.zw;
        #endif

        output.shadowCoords = TransformWorldToShadowCoord(output.positionWS);

        #ifdef UNPACK_UV0
            output.uv0 = UNPACK_UV0;
        #endif
        #ifdef UNPACK_UV1
            output.uv1 = UNPACK_UV1;
        #endif
        #ifdef UNPACK_UV2
            output.uv2 = UNPACK_UV2;
        #endif
        #ifdef UNPACK_UV3
            output.uv3 = UNPACK_UV3;
        #endif

        return output;
    }
};