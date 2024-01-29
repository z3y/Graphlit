#pragma vertex vert

Varyings vert(Attributes input)
{
    Varyings output = (Varyings)0;
    UNITY_INITIALIZE_OUTPUT(Varyings, output);

    VertexDescription vertexDescription = VertexDescriptionFunction(input, output);

    float3 positionWS = TransformObjectToWorld(input.positionOS);
    output.positionCS = TransformWorldToHClip(positionWS);
    return output;
}
