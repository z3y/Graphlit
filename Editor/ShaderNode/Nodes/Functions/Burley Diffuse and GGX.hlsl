float pow5_1(float x)
{
	float x2 = x * x;
	return x2 * x2 * x;
}

half3 F_Schlick_1(half u, half3 f0)
{
	return f0 + (1.0 - f0) * pow(1.0 - u, 5.0);
}

float F_Schlick_1(float f0, float f90, float VoH)
{
	return f0 + (f90 - f0) * pow5_1(1.0 - VoH);
}

half Fd_Burley_1(half roughness, half NoV, half NoL, half LoH)
{
	// Burley 2012, "Physically-Based Shading at Disney"
	half f90 = 0.5 + 2.0 * roughness * LoH * LoH;
	float lightScatter = F_Schlick_1(1.0, f90, NoL);
	float viewScatter  = F_Schlick_1(1.0, f90, NoV);
	return lightScatter * viewScatter;
}

half D_GGX_1(half NoH, half roughness)
{
	half a = NoH * roughness;
	half k = roughness / (1.0 - NoH * NoH + a * a);
	return k * k * (1.0 / UNITY_PI);
}

float D_GGX_Anisotropic_1(float NoH, float3 h, float3 t, float3 b, float at, float ab)
{
	half ToH = dot(t, h);
	half BoH = dot(b, h);
	half a2 = at * ab;
	float3 v = float3(ab * ToH, at * BoH, a2 * NoH);
	float v2 = dot(v, v);
	half w2 = a2 / v2;
	return a2 * w2 * w2 * (1.0 / UNITY_PI);
}

float V_SmithGGXCorrelatedFast_1(half NoV, half NoL, half roughness)
{
	half a = roughness;
	float GGXV = NoL * (NoV * (1.0 - a) + a);
	float GGXL = NoV * (NoL * (1.0 - a) + a);
	return 0.5 / (GGXV + GGXL);
}

float V_SmithGGXCorrelated_1(half NoV, half NoL, half roughness)
{
	#ifdef QUALITY_LOW
		return V_SmithGGXCorrelatedFast_1(NoV, NoL, roughness);
	#else
		half a2 = roughness * roughness;
		float GGXV = NoL * sqrt(NoV * NoV * (1.0 - a2) + a2);
		float GGXL = NoV * sqrt(NoL * NoL * (1.0 - a2) + a2);
		return 0.5 / (GGXV + GGXL);
	#endif
}

float V_SmithGGXCorrelated_Anisotropic_1(float at, float ab, float ToV, float BoV, float ToL, float BoL, float NoV, float NoL)
{
	float lambdaV = NoL * length(float3(at * ToV, ab * BoV, NoV));
	float lambdaL = NoV * length(float3(at * ToL, ab * BoL, NoL));
	float v = 0.5 / (lambdaV + lambdaL);
	return saturate(v);
}

void BurleyDiffuseAndGGXSpecularLightNode(out half3 diffuse, out half3 specular, half3 color, float3 direction, half attenuation, float3 normalWS, float3 viewDirectionWS, half roughness, half metallic, half3 albedo = 1, half reflectance = 0.5, half energyCompensation = 1)
{
	diffuse = 0;
	specular = 0;
	#if defined(UNITY_PASS_FORWARDBASE ) || defined(UNITY_PASS_FORWARDADD) || defined(PREVIEW)
		half3 f0 = 0.16 * reflectance * reflectance * (1.0 - metallic) + albedo * metallic;
		half NoV = abs(dot(normalWS, viewDirectionWS)) + 1e-5f;
		half NoL = saturate(dot(normalWS, direction));
		float3 halfVector = SafeNormalize(direction + viewDirectionWS);
		half LoH = saturate(dot(direction, halfVector));
		half NoH = saturate(dot(normalWS, halfVector));
		UNITY_BRANCH
		if (NoL > 0)
		{
			diffuse = NoL * attenuation * color;

			#if !defined(QUALITY_LOW)
				diffuse *= Fd_Burley_1(roughness, NoV, NoL, LoH);
			#endif

			#ifndef _SPECULARHIGHLIGHTS_OFF
				half clampedRoughness = max(roughness * roughness, 0.002);

				half3 F = F_Schlick_1(LoH, f0) * energyCompensation;
				half D = D_GGX_1(NoH, clampedRoughness);
				half V = V_SmithGGXCorrelated_1(NoV, NoL, clampedRoughness);

				specular = max(0.0, (D * V) * F) * diffuse * UNITY_PI * energyCompensation;
			#endif
		}
	#endif
}