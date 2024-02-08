void AudioLink_4BandChronotensity(float Band, float Mode, out float Out)
{
    Out = (AudioLinkDecodeDataAsUInt( ALPASS_CHRONOTENSITY + int2(Mode, Band)) % 628319 ) / 100000.0;
}