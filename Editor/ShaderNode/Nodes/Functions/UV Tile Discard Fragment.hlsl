
// r3  | 1031 | 1032 | 1033 | 1034
// r2  | 1021 | 1022 | 1023 | 1024
// r1  | 1011 | 1012 | 1013 | 1014
// r0  | 1001 | 1002 | 1003 | 1004
// UV  | x    | y    | z    | w

void UVTileDiscardFragment(float3 In, float2 UV, float4 r0, float4 r1, float4 r2, float4 r3, out float3 Out)
{
    int2 uv = floor(UV);

    float4 row = r0;
    if (uv.y == 1) row = r1;
    else if (uv.y == 2) row = r2;
    else if (uv.y == 3) row = r3;

    float flag = row.x;
    if (uv.x == 1) flag = row.y;
    else if (uv.x == 2) flag = row.z;
    else if (uv.x == 3) flag = row.w;

    if (flag)
    {
       discard;
    }
    
	Out = In;
}