
float lilTooningNoSaturateScale_Specular(float aascale, float value, float border, float blur)
{
	float borderMin = saturate(border - blur * 0.5);
	float borderMax = saturate(border + blur * 0.5);
	return (value - borderMin) / saturate(borderMax - borderMin + fwidth(value) * aascale);
}

void StylizedSpecularNode(out float3 specular, half3 lightColor, float3 lightDirection, float3 normalWS, float3 viewDirectionWS, half roughness = 0.5, half blur = 0.1, half border = 0.1, bool antiAlias = true)
{
	half NoL = saturate(dot(normalWS, lightDirection));
	float3 halfVector = SafeNormalize(lightDirection + viewDirectionWS);
	half LoH = saturate(dot(lightDirection, halfVector));
	half NoH = saturate(dot(normalWS, halfVector));

	roughness = max((roughness * roughness), 0.002);

	specular = saturate(lilTooningNoSaturateScale_Specular(antiAlias, pow(NoH, 1.0 / roughness), border, blur)) * lightColor * PI;
}

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