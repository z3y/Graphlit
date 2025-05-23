#pragma once

#ifdef _AREALIT
#include "Assets/AreaLit/Shader/Lighting.hlsl"

TEXTURE2D(_AreaLitOcclusion);

void IntegrateAreaLit(inout half3 diffuse, inout half3 reflection, FragmentData fragment, ShadingData shading)
{
    AreaLightFragInput areaLitInput = (AreaLightFragInput)0;
    half4 areaLitDiffuse;
    half4 areaLitSpecular;
    #ifdef _SPECULARHIGHLIGHTS_OFF
        bool areaLitSpecularEnabled = false;
    #else
        bool areaLitSpecularEnabled = true;
    #endif
    areaLitInput.pos = float4(fragment.positionWS.xyz, 1);
    areaLitInput.normal = shading.normalWS;
    areaLitInput.view = fragment.viewDirectionWS;
    areaLitInput.roughness = shading.perceptualRoughness * shading.perceptualRoughness;
    #ifdef LIGHTMAP_ON
    areaLitInput.occlusion = SAMPLE_TEXTURE2D_LOD(_AreaLitOcclusion, sampler_BilinearClamp, fragment.lightmapUV.xy, 0);
    #else
    areaLitInput.occlusion = 1;
    #endif
    areaLitInput.screenPos = fragment.positionNDC;
    ShadeAreaLights(areaLitInput, areaLitDiffuse, areaLitSpecular, true, areaLitSpecularEnabled);
    reflection += areaLitSpecular;
    diffuse += areaLitDiffuse;
}
#endif