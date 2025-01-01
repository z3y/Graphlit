Shader "Hidden/Graphlit/PackingPreview"
{
    Properties
    {
        [NoScaleOffset] _Texture0 ("Texture", 2D) = "white" {}
        [Enum(Red, 0, Green, 1, Blue, 2, Alpha, 3)] _Texture0Channel ("Channel", Int) = 0
        [ToggleUI] _Texture0Invert ("Invert", Int) = 0
        
        [NoScaleOffset] _Texture1 ("Texture", 2D) = "white" {}
        [Enum(Red, 0, Green, 1, Blue, 2, Alpha, 3)] _Texture1Channel ("Channel", Int) = 0
        [ToggleUI] _Texture1Invert ("Invert", Int) = 0
        
        [NoScaleOffset] _Texture2 ("Texture", 2D) = "white" {}
        [Enum(Red, 0, Green, 1, Blue, 2, Alpha, 3)] _Texture2Channel ("Channel", Int) = 0
        [ToggleUI] _Texture2Invert ("Invert", Int) = 0
        
        [NoScaleOffset] _Texture3 ("Texture", 2D) = "white" {}
        [Enum(Red, 0, Green, 1, Blue, 2, Alpha, 3)] _Texture3Channel ("Channel", Int) = 0
        [ToggleUI] _Texture3Invert ("Invert", Int) = 0
        
        _ChannelPreview ("", Int) = 0
        _AlphaPreview ("", Int) = 0
    }
    SubShader
    {
        Pass
        {
            Cull Off ZWrite Off ZTest Always
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            
            SamplerState inlineSampler_bilinear_clamp_sampler;

            Texture2D _Texture0;
            Texture2D _Texture1;
            Texture2D _Texture2;
            Texture2D _Texture3;

            uint _Texture0Channel;
            uint _Texture1Channel;
            uint _Texture2Channel;
            uint _Texture3Channel;

            uint _ChannelPreview;

            bool _Texture0Invert;
            bool _Texture1Invert;
            bool _Texture2Invert;
            bool _Texture3Invert;

            float SampleChannel(Texture2D tex, uint channel, float2 uv)
            {
                float4 sampl = tex.SampleLevel(inlineSampler_bilinear_clamp_sampler, uv, 0);
                return sampl[channel];
            }

            float4 frag (v2f_img i) : SV_Target
            {
                float texture0 = SampleChannel(_Texture0, _Texture0Channel, i.uv);

                if (_Texture0Invert) texture0 = 1.0 - texture0;

                return float4(texture0, texture0, texture0, 1);
            }
            ENDCG
        }
    }
}