
#ifdef GRAPHLIT_OPTIMIZER_ENABLED
    #define GRAPHLIT_SAMPLE_TEXTURE2D(tex, smp, uv) SAMPLE_TEXTURE2D_SWITCH_##tex(smp, uv)
#else
    #define GRAPHLIT_SAMPLE_TEXTURE2D(tex, smp, uv) SAMPLE_TEXTURE2D(tex, smp, uv)
#endif