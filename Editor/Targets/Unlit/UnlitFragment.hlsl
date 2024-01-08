#pragma fragment frag

half4 frag(VaryingsWrapper input) : SV_Target
{
    SurfaceDescription surfaceDescription = SurfaceDescriptionFunction((Varyings)input);

    half4 col = half4(surfaceDescription.Color.rgb, surfaceDescription.Alpha);

    UNITY_APPLY_FOG(i.fogCoord, col);
    return col;
}