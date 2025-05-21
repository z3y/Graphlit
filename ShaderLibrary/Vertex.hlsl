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

    ApplyDecalaryOffsets(positionWS);

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
            // varyings.positionCS = ApplyShadowClamping(varyings.positionCS);
        #else
            varyings.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _WorldSpaceLightPos0.rgb));   
            varyings.positionCS = ApplyShadowClamping(varyings.positionCS);
        #endif
    #elif defined(UNITY_PASS_META)
        varyings.positionCS = UnityMetaVertexPosition(input.positionOS, input.uv1.xy, input.uv2.xy);
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
    #ifdef DYNAMICLIGHTMAP_ON
        varyings.lightmapUV.zw = mad(input.uv2.xy, unity_DynamicLightmapST.xy, unity_DynamicLightmapST.zw);
    #endif

    #ifdef EDITOR_VISUALIZATION
        UnityEditorVizData(input.positionOS, input.uv0.xy, input.uv1.xy, input.uv2.xy, varyings.VizUV, varyings.LightCoord);
    #endif

    #ifdef UNPACK_POSITIONCSR
        UNPACK_POSITIONCSR = varyings.positionCS;
    #endif

    return varyings;
}
