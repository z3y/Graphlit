#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_fog

struct appdata : Attributes
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
};

struct v2f : Varyings
{
    float2 uv : TEXCOORD0;
    UNITY_FOG_COORDS(1)
    float4 vertex : SV_POSITION;
};

v2f vert(appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = v.uv;
    UNITY_TRANSFER_FOG(o, o.vertex);
    return o;
}

half4 frag(v2f i) : SV_Target
{
    SurfaceDescription surface = SurfaceDescriptionFunction((Varyings)i);

    half4 col = surface.Albedo.rgbb;

    UNITY_APPLY_FOG(i.fogCoord, col);
    return col;
}