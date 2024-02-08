void AudioLink_4BandAmplitudeLerp(float Band, float Delay, out float Out)
{
    Out = AudioLinkLerp( ALPASS_AUDIOLINK + float2( Delay, Band ) ).r;
}