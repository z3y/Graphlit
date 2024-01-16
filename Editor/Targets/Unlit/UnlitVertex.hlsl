#pragma vertex vert
#pragma multi_compile_fog

struct AttributesWrapper : Attributes
{
    float3 normalOS : NORMAL;
};

struct VaryingsWrapper : Varyings
{
    //UNITY_FOG_COORDS(1)
    //float4 vertex : SV_POSITION;
};

VaryingsWrapper vert(AttributesWrapper input)
{
    VaryingsWrapper varyings = (VaryingsWrapper)0;

    VertexDescription vertexDescription = VertexDescriptionFunction((Attributes) input, (Varyings)varyings);
    // input.positionOS += vertexDescription.Position;

    varyings.positionCS = UnityObjectToClipPos(input.positionOS);
    //varyings.uv0 = input.uv0;
    UNITY_TRANSFER_FOG(varyings, varyings.vertex);
    return varyings;
}
