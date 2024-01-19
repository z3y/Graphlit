#pragma fragment frag

// https://www.iquilezles.org/www/articles/spherefunctions/spherefunctions.htm
float sphIntersect(float3 ro, float3 rd, float4 sph)
{
    float3 oc = ro - sph.xyz;
    float b = dot( oc, rd );
    float c = dot( oc, oc ) - sph.w*sph.w;
    float h = b*b - c;
    // if( h<0.0 ) return -1.0;
    if( h<0.0 ) return -b;
    h = sqrt( h );
    return -b - h;
}

half4 frag(VaryingsWrapper varyings) : SV_Target
{
    #ifdef PREVIEW
        // https://bgolus.medium.com/rendering-a-sphere-on-a-quad-13c92025570c
        float2 uv = UNPACK_UV0.xy;
        uv -= 0.5;
        uv *= 1.02;
        float3 rayDir = normalize(float3(uv, 1));
        float3 rayOrigin = float3(0,0,0);
        const float offset = 2.23;
        float3 spherePos = float3(0,0, offset);
        float rayHit = sphIntersect(rayOrigin, rayDir, float4(spherePos, 1));

        float3 positionWS = rayDir * rayHit + rayOrigin;
        float3 normalWS = positionWS - spherePos;
        float4 tangentWS = float4(cross(normalWS, float3(0.0, 1.0, 0.0)), 1.0);

        tangentWS.xyz = tangentWS;
        positionWS.z -= offset;

        float dist = length(uv);
        float pwidth = length(float2(ddx(dist), ddy(dist)));
        float alpha3D = smoothstep(0.5, 0.5 - pwidth * 1.5, dist);

        #ifdef UNPACK_POSITIONWS
            UNPACK_POSITIONWS = positionWS;
        #endif
        #ifdef UNPACK_NORMALWS
            UNPACK_NORMALWS = normalWS;
        #endif
        #ifdef UNPACK_TANGENTWS
            UNPACK_TANGENTWS = tangentWS;
        #endif
    #endif

    SurfaceDescription surfaceDescription = SurfaceDescriptionFunction((Varyings)varyings);

    half4 col = surfaceDescription.Color;

        
    #ifdef PREVIEW
        #ifdef PREVIEW3D
            col.a = alpha3D;
            // col.rgb = dot(UNPACK_NORMALWS, normalize(float3(0.46, 0.18, -0.28)));
        #else
            col.a = 1;
        #endif
        col.r = LinearToGammaSpaceExact(col.r);
        col.g = LinearToGammaSpaceExact(col.g);
        col.b = LinearToGammaSpaceExact(col.b);
        col = saturate(col);
    #else 
        UNITY_APPLY_FOG(i.fogCoord, col);
    #endif

    return col;
}