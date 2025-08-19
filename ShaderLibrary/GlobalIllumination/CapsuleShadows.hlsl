// Closest distance between a ray (ro + u*rd, u>=0) and a segment (a + v*ab, v in [0,1])
float ClosestDist_Ray_Segment(float3 ro, float3 rd, float3 a, float3 b, out float uOut)
{
    float3 ab = b - a;
    float3 ao = ro - a;

    float A = 1.0;                   // dot(rd, rd) assuming rd normalized
    float B = dot(rd, ab);
    float C = dot(ab, ab);
    float D = dot(rd, ao);
    float E = dot(ab, ao);

    float denom = A*C - B*B;

    float u = 0.0; // ray param (>= 0)
    float v = 0.0; // segment param [0,1]

    if (denom > 1e-6)
    {
        // Unconstrained closest
        float uUn = (B*E - C*D) / denom;
        float vUn = (A*E - B*D) / denom;

        // Enforce constraints u>=0, v in [0,1]:
        v = saturate(vUn);

        // If clamping v changed it significantly, recompute u for that v
        // Otherwise keep uUn (but clamp u to >= 0)
        if (abs(v - vUn) > 1e-6)
        {
            // Closest point on ray to fixed segment point P(a+v*ab)
            float3 p = a + v * ab;
            u = max(0.0, dot(p - ro, rd));
        }
        else
        {
            u = max(0.0, uUn);
        }

        // If clamping u changed it to 0, recompute v = closest point on segment to ro
        if (u == 0.0)
        {
            v = (C > 1e-6) ? saturate(dot(ao, ab) / C) : 0.0;
        }
    }
    else
    {
        // Segment is degenerate: compare to the point a (or b ~ a)
        v = 0.0;
        u = max(0.0, dot(a - ro, rd));
    }

    uOut = u;
    float3 pRay = ro + u * rd;
    float3 pSeg = a + v * ab;
    return length(pRay - pSeg);
}

// Soft capsule shadow from a single capsule
// Inputs:
//   X             : world position of the shaded point (current pixel)
//   L             : light-to-scene direction (must be normalized)
//   P0, P1, radius: capsule data
//   lightRadius   : controls penumbra width (think "area light size" in world units)
//   maxRayLen     : optional fade distance (to avoid infinitely long shadows)
float SingleCapsuleShadow(float3 positionWS, float3 L, float3 P0, float3 P1, float radius, float lightRadius, float maxRayLen)
{
    float u; // distance along the ray toward the light
    // Ray from the point toward the light *source*: opposite the light direction
    float3 ro = positionWS;
    float3 rd = L; // toward light

    float d = ClosestDist_Ray_Segment(ro, rd, P0, P1, u); // closest separation
    // If the closest approach happens behind the point (u < 0) we already clamped to 0

    // Optional: fade with distance along the shadow to keep it finite
    float distFade = (maxRayLen > 0.0) ? saturate(1.0 - u / maxRayLen) : 1.0;

    // Penumbra grows with distance along the ray (simple linear model)
    // s = effective "soft edge" thickness
    float s = lightRadius * (0.1 + u); // tune as needed

    // Signed distance from the expanded capsule (negative = fully occluded)
    float sd = d - radius;

    // Smooth step from full occlusion (sd<=0) to fully lit (sd>=s)
    // s = max(0.15, lightRadius * (1 + u));
    float occl = 1.0 - smoothstep(0.0, s, sd);

    return occl * distFade; // 0..1 shadow amount (1 = fully shadowed)
}

float4 _UdonCapsuleShadowsPoints[32];
float4 _UdonCapsuleShadowsData[32];

uint _UdonCapsuleShadowsPointsCount;
float4 _UdonCapsuleShadowsParams;

half3 CapsuleShadows(float3 positionWS, float3 normalWS, half4 directionalLightmap)
{
    #ifdef LIGHTMAP_ON
        half3 L = normalize(directionalLightmap.rgb * 2.0 - 1.0);
        half focus = sqrt(saturate(length(directionalLightmap.rgb * 2.0 - 1.0)));
    #else
        half3 L = normalize((unity_SHAr.xyz + unity_SHAg.xyz + unity_SHAb.xyz) * 1.0/3.0);
        half focus = 1;
    #endif
    half shadow = 0;
    half NoL = saturate(dot(normalWS, L));
    for (uint cs = 0; cs < _UdonCapsuleShadowsPointsCount;)
    {
        half R = _UdonCapsuleShadowsData[cs].r;
        float3 P0 = _UdonCapsuleShadowsPoints[cs++].xyz;
        float3 P1 = _UdonCapsuleShadowsPoints[cs++].xyz;

        shadow = max(shadow, SingleCapsuleShadow(positionWS, L, P0, P1, R, _UdonCapsuleShadowsParams.y, _UdonCapsuleShadowsParams.z));
    }

    // return shadow;

    return 1.0 - (saturate(shadow) * sqrt(NoL));
}