void ShadeSHNode(float3 NormalWS, float3 PositionWS, out float3 Diffuse)
{
	#ifdef PREVIEW
		Diffuse = 0.15;
	#else
			Diffuse = 0;
		#ifdef UNITY_PASS_FORWARDBASE
			Diffuse = max(0, ShadeSHPerPixel(NormalWS, 0.0, PositionWS));
		#endif

		#ifdef UNIVERSALRP
			Diffuse = SampleSH(NormalWS);
		#endif
	#endif
}