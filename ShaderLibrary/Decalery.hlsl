void ApplyDecalaryOffsets(inout float3 positionWS)
{
    #ifdef _DECALERY
        float viewPushConstant = 0.001;
        #if defined(UNITY_STEREO_MULTIVIEW_ENABLED) || defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_SINGLE_PASS_STEREO) 
            float3 eyePos = 0.5 * (unity_StereoWorldSpaceCameraPos[0] + unity_StereoWorldSpaceCameraPos[1]);
        #else
            float3 eyePos = _WorldSpaceCameraPos;
        #endif
        positionWS += normalize(eyePos - positionWS) * viewPushConstant;
    #endif
}