#pragma fragment frag

// https://www.iquilezles.org/www/articles/spherefunctions/spherefunctions.htm
float sphIntersect_Preview(float3 ro, float3 rd, float4 sph)
{
    float3 oc = ro - sph.xyz;
    float b = dot( oc, rd );
    float c = dot( oc, oc ) - sph.w*sph.w;
    float h = b*b - c;
    if( h<0.0 ) return -1.0;
    h = sqrt( h );
    return -b - h;
}

half4 frag(Varyings varyings) : SV_Target
{
    // create data for preview
    float2 rawUV = UNPACK_UV0.xy;
    #ifdef PREVIEW3D
        // https://bgolus.medium.com/rendering-a-sphere-on-a-quad-13c92025570c
        float2 uv = UNPACK_UV0.xy;
        uv -= 0.5;
        uv *= 1.02;
        float3 spherePos = float3(0,0,0);
        float3 rayDir = normalize(float3(uv, 1));
        float3 rayOrigin = _WorldSpaceCameraPos.xyz;
        
        float rayHit = sphIntersect_Preview(rayOrigin, rayDir, float4(spherePos, 1));
        float alpha3D = 1;
        if (rayHit < 0) alpha3D = 0;

        float3 positionWS = rayDir * rayHit + rayOrigin;
        float3 normalWS = positionWS - spherePos;
        float4 tangentWS = float4(cross(normalWS, float3(0.0, 1.0, 0.0)), -1.0);

        tangentWS.xyz = tangentWS;

        float dist = length(uv);
        float pwidth = length(float2(ddx(dist), ddy(dist)));
        alpha3D *= smoothstep(0.5, 0.5 - pwidth * 1.5, dist);

        #ifdef UNPACK_POSITIONWS
            UNPACK_POSITIONWS = positionWS;
        #endif
        #ifdef UNPACK_NORMALWS
            UNPACK_NORMALWS = normalWS;
        #endif
        #ifdef UNPACK_TANGENTWS
            UNPACK_TANGENTWS = tangentWS;
        #endif

        #ifdef PREVIEW3D
        normalWS = normalize(normalWS);
        float2 generatedUV = float2(
            atan2(normalWS.z, normalWS.x) / (UNITY_PI * 2.0),
            acos(-normalWS.y) / UNITY_PI
        );
        generatedUV.x += 0.75;

        #ifdef UNPACK_UV0
            UNPACK_UV0.xy = generatedUV;
        #endif
        #ifdef UNPACK_UV1
            UNPACK_UV1.xy = generatedUV;
        #endif
        #ifdef UNPACK_UV2
            UNPACK_UV2.xy = generatedUV;
        #endif
        #ifdef UNPACK_UV3
            UNPACK_UV3.xy = generatedUV;
        #endif
        #endif

    #else // 2d preview
        #ifdef UNPACK_POSITIONWS
            UNPACK_POSITIONWS = float3(UNPACK_UV0.xy - 0.5, 0) * 2.0;
        #endif
        float3 normalWS = float3(0,0,1);
        #ifdef UNPACK_NORMALWS
            UNPACK_NORMALWS = normalWS;
        #endif
        #ifdef UNPACK_TANGENTWS
            UNPACK_TANGENTWS = float4(float3(1,0,0), 1.0);
        #endif
        #ifdef UNPACK_UV1
            UNPACK_UV1.xy = UNPACK_UV0.xy;
        #endif
        #ifdef UNPACK_UV2
            UNPACK_UV2.xy = UNPACK_UV0.xy;
        #endif
        #ifdef UNPACK_UV3
            UNPACK_UV3.xy = UNPACK_UV0.xy;
        #endif
    #endif
    
    #ifdef UNPACK_COLOR
        UNPACK_COLOR = 1.0;
    #endif


    SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(varyings);

    float2 checkerUV = rawUV * 8;
    float checkerboard = floor(checkerUV.x) + floor(checkerUV.y);
    checkerboard = frac(checkerboard * 0.5);
    checkerboard = checkerboard ? 0.35 : 0.4;
    // checkerboard = 1;

    half4 col = surfaceDescription.Color;
    half alpha = saturate(surfaceDescription.Color.a);

    #ifdef PREVIEW3D
        col.a = alpha3D;
        // col.rgb = dot(UNPACK_NORMALWS, normalize(float3(0.46, 0.18, -0.28)));
    #else
        col.a = 1;
    #endif
    col = saturate(col);
    col.r = LinearToGammaSpaceExact(col.r);
    col.g = LinearToGammaSpaceExact(col.g);
    col.b = LinearToGammaSpaceExact(col.b);
    col.rgb = lerp(checkerboard, col.rgb, alpha);

    return col;
}