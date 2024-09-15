#pragma once

float3 OutlineHeadDirection(float3 positionWS)
{
#if defined(USING_STEREO_MATRICES)
    return (unity_StereoWorldSpaceCameraPos[0].xyz + unity_StereoWorldSpaceCameraPos[1].xyz) * 0.5 - positionWS;
#else
    return _WorldSpaceCameraPos.xyz - positionWS;
#endif
}

void OutlineScale(float3 PositionOS, float3 NormalOS, float3 PositionWS, out float3 ScaledPositionWS, float Width = 0.1, float WidthFix = 0.3, bool applyToShadow = true)
{
    #if defined(OUTLINE_PASS) || defined(UNITY_PASS_SHADOWCASTER)

        #ifdef UNITY_PASS_SHADOWCASTER
            if (applyToShadow)
        #endif
        {
            Width *= lerp(1.0, saturate(length(OutlineHeadDirection(PositionWS))), WidthFix);
            PositionOS += NormalOS * Width * 0.01;
        }
    #endif

    ScaledPositionWS = TransformObjectToWorld(PositionOS);
}