
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
    float2 lightmapUV;

    static VertexData Create(inout Attributes attributes)
    {
        VertexData output = (VertexData)0;

        #ifdef _TERRAIN
        TerrainInstancing(attributes.positionOS, attributes.normalOS, attributes.uv0.xy);
        #endif

        #ifdef ATTRIBUTES_NEED_POSITIONOS
            output.positionOS = attributes.positionOS;
        #endif
        #ifdef ATTRIBUTES_NEED_NORMALOS
            output.normalOS = attributes.normalOS;
        #endif
        #ifdef ATTRIBUTES_NEED_TANGENTOS
            #ifdef _TERRAIN
                float4 vertex_tangentOS = ComputeTerrainTangent(attributes.normalOS);
                attributes.tangentOS.w = 1;
                attributes.tangentOS.xyz = vertex_tangentOS.xyz;
            #else
                float4 vertex_tangentOS = attributes.tangentOS;
            #endif
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
            #ifdef UNIVERSALRP
                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirectionWS = normalize(_LightPosition - output.positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif
                output.positionCSR = TransformWorldToHClip(ApplyShadowBias(output.positionWS, output.normalWS, lightDirectionWS));
                // output.positionCSR = ApplyShadowClamping(output.positionCSR);
            #else
                output.positionCSR = TransformWorldToHClip(ApplyShadowBias(output.positionWS, output.normalWS, _WorldSpaceLightPos0.rgb));  
                output.positionCSR = ApplyShadowClamping(output.positionCSR);
            #endif
        #elif defined(UNITY_PASS_META)
            output.positionCSR = UnityMetaVertexPosition(output.positionOS, attributes.uv1.xy, attributes.uv2.xy);
        #else
            output.positionCSR = TransformWorldToHClip(output.positionWS);
        #endif

        #if defined(LIGHTMAP_ON) || defined(SHADOWS_SHADOWMASK)
            output.lightmapUV = mad(attributes.uv1.xy, unity_LightmapST.xy, unity_LightmapST.zw);
        #endif

        return output;
    }
};
