void AudioLink_4BandFiltered(float Band, float FilterAmount, out float Out)
{
    Out = AudioLinkLerp( ALPASS_FILTEREDAUDIOLINK + float2( FilterAmount, Band ) ).r;
}