void AudioLink_4BandAmplitude(float Band, float Delay, out float Out)
{
    #ifdef AUDIOLINK_CGINC_INCLUDED
    Out = AudioLinkData(ALPASS_AUDIOLINK + uint2( Delay, Band ) ).r;
    #else
    Out = 0;
    #endif
}