#pragma fragment frag

#include "Functions.hlsl"

#ifdef _CBIRP
    #include "Packages/z3y.clusteredbirp/Shaders/CBIRP.hlsl"
#endif
#ifdef _LTCGI
    #include "Assets/_pi_/_LTCGI/Shaders/LTCGI.cginc"
#endif

#ifndef QUALITY_LOW
    #define BAKERY_SHNONLINEAR
#endif 

half4 frag(Varyings varyings) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(varyings);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(varyings);
    
    SurfaceDescription desc = SurfaceDescriptionFunction(varyings);

    #if !defined(_ALPHATEST_ON) && !defined(_ALPHAPREMULTIPLY_ON) && !defined(_ALPHAMODULATE_ON) && !defined(_ALPHAFADE_ON)
        desc.Alpha = 1.0;
    #endif

    #if defined(_ALPHATEST_ON)
        if (desc.Alpha < desc.Cutoff) discard;
    #endif

    FragmentData data = FragmentData::Create(varyings);

    Light light = Light::Initialize(varyings);
    half3 lightColor = light.color;
    float3 lightDirection = light.direction;
    half lightAttenuation = light.attenuation;
    float3 positionWS = data.positionWS;
    float3 normalWS = SafeNormalize(mul(desc.Normal, data.tangentSpaceTransform));
    float3 tangentWS = data.tangentWS;
    float3 bitangentWS = data.bitangentWS;
    float3 viewDirectionWS = data.viewDirectionWS;

    half reflectance = 0.5;
    half specularAOIntensity = 1.0;
    half NoV = abs(dot(normalWS, viewDirectionWS)) + 1e-5f;
    half roughness2 = desc.Roughness * desc.Roughness;
    half roughness2Clamped = max(roughness2, 0.002);
    float3 reflectVector = reflect(-viewDirectionWS, normalWS);
    #if !defined(QUALITY_LOW)
        reflectVector = lerp(reflectVector, normalWS, roughness2);
    #endif
    half3 f0 = 0.16 * reflectance * reflectance * (1.0 - desc.Metallic) + desc.Albedo * desc.Metallic;
    half3 brdf;
    half3 energyCompensation;
    Filament::EnvironmentBRDF(NoV, desc.Roughness, f0, brdf, energyCompensation);


    half3 directDiffuse = 0;
    half3 directSpecular = 0;
    half3 indirectSpecular = 0;
    half3 indirectDiffuse = 0;
    half3 indirectOcclusion = 1;
    #if defined(LIGHTMAP_ON)
        float2 lightmapUV = varyings.lightmapUV;
        #if defined(_BICUBIC_LIGHTMAP) && !defined(QUALITY_LOW)
            float4 texelSize = TexelSizeFromTexture2D(unity_Lightmap);
            half3 illuminance = SampleTexture2DBicubic(unity_Lightmap, custom_bilinear_clamp_sampler, lightmapUV, texelSize, 1.0).rgb;
        #else
            half3 illuminance = DecodeLightmap(unity_Lightmap.SampleLevel(custom_bilinear_clamp_sampler, lightmapUV, 0));
        #endif

        #if defined(DIRLIGHTMAP_COMBINED) || defined(_BAKERY_MONOSH)
            #if defined(_BICUBIC_LIGHTMAP) && !defined(QUALITY_LOW)
                half4 directionalLightmap = SampleTexture2DBicubic(unity_LightmapInd, custom_bilinear_clamp_sampler, lightmapUV, texelSize, 1.0);
            #else
                half4 directionalLightmap = unity_LightmapInd.SampleLevel(custom_bilinear_clamp_sampler, lightmapUV, 0);
            #endif
            #ifdef _BAKERY_MONOSH
                half3 L0 = illuminance;
                half3 nL1 = directionalLightmap * 2.0 - 1.0;
                half3 L1x = nL1.x * L0 * 2.0;
                half3 L1y = nL1.y * L0 * 2.0;
                half3 L1z = nL1.z * L0 * 2.0;
                #ifdef BAKERY_SHNONLINEAR
                    float lumaL0 = dot(L0, 1);
                    float lumaL1x = dot(L1x, 1);
                    float lumaL1y = dot(L1y, 1);
                    float lumaL1z = dot(L1z, 1);
                    float lumaSH = shEvaluateDiffuseL1Geomerics(lumaL0, float3(lumaL1x, lumaL1y, lumaL1z), normalWS);

                    half3 sh = L0 + normalWS.x * L1x + normalWS.y * L1y + normalWS.z * L1z;
                    float regularLumaSH = dot(sh, 1);
                    sh *= lerp(1, lumaSH / regularLumaSH, saturate(regularLumaSH * 16));
                #else
                    half3 sh = L0 + normalWS.x * L1x + normalWS.y * L1y + normalWS.z * L1z;
                #endif

                illuminance = sh;
                #ifdef _LIGHTMAPPED_SPECULAR
                {
                    half smoothnessLm = 1.0f - roughness2Clamped;
                    smoothnessLm *= sqrt(saturate(length(nL1)));
                    half roughnessLm = 1.0f - smoothnessLm;
                    half3 dominantDir = nL1;
                    half3 halfDir = Unity_SafeNormalize(normalize(dominantDir) + viewDirectionWS);
                    half nh = saturate(dot(normalWS, halfDir));
                    half spec = Filament::D_GGX(nh, roughnessLm);
                    sh = L0 + dominantDir.x * L1x + dominantDir.y * L1y + dominantDir.z * L1z;
                    
                    #ifdef _ANISOTROPY
                        // half at = max(roughnessLm * (1.0 + surf.Anisotropy), 0.001);
                        // half ab = max(roughnessLm * (1.0 - surf.Anisotropy), 0.001);
                        // indirectSpecular += max(Filament::D_GGX_Anisotropic(nh, halfDir, sd.tangentWS, sd.bitangentWS, at, ab) * sh, 0.0);
                    #else
                        indirectSpecular += max(spec * sh, 0.0);
                    #endif
                }
                #endif
            #else
                half halfLambert = dot(normalWS, directionalLightmap.xyz - 0.5) + 0.5;
                illuminance = illuminance * halfLambert / max(1e-4, directionalLightmap.w);
            #endif
        #endif
        #if defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN)
            illuminance = SubtractMainLightWithRealtimeAttenuationFromLightmap(illuminance, light.attenuation, float4(0,0,0,0), normalWS);
            light = (Light)0;
        #endif

        indirectDiffuse = illuminance;

        #if defined(_BAKERY_MONOSH)
            indirectOcclusion = (dot(nL1, reflectVector) + 1.0) * L0 * 2.0;
        #else
            indirectOcclusion = illuminance;
        #endif

    #elif defined(UNITY_PASS_FORWARDBASE)
        #if defined(_FLATSHADING)
        {
            float3 sh9Dir = (unity_SHAr.xyz + unity_SHAg.xyz + unity_SHAb.xyz);
            float3 sh9DirAbs = float3(sh9Dir.x, abs(sh9Dir.y), sh9Dir.z);
            half3 N = normalize(sh9DirAbs);
            UNITY_FLATTEN
            if (!any(unity_SHC.xyz))
            {
                N = 0;
            }
            half3 l0l1 = SHEvalLinearL0L1(float4(N, 1));
            half3 l2 = SHEvalLinearL2(float4(N, 1));
            indirectDiffuse = l0l1 + l2;
        }
        #else
            #if UNITY_SAMPLE_FULL_SH_PER_PIXEL
                indirectDiffuse = ShadeSHPerPixel(normalWS, 0.0, positionWS);
            #else
                indirectDiffuse = ShadeSHPerPixel(normalWS, varyings.sh, positionWS);
            #endif
            indirectOcclusion = indirectDiffuse;
        #endif
    #endif
    indirectDiffuse = max(0.0, indirectDiffuse);


    // main light
    ShadeLight(light, viewDirectionWS, normalWS, desc.Roughness, NoV, f0, energyCompensation, directDiffuse, directSpecular);

    // reflection probes
    #if !defined(_GLOSSYREFLECTIONS_OFF)
        Unity_GlossyEnvironmentData envData;
        envData.roughness = desc.Roughness;
        envData.reflUVW = BoxProjectedCubemapDirection(reflectVector, positionWS, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);

        half3 probe0 = Unity_GlossyEnvironment(UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, envData);
        half3 reflectionSpecular = probe0;

        #if defined(UNITY_SPECCUBE_BLENDING)
            UNITY_BRANCH
            if (unity_SpecCube0_BoxMin.w < 0.99999)
            {
                envData.reflUVW = BoxProjectedCubemapDirection(reflectVector, positionWS, unity_SpecCube1_ProbePosition, unity_SpecCube1_BoxMin, unity_SpecCube1_BoxMax);
                float3 probe1 = Unity_GlossyEnvironment(UNITY_PASS_TEXCUBE_SAMPLER(unity_SpecCube1, unity_SpecCube0), unity_SpecCube1_HDR, envData);
                reflectionSpecular = lerp(probe1, probe0, unity_SpecCube0_BoxMin.w);
            }
        #endif
        indirectSpecular += reflectionSpecular;
    #endif

                    
    #ifdef _CBIRP
            #ifdef LIGHTMAP_ON
            half4 shadowmask = _Udon_CBIRP_ShadowMask.SampleLevel(custom_bilinear_clamp_sampler, lightmapUV, 0);
            // half4 shadowmask = 1;
        #else
            half4 shadowmask = 1;
    #endif
        directDiffuse = 0;
        directSpecular = 0;
        uint3 cluster = CBIRP::GetCluster(positionWS);
        CBIRP::ComputeLights(cluster, positionWS, normalWS, viewDirectionWS, f0, NoV, desc.Roughness, shadowmask, directDiffuse, directSpecular);
        directSpecular /= UNITY_PI;
        directSpecular *= energyCompensation;
        indirectSpecular = CBIRP::SampleProbes(cluster, reflectVector, positionWS, desc.Roughness).xyz;
    #endif

    #if !defined(QUALITY_LOW)
        float horizon = min(1.0 + dot(reflectVector, normalWS), 1.0);
        indirectSpecular *= horizon * horizon;
    #endif


    #ifdef _LTCGI
        float2 untransformedLightmapUV = 0;
        #ifdef LIGHTMAP_ON
        untransformedLightmapUV = (lightmapUV - unity_LightmapST.zw) / unity_LightmapST.xy;
        #endif
        float3 ltcgiSpecular = 0;
        float3 ltcgiDiffuse = 0;
        LTCGI_Contribution(positionWS.xyz, normalWS, viewDirectionWS, desc.Roughness, untransformedLightmapUV, ltcgiDiffuse, ltcgiSpecular);
        #ifndef LTCGI_DIFFUSE_DISABLED
            directDiffuse += ltcgiDiffuse;
        #endif
        indirectSpecular += ltcgiSpecular;
    #endif

    half3 fr;
    fr = energyCompensation * brdf;
    indirectSpecular *= fr;
    directSpecular *= UNITY_PI;

    half specularAO;
    #if defined(QUALITY_LOW)
        specularAO = desc.Occlusion;
    #else
        specularAO = Filament::ComputeSpecularAO(NoV, desc.Occlusion, roughness2);
    #endif
    directSpecular *= specularAO;
    specularAO *= lerp(1.0, saturate(sqrt(dot(indirectOcclusion + directDiffuse, 1.0))), specularAOIntensity);
    indirectSpecular *= specularAO;


    #ifdef _FLATSHADING
        indirectDiffuse = saturate(max(indirectDiffuse, directDiffuse));
        directDiffuse = 0.0;
        #if !(!defined(_ALPHATEST_ON) && !defined(_ALPHAPREMULTIPLY_ON) && !defined(_ALPHAMODULATE_ON) && !defined(_ALPHAFADE_ON))
            #ifdef UNITY_PASS_FORWARDADD
                desc.Albedo *= desc.Alpha; // theres probably a better way
            #endif
        #endif
    #endif

    AlphaTransparentBlend(desc.Alpha, desc.Albedo, desc.Metallic);

    half4 color = half4(desc.Albedo * (1.0 - desc.Metallic) * (indirectDiffuse * desc.Occlusion + directDiffuse), desc.Alpha);
    color.rgb += directSpecular;
    #if defined(UNITY_PASS_FORWARDBASE)
    color.rgb += indirectSpecular;
    color.rgb += desc.Emission;
    #endif


    UNITY_APPLY_FOG(varyings.fogCoord, color);

    return color;
}