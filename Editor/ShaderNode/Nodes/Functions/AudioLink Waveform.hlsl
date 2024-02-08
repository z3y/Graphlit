void AudioLink_Waveform(float2 UV, out float Out)
{
    float Sample = AudioLinkLerpMultiline( ALPASS_WAVEFORM + float2( 200. * UV.x, 0 ) ).r;
    Out = 1 - 50 * abs( Sample - UV.y * 2. + 1 );
}