#pragma vertex vert

Varyings vert(Attributes input)
{
    Varyings varyings = (Varyings)0;
    ZERO_INITIALIZE(Varyings, varyings);

#if !defined(UNITY_PASS_META)
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, varyings);
#endif
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(varyings);

    VertexDescription vertexDescription = VertexDescriptionFunction(input, varyings);
    #if !defined(UNITY_PASS_META) && !defined(SKIP_VERTEX_FUNCTION)
    float3 positionWS = vertexDescription.Position;
    float3 normalWS = vertexDescription.Normal;
    #endif

#if !defined(UNITY_PASS_META) 
    #ifdef UNPACK_POSITIONWS
        UNPACK_POSITIONWS = positionWS;
    #endif
    #ifdef UNPACK_NORMALWS
        UNPACK_NORMALWS = normalWS;
    #endif
    #ifdef UNPACK_TANGENTWS
        UNPACK_TANGENTWS = float4(vertexDescription.Tangent, input.tangentOS.w);
    #endif
#endif

    #if defined(UNITY_PASS_SHADOWCASTER)
    
        #ifdef UNIVERSALRP
            #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                float3 lightDirectionWS = normalize(_LightPosition - positionWS);
            #else
                float3 lightDirectionWS = _LightDirection;
            #endif
            varyings.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
        #else
            varyings.positionCS = TransformWorldToHClip(ApplyShadowBiasNormal(positionWS, normalWS));   
            varyings.positionCS = UnityApplyLinearShadowBias(varyings.positionCS);
        #endif
    #elif defined(UNITY_PASS_META)
        varyings.positionCS = UnityMetaVertexPosition(float4(input.positionOS, 1.0), input.uv1.xy, input.uv2.xy, unity_LightmapST, unity_DynamicLightmapST);
    #else
        #ifdef SKIP_VERTEX_FUNCTION
            varyings.positionCS = TransformObjectToHClip(input.positionOS);
        #else
            varyings.positionCS = TransformWorldToHClip(positionWS);
        #endif
    #endif

    #if defined(LIGHTMAP_ON)
        varyings.lightmapUV.xy = mad(input.uv1.xy, unity_LightmapST.xy, unity_LightmapST.zw);
    #endif

    #ifdef UNIVERSALRP
        // todo: find proper functions for urp
        #ifdef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
            varyings.shadowCoord = TransformWorldToShadowCoord(positionWS);
        #endif
    #else
        UNITY_TRANSFER_SHADOW(varyings, input.uv1.xy);
        UNITY_TRANSFER_FOG(varyings, varyings.positionCS);
    #endif

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
