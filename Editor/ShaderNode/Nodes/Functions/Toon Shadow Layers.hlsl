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

#define defaultShadowBorderColor float3(1,0,0) // temp until parser is better
#define defaultShadowColor2 float4(0.68, 0.66, 0.79, 1)
#define defaultShadowColor1 float4(0.82, 0.76, 0.85, 1)
void ToonShadowsLayers(out float3 Diffuse, float3 LightColor, float3 LightDirection, float ShadowAttenuation, float3 NormalWS, float4 ShadowColor1 = defaultShadowColor1, float ShadowBorder1 = 0.5, float ShadowBlur1 = 0.1, float4 ShadowColor2 = defaultShadowColor2, float ShadowBorder2 = 0.15, float ShadowBlur2 = 0.1, float4 ShadowColor3 = 0, float ShadowBorder3 = 0.1, float ShadowBlur3 = 0.1, float3 ShadowBorderColor = defaultShadowBorderColor, float ShadowBorderRange = 0.08, bool applyShadow = true)
{
    half3 col = LightColor;

    const float antialias = 1.0;

    half4 lns = 1;
    lns.x = saturate(dot(LightDirection, NormalWS) * 0.5 + 0.5);
    lns.y = saturate(dot(LightDirection, NormalWS) * 0.5 + 0.5);
    lns.z = saturate(dot(LightDirection, NormalWS) * 0.5 + 0.5);

    if (applyShadow)
    {
        lns.xyz *= ShadowAttenuation;
    }
    lns.w = lns.x;

    lns.x = lilTooningNoSaturateScale_local(antialias, lns.x, ShadowBorder1, ShadowBlur1);
    lns.y = lilTooningNoSaturateScale_local(antialias, lns.y, ShadowBorder2, ShadowBlur2);
    lns.z = lilTooningNoSaturateScale_local(antialias, lns.z, ShadowBorder3, ShadowBlur3);
    lns.w = lilTooningNoSaturateScale_local(antialias, lns.w, ShadowBorder1, ShadowBlur1, ShadowBorderRange);

    lns = saturate(lns);

    col = lerp(col, lerp(LightColor * ShadowColor1, col, lns.x), ShadowColor1.a);
    col = lerp(col, lerp(LightColor * ShadowColor2, col, lns.y), ShadowColor2.a);
    col = lerp(col, lerp(LightColor * ShadowColor3, col, lns.z), ShadowColor3.a);

    col = lerp(col, LightColor, lns.w * ShadowBorderColor.rgb);

    Diffuse = col;
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