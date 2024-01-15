#pragma vertex vert
#pragma multi_compile_fog

struct AttributesWrapper : Attributes
{
    float3 normalOS : NORMAL;
};

struct VaryingsWrapper : Varyings
{
    //UNITY_FOG_COORDS(1)
    float4 vertex : SV_POSITION;
};

VaryingsWrapper vert(AttributesWrapper input)
{
    VaryingsWrapper varyings;

    VertexDescription vertexDescription = VertexDescriptionFunction((Attributes)input);
    // input.positionOS += vertexDescription.Position;

    varyings.vertex = UnityObjectToClipPos(input.positionOS);
    varyings.uv0 = input.uv0;
    UNITY_TRANSFER_FOG(varyings, varyings.vertex);
    return varyings;
}
