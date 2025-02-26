
//SOURCE - https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightEvaluation.hlsl  
//From Moving Frostbite to PBR document
//This function fakes the roughness based integration of reflection probes by adjusting the roughness value
//NOTE: Untouched from HDRP
float ComputeDistanceBaseRoughness(float distanceIntersectionToShadedPoint, float distanceIntersectionToProbeCenter, float perceptualRoughness)
{
    float newPerceptualRoughness = clamp(distanceIntersectionToShadedPoint / distanceIntersectionToProbeCenter * perceptualRoughness, 0, perceptualRoughness);
    return lerp(newPerceptualRoughness, perceptualRoughness, perceptualRoughness);
}

//SOURCE - https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.core/ShaderLibrary/GeometricTools.hlsl#L78
//This simplified version assume that we care about the result only when we are inside the box
//NOTE: Untouched from HDRP
float IntersectRayAABBSimple(float3 start, float3 dir, float3 boxMin, float3 boxMax)
{
    float3 invDir = rcp(dir);

    // Find the ray intersection with box plane
    float3 rbmin = (boxMin - start) * invDir;
    float3 rbmax = (boxMax - start) * invDir;

    float3 rbminmax = float3((dir.x > 0.0) ? rbmax.x : rbmin.x, (dir.y > 0.0) ? rbmax.y : rbmin.y, (dir.z > 0.0) ? rbmax.z : rbmin.z);

    return min(min(rbminmax.x, rbminmax.y), rbminmax.z);
}

//SOURCE - https://github.com/Unity-Technologies/Graphics/blob/504e639c4e07492f74716f36acf7aad0294af16e/Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightEvaluation.hlsl  
//return projectionDistance, can be used in ComputeDistanceBaseRoughness formula
//return in R the unormalized corrected direction which is used to fetch cubemap but also its length represent the distance of the capture point to the intersection
//Length R can be reuse as a parameter of ComputeDistanceBaseRoughness for distIntersectionToProbeCenter
//NOTE: Modified to be much simpler, and to work with the Built-In Render Pipeline (BIRP)
float EvaluateLight_EnvIntersection(float3 worldSpacePosition, inout float3 R, float4 probePosition, float4 boxMin, float4 boxMax)
{
    float projectionDistance = IntersectRayAABBSimple(worldSpacePosition, R, boxMin.xyz, boxMax.xyz);

    R = (worldSpacePosition + projectionDistance * R) - probePosition.xyz;

    return projectionDistance;
}

Unity_GlossyEnvironmentData GetEnvData(float3 reflDir, float3 positionWS, float4 probePosition, float4 boxMin, float4 boxMax, half perceptualRoughness)
{
    Unity_GlossyEnvironmentData envData;
    envData.roughness = perceptualRoughness;

    #ifndef QUALITY_LOW

        UNITY_FLATTEN
        if (probePosition.w <= 0.0f)
        {
            envData.reflUVW = reflDir;
            return envData;
        }

        float projectionDistance = EvaluateLight_EnvIntersection(positionWS, reflDir, probePosition, boxMin, boxMax);
        float distanceBasedRoughness = ComputeDistanceBaseRoughness(projectionDistance, length(reflDir), perceptualRoughness);
        envData.reflUVW = reflDir;
        envData.roughness = distanceBasedRoughness;
    #else
        envData.reflUVW = BoxProjectedCubemapDirection(reflDir, positionWS, probePosition, boxMin, boxMax);
    #endif

    return envData;
}

// thanks https://github.com/frostbone25/Unity-Improved-Box-Projected-Reflections/blob/main/ImprovedBoxProjectedReflections/Assets/Shaders/HDRPBoxProjection.cginc

/*
MIT License

Copyright (c) 2023 David Matos

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/