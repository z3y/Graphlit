void SimpleWindFunction(out float3 Position, Texture2D noiseTexture, SamplerState noiseSampler, float3 positionWS, float3 intensity = 0.1, float scale = .02, float speed = 0.05)
{
	#ifndef UNITY_PASS_META
	half3 windNoise = SAMPLE_TEXTURE2D_LOD(noiseTexture, noiseSampler, (positionWS.xz * scale) + (_Time.y * speed), 0).rgb;
	windNoise = windNoise * 2.0 - 1.0;
	positionWS += windNoise * intensity;
	#endif
	Position = positionWS;
}