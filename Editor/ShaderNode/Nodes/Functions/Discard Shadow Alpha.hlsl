void DiscardShadowAlpha(half alpha, bool shouldDiscard, out half Alpha)
{
	#ifdef UNITY_PASS_SHADOWCASTER
	if (shouldDiscard)
	{
		discard;
	}
	#endif
	Alpha = alpha;
}