void Proximity(float3 PositionWS, out float Out, float MinDistance = 0, float MaxDistance = 1)
{
    float3 cameraPos = GetCameraPositionWSCenter();

    Out = smoothstep(MinDistance, MaxDistance, distance(cameraPos, PositionWS));
}