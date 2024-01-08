#pragma vertex vert
#pragma multi_compile_fog

struct AttributesWrapper : Attributes
{
    float3 positionOS : POSITION;
    //float2 uv : TEXCOORD0;
};

struct VaryingsWrapper : Varyings
{
    float2 uv : TEXCOORD0;
    UNITY_FOG_COORDS(1)
    float4 vertex : SV_POSITION;
};

VaryingsWrapper vert(AttributesWrapper input)
{
    VaryingsWrapper output;

    VertexDescription vertexDescription = VertexDescriptionFunction((Attributes)input);
    input.positionOS += vertexDescription.Position;

    output.vertex = UnityObjectToClipPos(input.positionOS);
    //o.uv = v.uv;
    UNITY_TRANSFER_FOG(output, output.vertex);
    return output;
}
