void ShadeSHNode(float3 NormalWS, float3 PositionWS, out float3 Diffuse)
{
	#ifdef PREVIEW
		Diffuse = 0.15;
	#else
		#ifdef UNITY_PASS_FORWARDBASE
			Diffuse = max(0, ShadeSHPerPixel(NormalWS, 0.0, PositionWS));
		#else
			Diffuse = 0;
		#endif
	#endif
}