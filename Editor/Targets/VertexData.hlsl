
struct VertexData
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

    static VertexData Create(Attributes attributes)
    {
        VertexData output = (VertexData)1;

        #ifdef ATTRIBUTES_NEED_POSITIONOS
            output.positionOS = attributes.positionOS;
        #endif
        #ifdef ATTRIBUTES_NEED_NORMALOS
            output.normalOS = attributes.normalOS;
        #endif
        #ifdef ATTRIBUTES_NEED_TANGENTOS
            float4 vertex_tangentOS = attributes.tangentOS;
        #else
            float4 vertex_tangentOS = 0;
        #endif

        output.tangentOS = vertex_tangentOS.xyz;

        output.positionWS = TransformObjectToWorld(output.positionOS);
        output.tangentWS = TransformObjectToWorldDir(vertex_tangentOS.xyz);
        output.normalWS = TransformObjectToWorldNormal(output.normalOS);

        float crossSign = (vertex_tangentOS.w > 0.0 ? 1.0 : -1.0) * unity_WorldTransformParams.w;
        output.bitangentWS = normalize(crossSign * cross(output.normalWS.xyz, output.tangentWS.xyz));
        output.bitangentOS = TransformWorldToObjectDir(output.bitangentWS);

        output.tangentSpaceTransform = float3x3(output.tangentWS, output.bitangentWS, output.normalWS);

        output.viewDirectionWS = normalize(_WorldSpaceCameraPos.xyz - output.positionWS);
        output.viewDirectionOS = TransformWorldToObjectDir(output.viewDirectionWS);
        output.viewDirectionTS = mul(output.tangentSpaceTransform, output.viewDirectionWS);

        output.frontFace = true;

        #if defined(UNITY_PASS_SHADOWCASTER)
            output.positionCSR = TransformWorldToHClip(ApplyShadowBiasNormal(output.positionWS, output.normalWS));
            output.positionCSR = UnityApplyLinearShadowBias(output.positionCSR);
        #elif defined(UNITY_PASS_META)
            output.positionCSR = UnityMetaVertexPosition(float4(TransformWorldToObject(output.positionWS), 0), input.uv1, input.uv2, unity_LightmapST, unity_DynamicLightmapST);
        #else
            output.positionCSR = TransformWorldToHClip(output.positionWS);
        #endif

        return output;
    }
};