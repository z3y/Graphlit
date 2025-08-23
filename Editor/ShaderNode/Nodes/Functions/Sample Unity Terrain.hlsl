void SampleUnityTerrain(float2 UV, out half3 albedo, out half roughness, out half metallic, out half occlusion, out half alpha, out float3 normalTS)
{
	
	float2 uv = UV;
        
	float2 splatUV = (uv * (_Control_TexelSize.zw - 1.0f) + 0.5f) * _Control_TexelSize.xy;
	half4 splatControl = SAMPLE_TEXTURE2D(_Control, sampler_Control, splatUV);

	float4 uvSplat01;
	uvSplat01.xy = uv * _Splat0_ST.xy + _Splat0_ST.zw;
	uvSplat01.zw = uv * _Splat1_ST.xy + _Splat1_ST.zw;
	float4 uvSplat23;
	uvSplat23.xy = uv * _Splat2_ST.xy + _Splat2_ST.zw;
	uvSplat23.zw = uv * _Splat3_ST.xy + _Splat3_ST.zw;

	#ifdef _MASKMAP
		half4 maskMaps[4];
		maskMaps[0] = SAMPLE_TEXTURE2D(_Mask0, sampler_Mask0, uvSplat01.xy);
		maskMaps[1] = SAMPLE_TEXTURE2D(_Mask1, sampler_Mask0, uvSplat01.zw);
		maskMaps[2] = SAMPLE_TEXTURE2D(_Mask2, sampler_Mask0, uvSplat23.xy);
		maskMaps[3] = SAMPLE_TEXTURE2D(_Mask3, sampler_Mask0, uvSplat23.zw);
	#endif

	#if defined(_TERRAIN_BLEND_HEIGHT) && defined(_MASKMAP)
		HeightBasedSplatModify(splatControl, half4(maskMaps[0].b, maskMaps[1].b, maskMaps[2].b, maskMaps[3].b));
	#endif

	normalTS = float3(0,0,1);
	half smoothness = 0.5;
	metallic = 0;
	occlusion = 1;
	alpha = 1;

	half weight;
	half4 mixedDiffuse;
	half4 defaultSmoothness;
	SplatmapMix(uvSplat01, uvSplat23, splatControl, weight, mixedDiffuse, defaultSmoothness, normalTS);
	half4 defaultMetallic = half4(_Metallic0, _Metallic1, _Metallic2, _Metallic3);
	smoothness = dot(splatControl, defaultSmoothness);
	metallic = dot(splatControl, defaultMetallic);
	half4 defaultOcclusion = 1.0;

	albedo = mixedDiffuse.rgb;
	alpha = mixedDiffuse.a;

	#ifdef _MASKMAP
		half4 maskMap = 0;
		maskMap += splatControl.r * maskMaps[0];
		maskMap += splatControl.g * maskMaps[1];
		maskMap += splatControl.b * maskMaps[2];
		maskMap += splatControl.a * maskMaps[3];
		
		smoothness *= maskMap.a;
		metallic *= maskMap.r;
		occlusion *= maskMap.g;
	#endif

	#ifdef _ALPHATEST_ON
		ClipHoles(uv);
		#undef _ALPHATEST_ON
	#endif

	#ifdef TERRAIN_SPLAT_ADDPASS
		surface.Alpha = weight;
		#define _ALPHAMODULATE_ON
	#endif

	roughness = 1.0 - saturate(smoothness);
	metallic = saturate(metallic);
}