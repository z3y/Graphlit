#pragma fragment frag

#include "GlobalIllumination/ClusteredBIRP.hlsl"
#include "GlobalIllumination/LTCGI.hlsl"
#include "GlobalIllumination/AreaLit.hlsl"

#ifdef _ACES
    #include "ACES.hlsl"
#endif


float4 frag(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    FragmentData fragment = FragmentData::Create(input);

    SurfaceDescription surface = SurfaceDescriptionFunction(input);


    half alpha = surface.Alpha;
    #if defined(_ALPHATEST_ON)
        alpha = AlphaClip(surface.Alpha, surface.Cutoff);
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
        #define _NORMAL_DROPOFF_TS
    #endif

    ShadingData shading;
    shading.NoV = abs(dot(normalWS, fragment.viewDirectionWS)) + 1e-5f;
    shading.normalWS = normalWS;
    shading.reflectVector = reflect(-fragment.viewDirectionWS, normalWS);
    shading.perceptualRoughness = surface.Roughness;
    #ifdef QUALITY_LOW
    shading.roughness = max(shading.perceptualRoughness * shading.perceptualRoughness, HALF_MIN_SQRT);
    #else
    shading.roughness = max(shading.perceptualRoughness * shading.perceptualRoughness, 0.002);
    #endif

    shading.viewDirectionWS = fragment.viewDirectionWS;
    half dielectricSpecularF0 = 0.16 * surface.Reflectance * surface.Reflectance;
    shading.f0 = surface.Albedo * surface.Metallic + dielectricSpecularF0 * (1.0 - surface.Metallic);

    float3 positionWS = fragment.positionWS;

    half3 bakedGI = 0;
    half3 lightmapSpecular = 0;
    half3 indirectOcclusion = 0;
    #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
        SampleLightmap(bakedGI, lightmapSpecular, fragment.lightmapUV, normalWS, fragment.viewDirectionWS, shading.perceptualRoughness, indirectOcclusion, shading.reflectVector);
        #ifdef _VRC_LIGHTVOLUMES
            half3 lvL0; half3 lvL1r; half3 lvL1g; half3 lvL1b;
            LightVolumeAdditiveSH(positionWS, lvL0, lvL1r, lvL1g, lvL1b);
            bakedGI += LightVolumeEvaluate(normalWS, lvL0, lvL1r, lvL1g, lvL1b);
        #endif
        bakedGI = max(0, bakedGI);
    #elif defined(LIGHTPROBE_SH) || defined(UNIVERSALRP)
        bakedGI = SampleSH(normalWS, positionWS);
        indirectOcclusion = bakedGI;
    #endif

    float4 shadowCoord = TransformWorldToShadowCoord(positionWS);
    Light light = GetMainLight(positionWS, shadowCoord, fragment.lightmapUV.xy);

    #if defined(_MASKMAP) || defined(_OCCLUSION)
    // only for directional light
    #ifndef UNITY_PASS_FORWARDADD
    light.shadowAttenuation *= ComputeMicroShadowing(surface.Occlusion, dot(light.direction, normalWS), 1.0);
    #endif
    #endif

    #if defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) && defined(LIGHTMAP_ON)
    bakedGI = SubtractMainLightFromLightmap(bakedGI, normalWS, light.color, light.direction, light.shadowAttenuation);
    light.color = 0;
    #endif

    half3 diffuse = 0;
    half3 specular = 0;

    #if defined(LIGHTMAP_SPECULAR) && !defined(LIGHTMAP_ON) && !defined(_VRC_LIGHTVOLUMES)
    if (!light.enabled)
    {
        light.direction = normalize((unity_SHAr.xyz + unity_SHAg.xyz + unity_SHAb.xyz) * 1.0/3.0);
        light.color = half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
        light.specularOnly = true;
        light.enabled = true;
    }
    #elif defined(LIGHTMAP_SPECULAR) && !defined(LIGHTMAP_ON) && defined(_VRC_LIGHTVOLUMES)
        half3 lvL0; half3 lvL1r; half3 lvL1g; half3 lvL1b;
        LightVolumeSH(positionWS, lvL0, lvL1r, lvL1g, lvL1b);
        specular += LightVolumeSpecularDominant(surface.Albedo, 1.0 - surface.Roughness, surface.Metallic, normalWS, fragment.viewDirectionWS, lvL0, lvL1r, lvL1g, lvL1b) * INV_PI;
    #endif

    half3 indirectSpecular = 0;
#ifndef UNITY_PASS_FORWARDADD
#if !defined(_GLOSSYREFLECTIONS_OFF) && !defined(_CBIRP_REFLECTIONS)
    indirectSpecular = CalculateIrradianceFromReflectionProbes(shading.reflectVector,
        positionWS, shading.perceptualRoughness, 0, fragment.normalWS);
#endif
#endif

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
        URPLight additionalURPLight = GetAdditionalLight(lightIndex, positionWS, SampleShadowMask(fragment.lightmapUV.xy));
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
        half4 shadowmask = SampleShadowMask(fragment.lightmapUV.xy);
        uint3 cluster = CBIRP::GetCluster(fragment.positionWS);

        ComputeCBIRPLights(diffuse, specular, cluster, shadowmask, fragment, shading, surface);

        #ifdef _CBIRP_REFLECTIONS
            indirectSpecular += CBIRP::SampleProbes(cluster, shading.reflectVector, fragment.positionWS, surface.Roughness).xyz;
        #endif
    #endif

    #ifdef _MIRROR
        float2 mirrorUV = fragment.positionNDC.xy;
        #ifdef _NORMAL_DROPOFF_TS
        mirrorUV.xy += surface.Normal.xy;
        #endif
        half4 mirrorReflection = unity_StereoEyeIndex == 0 ? 
            SAMPLE_TEXTURE2D(_ReflectionTex0, sampler_BilinearClamp, mirrorUV) :
            SAMPLE_TEXTURE2D(_ReflectionTex1, sampler_BilinearClamp, mirrorUV);
        alpha *= mirrorReflection.a;
        indirectSpecular = mirrorReflection.rgb;
    #endif

    #ifdef _AREALIT
        IntegrateAreaLit(diffuse, indirectSpecular, fragment, shading);
    #endif

    half3 brdf;
    half3 energyCompensation;
    EnvironmentBRDF(shading.NoV, shading.perceptualRoughness, shading.f0, brdf, energyCompensation);
    indirectSpecular *= brdf * energyCompensation;
    lightmapSpecular *= brdf * energyCompensation;
    bakedGI *= 1.0 - brdf;
    specular *= energyCompensation * PI;

    #ifndef DISABLE_SPECULAR_OCCLUSION
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

#ifndef UNITY_PASS_FORWARDADD
    color.rgb += surface.Emission;
#endif

    color.rgb = MixFog(color.rgb, InitializeInputDataFog(float4(positionWS, 1), 0));

    #if defined(_SURFACE_TYPE_TRANSPARENT)
        bool isTransparent = true;
    #else
        bool isTransparent = false;
    #endif
    
    color.a = OutputAlpha(color.a, isTransparent);

    #ifdef _ACES
        color.rgb = ACESFitted(color.rgb);
    #endif

    return color;
}