void AudioLink_4BandAmplitude(float Band, float Delay, out float Out)
{
    Out = AudioLinkData(ALPASS_AUDIOLINK + uint2( Delay, Band ) ).r;
}