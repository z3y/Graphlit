#pragma vertex vert
#pragma multi_compile_fog

struct AttributesWrapper : Attributes
{
    float3 positionOS : POSITION;
    float3 normalOS : NORMAL;
    //float2 uv : TEXCOORD0;
};

struct VaryingsWrapper : Varyings
{
    //float2 uv : TEXCOORD0;
    UNITY_FOG_COORDS(1)
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
