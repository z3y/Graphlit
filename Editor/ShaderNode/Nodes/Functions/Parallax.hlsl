#pragma once

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ParallaxMapping.hlsl"

float3 CalculateTangentViewDir(float3 tangentViewDir)
{
    tangentViewDir = SafeNormalize(tangentViewDir);
    tangentViewDir.xy /= (tangentViewDir.z + 0.42);
	return tangentViewDir;
}

float2 GetMinUvSize(float2 baseUV, float4 texelSize)
{
    float2 minUvSize = float2(FLT_MAX, FLT_MAX);

    minUvSize = min(baseUV * texelSize.zw, minUvSize);

    return minUvSize;
}

void ParallaxOcclusionMapping(Texture2D HeightMap, SamplerState Sampler, float strength, half offset, float steps, float2 UV, float3 viewDirectionTS, float4 texelSize, out float2 uvOffset)
{
    #ifdef QUALITY_LOW
    uvOffset = 0;
    return;
    #endif

    viewDirectionTS = CalculateTangentViewDir(viewDirectionTS);
    float surfaceHeight = SAMPLE_TEXTURE2D(HeightMap, Sampler, UV).r;
    uvOffset = 0;
	float stepSize = 1.0 / (uint)steps;
    float3 uvDelta_stepSize = float3(viewDirectionTS.xy * (stepSize * strength), stepSize);
    float3 uvOffset_stepHeight = float3(float2(0, 0), 1.0);
    
    float2 minUvSize = GetMinUvSize(UV, texelSize);
    float lod = ComputeTextureLOD(minUvSize);

    float previousStepHeight = 0;
    float previousSurfaceHeight = 0;
    float2 previousUVOffset = 0;

    [loop]
    for (uint j = 0; j < (uint)steps; j++)
    {
        if (uvOffset_stepHeight.z < surfaceHeight)
        {
            break;
        }

        previousStepHeight = uvOffset_stepHeight.z;
        previousSurfaceHeight = surfaceHeight;
        previousUVOffset = uvOffset_stepHeight.xy;


        uvOffset_stepHeight -= uvDelta_stepSize;
        surfaceHeight = SAMPLE_TEXTURE2D_LOD(HeightMap, Sampler, (UV + uvOffset_stepHeight.xy), lod).r + offset;
    }

    // taken from filamented cause it looks better https://gitlab.com/s-ilent/filamented
    float previousDifference = previousStepHeight - previousSurfaceHeight;
    float delta = surfaceHeight - uvOffset_stepHeight.z;
    uvOffset_stepHeight.xy = previousUVOffset - uvDelta_stepSize.xy * previousDifference / (previousDifference + delta);


    uvOffset = uvOffset_stepHeight.xy;
}

// Original code from Mochie
// https://github.com/MochiesCode/Mochies-Unity-Shaders/blob/7d48f101d04dac11bd4702586ee838ca669f426b/Mochie/Standard%20Shader/MochieStandardParallax.cginc#L13
// MIT License

// Copyright (c) 2020 MochiesCode

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
