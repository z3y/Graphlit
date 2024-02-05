#pragma fragment frag

half4 frag(Varyings varyings) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(varyings);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(varyings);
    
    SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(varyings);

    half4 col;
    col.rgb = surfaceDescription.Color;
    col.a = surfaceDescription.Alpha;

    UNITY_APPLY_FOG(varyings.fogCoord, col);

    return col;
}