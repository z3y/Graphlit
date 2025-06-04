void AudioLink_Exists(out bool Out)
{
    #ifdef AUDIOLINK_CGINC_INCLUDED
    Out = AudioLinkIsAvailable();
    #else
    Out = false;
    #endif
}