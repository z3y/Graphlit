void AudioLink_4BandFiltered(float Band, float FilterAmount, out float Out)
{
    #ifdef AUDIOLINK_CGINC_INCLUDED
    Out = AudioLinkLerp( ALPASS_FILTEREDAUDIOLINK + float2( FilterAmount, Band ) ).r;
    #else
    Out = 0;
    #endif
}