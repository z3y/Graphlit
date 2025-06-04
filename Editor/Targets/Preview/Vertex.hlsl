// #pragma vertex vert

Varyings vert(Attributes input)
{
    Varyings output = (Varyings)0;
    ZERO_INITIALIZE(Varyings, output);

    VertexDescription vertexDescription = VertexDescriptionFunction(input, output);

    float3 positionWS = TransformObjectToWorld(input.positionOS);
    output.positionCS = TransformWorldToHClip(positionWS);
    return output;
}
