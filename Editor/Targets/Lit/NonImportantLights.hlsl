#if defined(VERTEXLIGHT_ON)
void NonImportantLightsPerPixel(FragmentData fragData, GIInput giInput, SurfaceDescription surf, inout GIOutput giOutput)
{
    half clampedRoughness = max(surf.Roughness * surf.Roughness, 0.002);

    // Original code by Xiexe
    // https://github.com/Xiexe/Xiexes-Unity-Shaders

    // MIT License

    // Copyright (c) 2019 Xiexe

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

    float4 toLightX = unity_4LightPosX0 - fragData.positionWS.x;
    float4 toLightY = unity_4LightPosY0 - fragData.positionWS.y;
    float4 toLightZ = unity_4LightPosZ0 - fragData.positionWS.z;

    float4 lengthSq = 0.0;
    lengthSq += toLightX * toLightX;
    lengthSq += toLightY * toLightY;
    lengthSq += toLightZ * toLightZ;

    #if 0
        float4 attenuation = 1.0 / (1.0 + lengthSq * unity_4LightAtten0);
        float4 atten2 = saturate(1 - (lengthSq * unity_4LightAtten0 / 25.0));
        attenuation = min(attenuation, atten2 * atten2);
    #else
        // https://forum.unity.com/threads/point-light-in-v-f-shader.499717/
        float4 range = 5.0 * (1.0 / sqrt(unity_4LightAtten0));
        float4 attenUV = sqrt(lengthSq) / range;
        float4 attenuation = saturate(1.0 / (1.0 + 25.0 * attenUV * attenUV) * saturate((1 - attenUV) * 5.0));
    #endif

    UNITY_LOOP
    for (uint i = 0; i < 4; i++)
    {
        UNITY_BRANCH
        if (attenuation[i] <= 0.0)
        {
            break;
        }

        float3 direction = normalize(float3(unity_4LightPosX0[i], unity_4LightPosY0[i], unity_4LightPosZ0[i]) - fragData.positionWS);
        GraphlitLight vertexLight = (GraphlitLight)0;
        vertexLight.color = unity_LightColor[i];
        vertexLight.attenuation = attenuation[i];
        vertexLight.direction = direction;
        vertexLight.ComputeData(fragData, giInput);

        LIGHT_IMPL(vertexLight, fragData, giInput, surf, giOutput);

    }
}
#endif