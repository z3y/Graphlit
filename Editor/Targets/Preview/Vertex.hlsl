#pragma vertex vert

Varyings vert(Attributes input)
{
    Varyings output = (Varyings)0;
    UNITY_INITIALIZE_OUTPUT(Varyings, output);

    VertexDescription vertexDescription = VertexDescriptionFunction(input, output);

    output.positionCS = UnityObjectToClipPos(input.positionOS);
    return output;
}
