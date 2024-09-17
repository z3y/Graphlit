void SampleLightmapAndSpecularNode(out half3 Diffuse, out half3 Specular, out half3 Color, out half4 Direction, float2 lightmapUV, float3 normalWS, float3 viewDirectionWS, half roughness)
{
	Diffuse = 0;
	Color = 0;
	Direction = 0;
	Specular = 0;

	half roughness2 = roughness * roughness;

	#if defined(LIGHTMAP_ON)
        #if defined(_BICUBIC_LIGHTMAP) && !defined(QUALITY_LOW)
            float4 texelSize = TexelSizeFromTexture2D(unity_Lightmap);
            half3 illuminance = SampleTexture2DBicubic(unity_Lightmap, custom_bilinear_clamp_sampler, lightmapUV, texelSize, 1.0).rgb;
        #else
            half3 illuminance = DecodeLightmap(unity_Lightmap.SampleLevel(custom_bilinear_clamp_sampler, lightmapUV, 0));
        #endif

		Color = illuminance;

        #if defined(DIRLIGHTMAP_COMBINED) || defined(_BAKERY_MONOSH)
            #if defined(_BICUBIC_LIGHTMAP) && !defined(QUALITY_LOW)
                half4 directionalLightmap = SampleTexture2DBicubic(unity_LightmapInd, custom_bilinear_clamp_sampler, lightmapUV, texelSize, 1.0);
            #else
                half4 directionalLightmap = unity_LightmapInd.SampleLevel(custom_bilinear_clamp_sampler, lightmapUV, 0);
            #endif
			Direction = directionalLightmap;
            #ifdef _BAKERY_MONOSH
                half3 L0 = illuminance;
                half3 nL1 = directionalLightmap * 2.0 - 1.0;
				Direction = nL1;
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
                    half smoothnessLm = 1.0f - max(roughness2, 0.002);
                    smoothnessLm *= sqrt(saturate(length(nL1)));
                    half roughnessLm = 1.0f - smoothnessLm;
                    half3 dominantDir = nL1;
                    half3 halfDir = Unity_SafeNormalize(normalize(dominantDir) + fragData.viewDirectionWS);
                    half nh = saturate(dot(normalWS, halfDir));
                    half spec = Filament::D_GGX(nh, roughnessLm);
                    sh = L0 + dominantDir.x * L1x + dominantDir.y * L1y + dominantDir.z * L1z;
                    
                    #ifdef _ANISOTROPY
                        // half at = max(roughnessLm * (1.0 + surf.Anisotropy), 0.001);
                        // half ab = max(roughnessLm * (1.0 - surf.Anisotropy), 0.001);
                        // giOutput.indirectSpecular += max(Filament::D_GGX_Anisotropic(nh, halfDir, sd.tangentWS, sd.bitangentWS, at, ab) * sh, 0.0);
                    #else
                        Specular += max(spec * sh, 0.0);
                    #endif
                }
                #endif
            #else
                half halfLambert = dot(normalWS, directionalLightmap.xyz - 0.5) + 0.5;
                illuminance = illuminance * halfLambert / max(1e-4, directionalLightmap.w);
            #endif
        #endif
        #if defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN)
            illuminance = SubtractMainLightWithRealtimeAttenuationFromLightmap(illuminance, unityLight.attenuation, float4(0,0,0,0), normalWS);
            unityLight.color = 0;
        #endif

        Diffuse = illuminance;

        // #if defined(_BAKERY_MONOSH)
        //     giOutput.indirectOcclusion = (dot(nL1, giInput.reflectVector) + 1.0) * L0 * 2.0;
        // #else
        //     giOutput.indirectOcclusion = illuminance;
        // #endif
	#endif
}