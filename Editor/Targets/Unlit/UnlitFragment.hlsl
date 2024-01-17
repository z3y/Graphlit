#pragma fragment frag

half4 frag(VaryingsWrapper input) : SV_Target
{
    SurfaceDescription surfaceDescription = SurfaceDescriptionFunction((Varyings)input);

    half4 col = surfaceDescription.Color;
    
    #ifdef PREVIEW
    col.r = LinearToGammaSpaceExact(col.r);
    col.g = LinearToGammaSpaceExact(col.g);
    col.b = LinearToGammaSpaceExact(col.b);
    #endif

    UNITY_APPLY_FOG(i.fogCoord, col);
    return col;
}