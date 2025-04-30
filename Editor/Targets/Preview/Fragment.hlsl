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

inline float LinearToGammaSpaceExactPreview(float value)
{
    if (value <= 0.0F)
        return 0.0F;
    else if (value <= 0.0031308F)
        return 12.92F * value;
    else if (value < 1.0F)
        return 1.055F * pow(value, 0.4166667F) - 0.055F;
    else
        return pow(value, 0.45454545F);
}

half4 frag(Varyings varyings) : SV_Target
{
    // create data for preview
    float2 rawUV = UNPACK_UV0.xy;
    float alpha3D = 1;
    
    UNITY_BRANCH
    if (_Preview3D)
    {
        // https://bgolus.medium.com/rendering-a-sphere-on-a-quad-13c92025570c
        float2 uv = UNPACK_UV0.xy;
        uv -= 0.5;
        uv *= 1.02;
        float3 spherePos = float3(0,0,0);
        float3 rayDir = normalize(float3(uv, 1));
        float3 rayOrigin = _WorldSpaceCameraPos.xyz;
        
        float rayHit = sphIntersect_Preview(rayOrigin, rayDir, float4(spherePos, 1));
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

        normalWS = normalize(normalWS);
        float2 generatedUV = float2(
            atan2(normalWS.z, normalWS.x) / (PI * 2.0),
            acos(-normalWS.y) / PI
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
    }
    else // 2d preview
    {
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
    }
    
    #ifdef UNPACK_COLOR
        UNPACK_COLOR = 1.0;
    #endif

    #ifdef UNPACK_POSITIONCSR
        // TODO: figure out how to make an accurate preview
        UNPACK_POSITIONCSR = float4((rawUV.xy - 0.5) * float2(1, -1), 0, 1);
    #endif


    SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(varyings);

    float2 checkerUV = rawUV * 8;
    float checkerboard = floor(checkerUV.x) + floor(checkerUV.y);
    checkerboard = frac(checkerboard * 0.5);
    checkerboard = checkerboard ? 0.35 : 0.4;
    // checkerboard = 1;

    float4 col = surfaceDescription.Color;
    #ifdef TEXTURE_OUTPUT
        return col;
    #endif
    float alpha = saturate(surfaceDescription.Color.a);

    col.a = _Preview3D ? alpha3D : 1.0;

    col = saturate(col);
    col.r = LinearToGammaSpaceExactPreview(col.r);
    col.g = LinearToGammaSpaceExactPreview(col.g);
    col.b = LinearToGammaSpaceExactPreview(col.b);
    col.rgb = lerp(checkerboard, col.rgb, alpha);

    return col;
}