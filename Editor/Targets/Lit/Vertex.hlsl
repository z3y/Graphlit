#pragma vertex vert

Varyings vert(Attributes input)
{
    Varyings varyings = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_OUTPUT(Varyings, varyings);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(varyings);

    VertexDescription vertexDescription = VertexDescriptionFunction(input, varyings);
    float3 positionWS = TransformObjectToWorld(vertexDescription.Position);
    
    float3 normalWS = TransformObjectToWorldNormal(vertexDescription.Normal);

    #ifdef UNPACK_POSITIONWS
        UNPACK_POSITIONWS = positionWS;
    #endif
    #ifdef UNPACK_NORMALWS
        UNPACK_NORMALWS = normalWS;
    #endif
    #ifdef UNPACK_TANGENTWS
        UNPACK_TANGENTWS = float4(TransformObjectToWorldDir(vertexDescription.Tangent), input.tangentOS.w);
    #endif

    #if defined(UNITY_PASS_SHADOWCASTER)
        varyings.positionCS = TransformWorldToHClip(ApplyShadowBiasNormal(positionWS, normalWS));
        varyings.positionCS = UnityApplyLinearShadowBias(varyings.positionCS);
    #elif defined(UNITY_PASS_META)
        varyings.positionCS = UnityMetaVertexPosition(float4(TransformWorldToObject(positionWS), 0), input.uv1, input.uv2, unity_LightmapST, unity_DynamicLightmapST);
    #else
        varyings.positionCS = TransformWorldToHClip(positionWS);
    #endif

    #if defined(LIGHTMAP_ON)
        varyings.lightmapUV.xy = mad(input.uv1.xy, unity_LightmapST.xy, unity_LightmapST.zw);
    #endif

    #if !UNITY_SAMPLE_FULL_SH_PER_PIXEL && defined(UNITY_PASS_FORWARDBASE)
        varyings.sh = ShadeSHPerVertex(normalWS, 0);
    #endif

    UNITY_TRANSFER_SHADOW(varyings, attributes.uv1.xy);
    UNITY_TRANSFER_FOG(varyings, varyings.positionCS);
    return varyings;
}
