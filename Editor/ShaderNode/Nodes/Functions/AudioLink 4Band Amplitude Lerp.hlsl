void AudioLink_4BandAmplitudeLerp(float Band, float Delay, out float Out)
{
    #ifdef AUDIOLINK_CGINC_INCLUDED
    Out = AudioLinkLerp( ALPASS_AUDIOLINK + float2( Delay, Band ) ).r;
    #else
    Out = 0;
    #endif
}