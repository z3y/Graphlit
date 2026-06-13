#pragma once

float3 OutlineHeadDirection(float3 positionWS)
{
    return GetCameraPositionWSCenter() - positionWS;
}

void OutlineScale(float3 PositionOS, float3 NormalOS, float3 PositionWS, out float3 ScaledPositionWS, float Width = 0.1, float WidthFix = 0.3, bool applyToShadow = true)
{
    #ifdef OUTLINE_PASS_ENABLED
        #if defined(OUTLINE_PASS) || defined(UNITY_PASS_SHADOWCASTER)

            #ifdef UNITY_PASS_SHADOWCASTER
                if (applyToShadow)
            #endif
            {
                Width *= lerp(1.0, saturate(length(OutlineHeadDirection(PositionWS))), WidthFix);
                PositionOS += NormalOS * Width * 0.01;
            }
        #endif
    #endif

    ScaledPositionWS = TransformObjectToWorld(PositionOS);
}