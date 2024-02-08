void OutlineScale(float3 PositionOS, float3 NormalOS, out float3 PositionWS, float Width = 0.1)
{
    #if defined(OUTLINE_PASS) || defined(UNITY_PASS_SHADOWCASTER)
    PositionOS += NormalOS * Width;
    #endif
    PositionWS = TransformObjectToWorld(PositionOS);
}