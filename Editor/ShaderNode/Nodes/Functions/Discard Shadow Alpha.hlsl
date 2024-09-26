void DiscardShadowAlpha(half alpha, out half Alpha)
{
	#ifdef UNITY_PASS_SHADOWCASTER
	discard;
	#endif
	Alpha = alpha;
}