#pragma vertex vert

Varyings vert(Attributes input)
{
    Varyings varyings = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_OUTPUT(Varyings, varyings);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(varyings);

    VertexDescription vertexDescription = VertexDescriptionFunction(input, varyings);

    float3 positionWS = TransformObjectToWorld(vertexDescription.Position);
    varyings.positionCS = TransformWorldToHClip(positionWS);

    #ifdef UNPACK_NORMALWS
        UNPACK_NORMALWS = TransformObjectToWorldNormal(vertexDescription.Normal);
    #endif
    #ifdef UNPACK_TANGENTWS
        UNPACK_TANGENTWS = float4(TransformObjectToWorldDir(vertexDescription.Tangent), input.tangentOS.w);
    #endif
    
    UNITY_TRANSFER_FOG(varyings, varyings.positionCS);
    return varyings;
}
