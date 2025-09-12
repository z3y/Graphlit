// #pragma fragment frag

#include "GlobalIllumination/ClusteredBIRP.hlsl"
#include "GlobalIllumination/LTCGI.hlsl"
#include "GlobalIllumination/AreaLit.hlsl"

#ifdef _ACES
    #include "ACES.hlsl"
#endif

#ifdef _VRCTRACE
    #include "Packages/com.z3y.vrctrace/Runtime/Shaders/VRCTrace.hlsl"
#endif


float4 frag(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    FragmentData fragment = FragmentData::Create(input);

    SurfaceDescription surface = SurfaceDescriptionFunction(input);

    #if !defined(UNITY_PASS_FORWARDADD)
        half3 emissionEDF = surface.Emission;
    #else
        half3 emissionEDF = 0;
    #endif


    half alpha = surface.Alpha;
    #if defined(_ALPHATEST_ON)
        alpha = AlphaClip(surface.Alpha, surface.Cutoff);
    #endif

    surface.Roughness = ComputeCoatAffectedRoughness(surface.Roughness, surface.CoatRoughness, surface.CoatWeight);
    half3 coatAttenuation = lerp(1.0, surface.CoatColor, surface.CoatWeight);
    surface.Albedo *= coatAttenuation;

    half3 diffuseColor = surface.Albedo * (1.0 - surface.Metallic);
    if (USE_ALPHAMULTIPLY)
    {
        
        diffuseColor = lerp(1.0, diffuseColor, alpha);
    }
    if (USE_ALPHAPREMULTIPLY)
    {
        diffuseColor *= alpha;
    }

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
    shading.coatNoV = abs(dot(fragment.normalWS, fragment.viewDirectionWS)) + 1e-5f;
    shading.normalWS = normalWS;
    shading.geometricNormalWS = fragment.normalWS;
    shading.reflectVector = reflect(-fragment.viewDirectionWS, normalWS);
    shading.coatReflectVector = reflect(-fragment.viewDirectionWS, fragment.normalWS);
    shading.perceptualRoughness = surface.Roughness;
    #ifdef QUALITY_LOW
    shading.specularRoughness = max(surface.Roughness * surface.Roughness, HALF_MIN_SQRT);
    #else
    shading.specularRoughness = max(surface.Roughness * surface.Roughness, 0.002);
    #endif

    shading.viewDirectionWS = fragment.viewDirectionWS;
    // half dielectricSpecularF0 = 0.16 * surface.Reflectance * surface.Reflectance;
    half dielectricSpecularF0 = IorToFresnel0(surface.IOR + 0.0001);
    shading.f0 = surface.Albedo * surface.Metallic + dielectricSpecularF0 * (1.0 - surface.Metallic);
    shading.f82 = surface.SpecularColor;
    shading.metallic = surface.Metallic;

    shading.coatf0 = IorToFresnel0(surface.CoatIOR + 0.0001);
    shading.coatWeight = surface.CoatWeight;
    shading.coatSpecularRoughness = max(surface.CoatRoughness * surface.CoatRoughness, 0.002);

    #ifdef _ANISOTROPY
        float3 tangentWS = TransformTangentToWorld(surface.Tangent, fragment.tangentSpaceTransform);
        tangentWS = Orthonormalize(tangentWS, normalWS);
        float3 bitangentWS = normalize(cross(normalWS, tangentWS));

        float3 anisotropicDirection = surface.Anisotropy >= 0.0 ? bitangentWS : tangentWS;
        float3 anisotropicTangent = cross(anisotropicDirection, fragment.viewDirectionWS);
        float3 anisotropicNormal = cross(anisotropicTangent, anisotropicDirection);
        float bendFactor = abs(surface.Anisotropy) * saturate(1.0 - (pow5(1.0 - surface.Roughness)));
        float3 bentNormal = normalize(lerp(normalWS, anisotropicNormal, bendFactor));
        shading.reflectVector = reflect(-fragment.viewDirectionWS, bentNormal);
        shading.bitangentWS = bitangentWS;
        shading.tangentWS = tangentWS;
        shading.anisotropy = surface.Anisotropy;
    #endif

    
    #if !defined(QUALITY_LOW)
        float topIor = 1.0;
        half tfNoV = shading.NoV;
        #ifdef _ANISOTROPY
            tfNoV = abs(dot(bentNormal, fragment.viewDirectionWS)) + 1e-5f;
        #endif
        shading.f0 = lerp(shading.f0, EvalIridescence(topIor, tfNoV, surface.ThinFilmThickness, shading.f0, surface.IOR), surface.ThinFilmWeight);
    #endif

    #ifndef QUALITY_LOW
        shading.reflectVector = lerp(shading.reflectVector, normalWS, surface.Roughness * surface.Roughness);
    #endif

    float3 positionWS = fragment.positionWS;

    half3 bakedGI = 0;
    half3 lightmapSpecular = 0;
    half3 indirectOcclusion = 0;
    #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
        half4 directionalLightmap;
        SampleLightmap(bakedGI, lightmapSpecular, directionalLightmap, fragment.lightmapUV, normalWS, fragment.viewDirectionWS, shading.perceptualRoughness, indirectOcclusion, shading.reflectVector);

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

    #if defined(LIGHTMAP_SPECULAR) && !defined(LIGHTMAP_ON)
    if (!light.enabled)
    {
        #ifdef _VRC_LIGHTVOLUMES
            half3 lvL0; half3 lvL1r; half3 lvL1g; half3 lvL1b;
            LightVolumeSH(positionWS, lvL0, lvL1r, lvL1g, lvL1b);
            light.direction = normalize((lvL1r + lvL1g + lvL1b) * 1.0/3.0);
            light.color = lvL0;
        #else
            light.direction = normalize((unity_SHAr.xyz + unity_SHAg.xyz + unity_SHAb.xyz) * 1.0/3.0);
            light.color = half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
        #endif
        light.specularOnly = true;
        light.enabled = true;
    }
    #endif

    half3 indirectSpecular = 0;
#ifndef UNITY_PASS_FORWARDADD
#if !defined(_GLOSSYREFLECTIONS_OFF) && !defined(_CBIRP_REFLECTIONS)
    indirectSpecular = CalculateIrradianceFromReflectionProbes(shading.reflectVector,
        positionWS, shading.perceptualRoughness, 0, fragment.normalWS);
#endif
#endif

#if !defined(UNITY_PASS_FORWARDADD)
    half3 coatResponse = CalculateIrradianceFromReflectionProbes(shading.coatReflectVector, positionWS, surface.CoatRoughness, 0, fragment.normalWS);
    half3 coatBrdf, coatEnergyCompensation;
    EnvironmentBRDF(shading.coatNoV, surface.CoatRoughness, shading.coatf0, coatBrdf, coatEnergyCompensation, 1, 0);
    half3 coatAvgDirAlbedo = dot(coatBrdf, 1.0 / 3.0);
    half3 coatThroughput = 1.0 - coatAvgDirAlbedo * surface.CoatWeight;
    indirectSpecular *= coatThroughput;
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

    half3 brdf, energyCompensation;
    EnvironmentBRDF(shading.NoV, shading.perceptualRoughness, shading.f0, brdf, energyCompensation, surface.SpecularColor, surface.Metallic);
    half3 dirAlbedo = brdf * energyCompensation;
    half avgDirAlbedo = dot(dirAlbedo, 1.0 / 3.0);
    indirectSpecular *= dirAlbedo;
    lightmapSpecular *= dirAlbedo;
    specular *= energyCompensation * PI;
    half3 indirectSpecularThroughput = 1.0 - avgDirAlbedo * 1;

#if !defined(UNITY_PASS_FORWARDADD)
    coatResponse *= coatBrdf * surface.CoatWeight;
    indirectSpecular += coatResponse;
    indirectSpecularThroughput = coatThroughput * indirectSpecularThroughput;
#endif

    #if defined(_VRCTRACE) && defined(UNITY_PASS_FORWARDBASE) && !defined(_GLOSSYREFLECTIONS_OFF) && !defined(QUALITY_LOW)

        float2 xi = GetRand(input.positionCS.xy * _Time.y);

        Ray ray;
        float3 newDir = lerp(shading.reflectVector, RandomDirectionInHemisphere(normalWS, xi), surface.Roughness * surface.Roughness);
        ray.D = newDir;
        ray.P = RayOffset(positionWS, ray.D);

        Intersection intersection;
        if (SceneIntersects(ray, intersection))
        {
            float3 hitP, hitN;
            TrianglePointNormal(intersection, hitP, hitN);
            hitN = TriangleSmoothNormal(intersection, hitN);
            float2 hitUV = TriangleUV(intersection);
            float3 hitCombined = SAMPLE_TEXTURE2D_LOD(_UdonVRCTraceCombinedAtlas, sampler_BilinearClamp, hitUV, 0);
            indirectSpecular = max(0, hitCombined * dirAlbedo);
        }
        else
        {
            // miss should only sample the skybox
            half4 encodedIrradiance = half4(SAMPLE_TEXTURECUBE_LOD(_UdonVRCTraceSkybox, sampler_UdonVRCTraceSkybox, shading.reflectVector, 0));
            indirectSpecular = encodedIrradiance.rgb;
        }
        specular = 0;
        lightmapSpecular = 0;
    #endif

    bakedGI *= indirectSpecularThroughput;

#if !defined(UNITY_PASS_FORWARDADD) && !defined(DISABLE_SPECULAR_OCCLUSION)
    #if defined(SHADOWS_SCREEN) && defined(SPECULAR_OCCLUSION_REALTIME_SHADOWS)
        half NoL = saturate(dot(normalWS, light.direction));
        indirectOcclusion *= NoL * light.shadowAttenuation;
    #endif
    half occlusionFromLightmap = sqrt(dot(indirectOcclusion + diffuse, _SpecularOcclusionExp));
    occlusionFromLightmap = saturate(lerp(1.0, occlusionFromLightmap, _SpecularOcclusion));
    indirectSpecular *= occlusionFromLightmap;
#endif
    
    #if defined(_MASKMAP) || defined(_OCCLUSION) || defined(_OCCLUSIONMAP) // doesnt get optimized out even if occlusion is 1
        half singleBounceAO = GetSpecularOcclusionFromAmbientOcclusion(shading.NoV, surface.Occlusion,
            surface.Roughness * surface.Roughness);
        indirectSpecular *= GTAOMultiBounce(singleBounceAO, shading.f0);
        bakedGI *= GTAOMultiBounce(surface.Occlusion, diffuseColor);
    #endif


    float4 color = float4(diffuseColor * (diffuse + bakedGI) + specular + lightmapSpecular + indirectSpecular, alpha);

#ifndef UNITY_PASS_FORWARDADD
    half3 coatTintedEmisionEDF = emissionEDF * surface.CoatColor;
    half3 coatedEmissionEDF = F_Schlick(1.0 - shading.coatf0, 0, shading.coatNoV) * coatTintedEmisionEDF;
    emissionEDF = lerp(emissionEDF, coatedEmissionEDF, surface.CoatWeight);

    color.rgb += emissionEDF;
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

    #ifdef _MODIFY_FINAL_COLOR
        ModifyFinalColor(color);
    #endif

    return color;
}