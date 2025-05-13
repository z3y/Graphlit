void ShadeSHNode(float3 NormalWS, float3 PositionWS, out float3 Diffuse)
{
	#ifdef PREVIEW
		Diffuse = 0.15;
	#else
		Diffuse = SampleSH(NormalWS, PositionWS);
	#endif
}