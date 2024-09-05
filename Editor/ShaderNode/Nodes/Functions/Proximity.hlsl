void Proximity(float3 PositionWS, out float Out, float MinDistance = 0, float MaxDistance = 1)
{
    #if defined(USING_STEREO_MATRICES)
        float3 cameraPos = (unity_StereoWorldSpaceCameraPos[0].xyz + unity_StereoWorldSpaceCameraPos[1].xyz) * 0.5;
    #else
        float3 cameraPos = _WorldSpaceCameraPos.xyz;
    #endif
    Out = smoothstep(MinDistance, MaxDistance, distance(cameraPos, PositionWS));
}