void SampleLightmapAndSpecularNode(out half3 Diffuse, out half3 Specular, out half3 Color, out half4 Direction, float2 lightmapUV, float3 normalWS, float3 viewDirectionWS, half roughness)
{
	Diffuse = 0;
	Color = 0;
	Direction = 0;
	Specular = 0;

    float4 uv = float4(lightmapUV, 0, 0); // todo make lighmap uv float4

    // unused
    half3 indirectOcclusion = 0;
    half3 reflectVector = 0;
	SampleLightmap(Diffuse, Specular, uv, normalWS, viewDirectionWS, roughness, indirectOcclusion, reflectVector);

    Color = Diffuse; // what was this even supposed to be?
}