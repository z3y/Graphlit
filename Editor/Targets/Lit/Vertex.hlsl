#pragma vertex vert

Varyings vert(Attributes input)
{
    Varyings varyings = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_OUTPUT(Varyings, varyings);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(varyings);

    VertexDescription vertexDescription = VertexDescriptionFunction(input, varyings);
    #ifndef UNITY_PASS_META
    float3 positionWS = vertexDescription.Position;
    float3 normalWS = vertexDescription.Normal;
    #endif

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
        varyings.positionCS = UnityMetaVertexPosition(float4(input.positionOS, 1.0), input.uv1.xy, input.uv2.xy, unity_LightmapST, unity_DynamicLightmapST);
    #else
        varyings.positionCS = TransformWorldToHClip(positionWS);
    #endif

    #if defined(LIGHTMAP_ON)
        varyings.lightmapUV.xy = mad(input.uv1.xy, unity_LightmapST.xy, unity_LightmapST.zw);
    #endif

    #if !UNITY_SAMPLE_FULL_SH_PER_PIXEL && defined(UNITY_PASS_FORWARDBASE)
        varyings.sh = ShadeSHPerVertex(normalWS, 0);
    #endif

    UNITY_TRANSFER_SHADOW(varyings, input.uv1.xy);
    UNITY_TRANSFER_FOG(varyings, varyings.positionCS);

    #ifdef EDITOR_VISUALIZATION
        varyings.vizUV = 0;
        varyings.lightCoord = 0;
        if (unity_VisualizationMode == EDITORVIZ_TEXTURE)
            varyings.vizUV = UnityMetaVizUV(unity_EditorViz_UVIndex, input.uv0.xy, input.uv1.xy, input.uv2.xy, unity_EditorViz_Texture_ST);
        else if (unity_VisualizationMode == EDITORVIZ_SHOWLIGHTMASK)
        {
            varyings.vizUV = input.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
            varyings.lightCoord = mul(unity_EditorViz_WorldToLight, mul(unity_ObjectToWorld, float4(input.positionOS, 1)));
        }
    #endif

    #ifdef UNPACK_POSITIONCSR
        UNPACK_POSITIONCSR = varyings.positionCS;
    #endif

    return varyings;
}
