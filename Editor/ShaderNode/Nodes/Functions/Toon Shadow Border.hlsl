void ToonShadowBorder(out float3 Color_0, float3 lightColor_4, float3 lightDirection_5, float3 normal_3, float3 shadowColor_6, float border_8 = 0.5, float blur_9 = 0.1, float3 blendLayer_1 = 1, float borderRange_10 = 0.5)
{
	float value = saturate(dot(lightDirection_5, normal_3) * 0.5 + 0.5);

    float borderMin = saturate(border_8 - blur_9 * 0.5 - borderRange_10);
    float borderMax = saturate(border_8 + blur_9 * 0.5);
    float lns = saturate((value - borderMin) / saturate(borderMax - borderMin));


    Color_0 = lerp(blendLayer_1, lightColor_4, lns * shadowColor_6);
}