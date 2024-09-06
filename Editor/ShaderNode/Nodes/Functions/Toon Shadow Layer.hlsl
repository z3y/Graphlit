void ToonShadowLayer(out float3 Color_0, float3 lightColor_4, float3 lightDirection_5, float3 normal_3, float4 shadowColor_6 = 1, float border_8 = 0.5, float blur_9 = 0.1, float3 blendLayer_1 = 1, bool antiAlias_10 = true)
{
	half value = saturate(dot(lightDirection_5, normal_3) * 0.5 + 0.5);

    float borderMin = saturate(border_8 - blur_9 * 0.5);
    float borderMax = saturate(border_8 + blur_9 * 0.5);
    float lns = saturate((value - borderMin) / saturate(borderMax - borderMin + fwidth(value) * antiAlias_10));


    Color_0 = lerp(blendLayer_1, lerp(lightColor_4 * shadowColor_6.rgb, blendLayer_1, lns), shadowColor_6.a);
}