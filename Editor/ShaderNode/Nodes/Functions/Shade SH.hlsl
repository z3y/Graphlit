void ShadeSHNode(float3 NormalWS, float3 PositionWS, out float3 Out)
{
	#ifdef PREVIEW
		Out = 0.15;
	#else
		#ifdef UNITY_PASS_FORWARDBASE
			Out = max(0, ShadeSHPerPixel(NormalWS, 0.0, PositionWS));
		#else
			Out = 0;
		#endif
	#endif
}