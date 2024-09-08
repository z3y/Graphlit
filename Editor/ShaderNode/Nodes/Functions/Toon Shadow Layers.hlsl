#pragma once

float lilTooningNoSaturateScale_local(float aascale, float value, float border, float blur)
{
    float borderMin = saturate(border - blur * 0.5);
    float borderMax = saturate(border + blur * 0.5);
    return (value - borderMin) / saturate(borderMax - borderMin + fwidth(value) * aascale);
}
float lilTooningNoSaturateScale_local(float aascale, float value, float border, float blur, float borderRange)
{
    float borderMin = saturate(border - blur * 0.5 - borderRange);
    float borderMax = saturate(border + blur * 0.5);
    return (value - borderMin) / saturate(borderMax - borderMin);
}


void ToonShadowsLayers(out float3 Out, float3 LightColor, float3 LightDirection, float DistanceAttenuation, float ShadowAttenuation, float4 ShadowColor1, float4 ShadowColor2, float4 ShadowColor3, float3 ShadowsBorder, float3 ShadowsReceive, float3 ShadowsBlur, float3 ShadowBorderColor, float ShadowBorderRange, float3 NormalWS)
{
    half3 col = LightColor * DistanceAttenuation;
    half3 defaultCol = col;


    const float antialias = 1.0;

    half4 lns = 1;
    lns.x = saturate(dot(LightDirection, NormalWS) * 0.5 + 0.5);
    lns.y = saturate(dot(LightDirection, NormalWS) * 0.5 + 0.5);
    lns.z = saturate(dot(LightDirection, NormalWS) * 0.5 + 0.5);

    lns.xyz *= lerp(DistanceAttenuation, ShadowAttenuation, ShadowsReceive);
    lns.w = lns.x;

    lns.x = lilTooningNoSaturateScale_local(antialias, lns.x, ShadowsBorder.x, ShadowsBlur.x);
    lns.y = lilTooningNoSaturateScale_local(antialias, lns.y, ShadowsBorder.y, ShadowsBlur.y);
    lns.z = lilTooningNoSaturateScale_local(antialias, lns.z, ShadowsBorder.z, ShadowsBlur.z);
    lns.w = lilTooningNoSaturateScale_local(antialias, lns.w, ShadowsBorder.x, ShadowsBlur.x, ShadowBorderRange);

    lns = saturate(lns);

    col = lerp(col, lerp(defaultCol * ShadowColor1, col, lns.x), 1.0);
    col = lerp(col, lerp(defaultCol * ShadowColor2, col, lns.y), ShadowColor2.a);
    col = lerp(col, lerp(defaultCol * ShadowColor3, col, lns.z), ShadowColor3.a);

    col = lerp(col, defaultCol, lns.w * ShadowBorderColor.rgb);

    Out = col;
}

// MIT License

// Copyright (c) 2020-2024 lilxyzw

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