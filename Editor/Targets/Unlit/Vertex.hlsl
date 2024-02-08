#pragma vertex vert

Varyings vert(Attributes input)
{
    Varyings varyings = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_OUTPUT(Varyings, varyings);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(varyings);

    VertexDescription vertexDescription = VertexDescriptionFunction(input, varyings);
    float3 positionWS = vertexDescription.Position;
    float3 normalWS = vertexDescription.Normal;

    #ifdef UNPACK_POSITIONWS
        UNPACK_POSITIONWS = positionWS;
    #endif
    #ifdef UNPACK_NORMALWS
        UNPACK_NORMALWS = normalWS;
    #endif
    #ifdef UNPACK_TANGENTWS
        UNPACK_TANGENTWS = float4(vertexDescription.Tangent, input.tangentOS.w);
    #endif

    #if defined(UNITY_PASS_SHADOWCASTER)
        varyings.positionCS = TransformWorldToHClip(ApplyShadowBiasNormal(positionWS, normalWS));
        varyings.positionCS = UnityApplyLinearShadowBias(varyings.positionCS);
    #elif defined(UNITY_PASS_META)
        varyings.positionCS = UnityMetaVertexPosition(float4(TransformWorldToObject(positionWS), 0), input.uv1, input.uv2, unity_LightmapST, unity_DynamicLightmapST);
    #else
        varyings.positionCS = TransformWorldToHClip(positionWS);
    #endif


    
    UNITY_TRANSFER_FOG(varyings, varyings.positionCS);
    return varyings;
}
