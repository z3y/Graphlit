void FinalColorNode(out half3 Color, out half Alpha, half3 diffuse = 1, half specular = 0, half3 emission = 0, half3 albedo = 1, half metallic = 0, half alpha = 1)
{
    Color = diffuse;
    #ifndef UNITY_PASS_SHADOWCASTER

        #if defined(_ALPHAPREMULTIPLY_ON)
            albedo *= alpha;
            alpha = lerp(alpha, 1.0, metallic);
        #endif

        #if defined(_ALPHAMODULATE_ON)
            albedo = lerp(1.0, albedo, alpha);
        #endif

        #if !defined(_ALPHAFADE_ON) && !defined(_ALPHATEST_ON) && !defined(_ALPHAPREMULTIPLY_ON) && !defined(_ALPHAMODULATE_ON)
            alpha = 1.0f;
        #endif

        Color = albedo * (1.0 - metallic) * diffuse;
        Color += specular;

        #if defined(UNITY_PASS_FORWARDBASE)
            Color += emission;
        #endif
    #endif
    Alpha = alpha;
}