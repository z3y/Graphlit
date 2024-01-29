#pragma vertex vert

Varyings vert(Attributes input)
{
    Varyings output = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_OUTPUT(Varyings, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexDescription vertexDescription = VertexDescriptionFunction(input, output);

    output.positionCS = UnityObjectToClipPos(input.positionOS);
    UNITY_TRANSFER_FOG(output, input.positionOS);
    return output;
}
