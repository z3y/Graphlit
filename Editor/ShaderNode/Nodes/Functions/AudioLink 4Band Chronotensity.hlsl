void AudioLink_4BandChronotensity(float Band, float Mode, out float Out)
{
    #ifdef AUDIOLINK_CGINC_INCLUDED
    Out = (AudioLinkDecodeDataAsUInt( ALPASS_CHRONOTENSITY + int2(Mode, Band)) % 628319 ) / 100000.0;
    #else
    Out = 0;
    #endif
}