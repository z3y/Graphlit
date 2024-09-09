#pragma once

float3 headDirection(float3 positionWS)
{
#if defined(USING_STEREO_MATRICES)
        return (unity_StereoWorldSpaceCameraPos[0].xyz + unity_StereoWorldSpaceCameraPos[1].xyz) * 0.5 - positionWS;
#else
    return _WorldSpaceCameraPos.xyz - positionWS;
#endif
}

// #ifdef UNITY_PASS_SHADOWCASTER
// bool _OutlineToggle; // temporary before theres a better solution
// #endif

void OutlineScale(float3 PositionOS, float3 NormalOS, float3 PositionWS, out float3 ScaledPositionWS, float Width = 0.1, float WidthFix = 0.3)
{
    #if defined(OUTLINE_PASS)// || defined(UNITY_PASS_SHADOWCASTER)
    Width *= lerp(1.0, saturate(length(headDirection(PositionWS))), WidthFix);
    #ifdef UNITY_PASS_SHADOWCASTER
        Width *= 0;
    #endif
    PositionOS += NormalOS * Width * 0.01;
    #endif
    ScaledPositionWS = TransformObjectToWorld(PositionOS);
}