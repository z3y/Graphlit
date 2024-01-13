#pragma fragment frag

half4 frag(VaryingsWrapper input) : SV_Target
{
    SurfaceDescription surfaceDescription = SurfaceDescriptionFunction((Varyings)input);

    half4 col = 0; //surfaceDescription.Color;

    UNITY_APPLY_FOG(i.fogCoord, col);
    return col;
}