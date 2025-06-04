void AudioLink_Waveform(float2 UV, out float Out)
{
    #ifdef AUDIOLINK_CGINC_INCLUDED
    float Sample = AudioLinkLerpMultiline( ALPASS_WAVEFORM + float2( 200. * UV.x, 0 ) ).r;
    Out = 1 - 50 * abs( Sample - UV.y * 2. + 1 );
    #else
    Out = 0;
    #endif
}