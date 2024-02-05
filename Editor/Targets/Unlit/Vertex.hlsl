#pragma vertex vert

Varyings vert(Attributes input)
{
    Varyings output = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_OUTPUT(Varyings, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexDescription vertexDescription = VertexDescriptionFunction(input, output);

    float3 positionWS = TransformObjectToWorld(vertexDescription.Position);
    output.positionCS = TransformWorldToHClip(positionWS);
    
    UNITY_TRANSFER_FOG(output, output.positionCS);
    return output;
}
