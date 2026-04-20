#pragma once

float GetFogTypeVarying()
{
    float fogType = 0;
    bool isExp2 = false;
    #ifdef FOG_EXP2
        isExp2 = true;
    #endif
    if (unity_FogParams.z != unity_FogParams.w) {
        fogType = 1;
    } else if (isExp2) {
        fogType = 3;
    } else if (unity_FogParams.y != 0.0) {
        fogType = 2;
    }

    return fogType;
}

float GraphlitComputeFogFactorZ0ToFar(float z, float fogType)
{
    [flatten]
    if (fogType < 1.5)
    {
        return saturate(z * unity_FogParams.z + unity_FogParams.w);
    }
    else
    {
        return unity_FogParams.x * z;
    }
}

void GraphlitApplyFog(float3 positionWS, float fogType, inout float3 color)
{
    [branch]
    if (fogType != 0)
    {
        float3 fogColor = unity_FogColor.rgb;
        #ifdef UNITY_PASS_FORWARDADD
            fogColor = 0;
        #endif

        float fogFactor = 0;
        float viewZ = -(mul(UNITY_MATRIX_V, float4(positionWS, 1)).z);
        float nearToFarZ = max(viewZ - _ProjectionParams.y, 0);
        fogFactor = GraphlitComputeFogFactorZ0ToFar(nearToFarZ, fogType);

        float fogIntensity = fogFactor;

        [flatten]
        if (fogType > 1.5)
        {
            fogIntensity = saturate(exp2(-fogFactor));
        }
        [flatten]
        if (fogType > 2.5)
        {
            fogIntensity = saturate(exp2(-fogFactor * fogFactor));
        }


        color = color * fogIntensity + fogColor * (half(1.0) - fogIntensity);
    }

}