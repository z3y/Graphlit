#pragma fragment frag

#include "GlobalIllumination/ClusteredBIRP.hlsl"
#include "GlobalIllumination/LTCGI.hlsl"

float4 frag(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    FragmentData fragment = FragmentData::Create(input);

    SurfaceDescription surface = SurfaceDescriptionFunction(input);


    half alpha = surface.Alpha;
    #if defined(_ALPHATEST_ON)
        half clippedAlpha = AlphaClip(surface.Alpha, surface.Cutoff);
    #endif

    half3 diffuseColor = surface.Albedo * (1.0 - surface.Metallic);
    #if defined(_ALPHAMODULATE_ON)
        diffuseColor = AlphaModulate(diffuseColor, alpha);
    #endif
    #if defined(_ALPHAPREMULTIPLY_ON)
        diffuseColor *= alpha;
    #endif

    #if defined(_NORMAL_DROPOFF_OFF)
        float3 normalWS = fragment.normalWS;
    #elif defined(_NORMAL_DROPOFF_WS)
        float3 normalWS = surface.Normal;
    #elif defined(_NORMAL_DROPOFF_OS)
        float3 normalWS = TransformObjectToWorldNormal(surface.Normal);
    #else // _NORMAL_DROPOFF_TS
        float3 normalWS = SafeNormalize(mul(surface.Normal, fragment.tangentSpaceTransform));
    #endif

    ShadingData shading;
    shading.NoV = abs(dot(normalWS, fragment.viewDirectionWS)) + 1e-5f;
    shading.normalWS = normalWS;
    shading.reflectVector = reflect(-fragment.viewDirectionWS, normalWS);
    shading.perceptualRoughness = surface.Roughness;
    shading.roughness = max(shading.perceptualRoughness * shading.perceptualRoughness, HALF_MIN_SQRT);

    shading.viewDirectionWS = fragment.viewDirectionWS;
    half dielectricSpecularF0 = 0.16 * surface.Reflectance * surface.Reflectance;
    shading.f0 = surface.Albedo * surface.Metallic + dielectricSpecularF0 * (1.0 - surface.Metallic);

    float3 positionWS = fragment.positionWS;

    half3 bakedGI = 0;
    half3 lightmapSpecular = 0;
    half3 indirectOcclusion = 0;
    #if defined(LIGHTMAP_ON)
        SampleLightmap(bakedGI, lightmapSpecular, fragment.lightmapUV, normalWS, fragment.viewDirectionWS, shading.perceptualRoughness, indirectOcclusion, shading.reflectVector);
    #elif defined(LIGHTPROBE_SH)
        bakedGI = SampleSH(normalWS, positionWS);
    #endif

    float4 shadowCoord = TransformWorldToShadowCoord(positionWS);
    Light light = GetMainLight(positionWS, shadowCoord, fragment.lightmapUV);

    #if defined(_MASKMAP) || defined(_OCCLUSION)
    // only for directional light
    light.shadowAttenuation *= ComputeMicroShadowing(surface.Occlusion, dot(light.direction, normalWS), 1.0);
    #endif

    #if defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) && defined(LIGHTMAP_ON)
    bakedGI = SubtractMainLightFromLightmap(bakedGI, normalWS, light.color, light.direction, light.shadowAttenuation);
    light.color = 0;
    #endif

    half3 indirectSpecular = 0;
#ifndef _GLOSSYREFLECTIONS_OFF
    indirectSpecular = CalculateIrradianceFromReflectionProbes(shading.reflectVector,
        positionWS, shading.perceptualRoughness, 0, fragment.normalWS);
#endif

    half3 diffuse = 0;
    half3 specular = 0;
    ShadeLight(diffuse, specular, light, shading);


    #ifdef VERTEXLIGHT_ON
    for (uint i = 0; i < GetAdditionalLightCount(); i++)
    {
        Light additionalLight = GetAdditionalLight(positionWS, i);
        ShadeLight(diffuse, specular, additionalLight, shading);
    }
    #endif

    #ifdef UNIVERSALRP
        uint pixelLightCount = GetAdditionalLightsCount();
        uint meshRenderingLayers = GetMeshRenderingLayer();
    LIGHT_LOOP_BEGIN(pixelLightCount)
        URPLight additionalURPLight = GetAdditionalLight(lightIndex, positionWS, SampleShadowMask(fragment.lightmapUV));
#ifdef _LIGHT_LAYERS
        if (IsMatchingLightLayer(additionalURPLight.layerMask, meshRenderingLayers))
#endif
        {
            Light additionalLight;
            CopyUniversalLight(additionalLight, additionalURPLight);
            ShadeLight(diffuse, specular, additionalLight, shading);
        }
    LIGHT_LOOP_END
    #endif

    #ifdef _LTCGI
        GetLTCGIDiffuseAndSpecular(diffuse, indirectSpecular, shading, fragment, surface);
    #endif

    #ifdef _CBIRP
        half4 shadowmask = SampleShadowMask(fragment.lightmapUV);
        uint3 cluster = CBIRP::GetCluster(fragment.positionWS);

        ComputeCBIRPLights(diffuse, specular, cluster, shadowmask, fragment, shading, surface);

        #ifdef _CBIRP_REFLECTIONS
            indirectSpecular = CBIRP::SampleProbes(cluster, shading.reflectVector, fragment.positionWS, surface.Roughness).xyz;
        #endif
    #endif

    half3 brdf;
    half3 energyCompensation;
    EnvironmentBRDF(shading.NoV, shading.perceptualRoughness, shading.f0, brdf, energyCompensation);
    indirectSpecular *= brdf * energyCompensation;
    lightmapSpecular *= brdf * energyCompensation;
    bakedGI *= 1.0 - brdf;
    specular *= energyCompensation * PI;

    #ifdef LIGHTMAP_ON
        half indirectOcclusionIntensity = _SpecularOcclusion;
        half occlusionFromLightmap = saturate(lerp(1.0, saturate(sqrt(dot(indirectOcclusion + diffuse, 1.0))), indirectOcclusionIntensity));
        indirectSpecular *= occlusionFromLightmap;
    #endif
    
    #if defined(_MASKMAP) || defined(_OCCLUSION) // doesnt get optimized out even if occlusion is 1
        half singleBounceAO = GetSpecularOcclusionFromAmbientOcclusion(shading.NoV, surface.Occlusion,
            surface.Roughness * surface.Roughness);
        indirectSpecular *= GTAOMultiBounce(singleBounceAO, shading.f0);
        bakedGI *= GTAOMultiBounce(surface.Occlusion, diffuseColor);
    #endif


    float4 color = float4(diffuseColor * (diffuse + bakedGI) + specular + lightmapSpecular + indirectSpecular, alpha);

    color.rgb += surface.Emission;

    color.rgb = MixFog(color.rgb, InitializeInputDataFog(float4(positionWS, 1), 0));

    #if defined(_SURFACE_TYPE_TRANSPARENT)
        bool isTransparent = true;
    #else
        bool isTransparent = false;
    #endif
    
    color.a = OutputAlpha(color.a, isTransparent);

    return color;
}