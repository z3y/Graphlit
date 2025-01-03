namespace LilGlitter
{
    float lilNsqDistance(float2 a, float2 b)
    {
        return dot(a-b,a-b);
    }

    // Simplest Fastest 2D Hash
    // https://www.shadertoy.com/view/MdcfDj
    void lilHashRGB4(float2 pos, out float3 noise0, out float3 noise1, out float3 noise2, out float3 noise3)
    {
        // Hash
        // https://www.shadertoy.com/view/MdcfDj
        #define M1 1597334677U
        #define M2 3812015801U
        #define M3 2912667907U
        uint2 q = (uint2)pos;
        uint4 q2 = uint4(q.x, q.y, q.x+1, q.y+1) * uint4(M1, M2, M1, M2);
        uint3 n0 = (q2.x ^ q2.y) * uint3(M1, M2, M3);
        uint3 n1 = (q2.z ^ q2.y) * uint3(M1, M2, M3);
        uint3 n2 = (q2.x ^ q2.w) * uint3(M1, M2, M3);
        uint3 n3 = (q2.z ^ q2.w) * uint3(M1, M2, M3);
        noise0 = float3(n0) * (1.0/float(0xffffffffU));
        noise1 = float3(n1) * (1.0/float(0xffffffffU));
        noise2 = float3(n2) * (1.0/float(0xffffffffU));
        noise3 = float3(n3) * (1.0/float(0xffffffffU));
        #undef M1
        #undef M2
        #undef M3
    }

    float4 lilVoronoi(float2 pos, out float2 nearoffset, float scaleRandomize)
    {
        #if defined(SHADER_API_D3D9) || defined(SHADER_API_D3D11_9X)
        #define M1 46203.4357
        #define M2 21091.5327
        #define M3 35771.1966
        float2 q = trunc(pos);
        float4 q2 = float4(q.x, q.y, q.x+1, q.y+1);
        float3 noise0 = frac(sin(dot(q2.xy,float2(12.9898,78.233))) * float3(M1, M2, M3));
        float3 noise1 = frac(sin(dot(q2.zy,float2(12.9898,78.233))) * float3(M1, M2, M3));
        float3 noise2 = frac(sin(dot(q2.xw,float2(12.9898,78.233))) * float3(M1, M2, M3));
        float3 noise3 = frac(sin(dot(q2.zw,float2(12.9898,78.233))) * float3(M1, M2, M3));
        #undef M1
        #undef M2
        #undef M3
        #else
        float3 noise0, noise1, noise2, noise3;
        lilHashRGB4(pos, noise0, noise1, noise2, noise3);
        #endif

        // Get the nearest position
        float4 fracpos = frac(pos).xyxy + float4(0.5,0.5,-0.5,-0.5);
        float4 dist4 = float4(lilNsqDistance(fracpos.xy,noise0.xy), lilNsqDistance(fracpos.zy,noise1.xy), lilNsqDistance(fracpos.xw,noise2.xy), lilNsqDistance(fracpos.zw,noise3.xy));
        dist4 = lerp(dist4, dist4 / max(float4(noise0.z, noise1.z, noise2.z, noise3.z), 0.001), scaleRandomize);

        float3 nearoffset0 = dist4.x < dist4.y ? float3(0,0,dist4.x) : float3(1,0,dist4.y);
        float3 nearoffset1 = dist4.z < dist4.w ? float3(0,1,dist4.z) : float3(1,1,dist4.w);
        nearoffset = nearoffset0.z < nearoffset1.z ? nearoffset0.xy : nearoffset1.xy;

        float4 near0 = dist4.x < dist4.y ? float4(noise0,dist4.x) : float4(noise1,dist4.y);
        float4 near1 = dist4.z < dist4.w ? float4(noise2,dist4.z) : float4(noise3,dist4.w);
        return near0.w < near1.w ? near0 : near1;
    }

    float3 lilCalcGlitter(float2 uv, float3 cameraDirection, float4 glitterParams1, float4 glitterParams2, float glitterSensitivity, float glitterScaleRandomize, bool glitterAngleRandomize, bool glitterApplyShape, Texture2D glitterShapeTex, float4 glitterShapeTex_ST, float4 glitterAtras, float3 viewDirection, float3 lightDirection, float3 normalWS)
    {
        // glitterParams1
        // x: Scale, y: Scale, z: Size, w: Contrast
        // glitterParams2
        // x: Speed, y: Angle, z: Light Direction, w:

        #define GLITTER_DEBUG_MODE 0
        #define GLITTER_MIPMAP 1
        #define GLITTER_ANTIALIAS 1

        #if GLITTER_MIPMAP == 1
            float2 pos = uv * glitterParams1.xy;
            float2 dd = fwidth(pos);
            float factor = frac(sin(dot(floor(pos/floor(dd + 3.0)),float2(12.9898,78.233))) * 46203.4357) + 0.5;
            float2 factor2 = floor(dd + factor * 0.5);
            pos = pos/max(1.0,factor2) + glitterParams1.xy * factor2;
        #else
            float2 pos = uv * glitterParams1.xy + glitterParams1.xy;
        #endif
        float2 nearoffset;
        float4 near = lilVoronoi(pos, nearoffset, glitterScaleRandomize);
        

        // Glitter
        float3 glitterNormal = abs(frac(near.xyz*14.274 + _Time.x * glitterParams2.x) * 2.0 - 1.0);
        glitterNormal = normalize(glitterNormal * 2.0 - 1.0);
        float glitter = dot(glitterNormal, cameraDirection);
        glitter = abs(frac(glitter * glitterSensitivity + glitterSensitivity) - 0.5) * 4.0 - 1.0;
        glitter = saturate(1.0 - (glitter * glitterParams1.w + glitterParams1.w));
        // glitter = pow(glitter, glitterPostContrast);
        // Circle
        #if GLITTER_ANTIALIAS == 1
            glitter *= saturate((glitterParams1.z-near.w) / fwidth(near.w));
        #else
            glitter = near.w < glitterParams1.z ? glitter : 0.0;
        #endif
        // Angle
        // not needed in graph
        // float3 halfDirection = normalize(viewDirection + lightDirection * glitterParams2.z);
        // float nh = saturate(dot(normalWS, halfDirection));
        // glitter = saturate(glitter * saturate(nh * glitterParams2.y + 1.0 - glitterParams2.y));
        // Random Color
        float3 glitterColor = glitter - glitter * frac(near.xyz*278.436) * glitterParams2.w;
        
        // Shape
        if (glitterApplyShape)
        {
            float2 maskUV = pos - floor(pos) - nearoffset + 0.5 - near.xy;
            maskUV = maskUV / glitterParams1.z;
            if (glitterAngleRandomize)
            {
                float si,co;
                sincos(near.z * 785.238, si, co);
                maskUV = float2(
                    maskUV.x * co - maskUV.y * si,
                    maskUV.x * si + maskUV.y * co
                );
            }
            float randomScale = lerp(1.0, 1.0 / sqrt(max(near.z, 0.001)), glitterScaleRandomize);
            maskUV = maskUV * randomScale + 0.5;
            bool clamp = maskUV.x == saturate(maskUV.x) && maskUV.y == saturate(maskUV.y);
            maskUV = (maskUV + floor(near.xy * glitterAtras.xy)) / glitterAtras.xy;
            float2 mipfactor = 0.125 / glitterParams1.z * glitterAtras.xy * glitterShapeTex_ST.xy * randomScale;
            float4 shapeTex = SAMPLE_TEXTURE2D_GRAD(glitterShapeTex, custom_bilinear_clamp_sampler, maskUV, abs(ddx(pos)) * mipfactor.x, abs(ddy(pos)) * mipfactor.y);
            shapeTex.a = clamp ? shapeTex.a : 0;
            glitterColor *= shapeTex.rgb * shapeTex.a;
        }
        return glitterColor;
    }

    float3 lilCameraDirection()
    {
        #if defined(USING_STEREO_MATRICES)
            return normalize(UNITY_STEREO_MATRIX_V(0)._m20_m21_m22 + UNITY_STEREO_MATRIX_V(1)._m20_m21_m22);
        #else
            return UNITY_MATRIX_V._m20_m21_m22;
        #endif
    }
}

void GlitterNode(out float3 Glitter, Texture2D shapeTexture, bool applyShape, bool shapeRandomAngle, float2 UV, float2 scale = 256, float size = 0.16, float contrast = 50, float speed = 0, float sensitivity = 0.25, float randomScale = 0.5, float randomColor = 0)
{
    float3 camDir = LilGlitter::lilCameraDirection();

    // some parts are removed to simplify inputs since they can be remade in the graph
    Glitter = LilGlitter::lilCalcGlitter(UV, camDir, float4(scale, size, contrast),
        float4(speed, 0, 0, randomColor), sensitivity, randomScale, shapeRandomAngle, applyShape, shapeTexture,
        float4(0,0,0,0), float4(1,1,0,0), 0, 0, 0);
}


// ported from liltoon
/*
MIT License

Copyright (c) 2020-2024 lilxyzw

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