struct SurfaceDescription
{
    static SurfaceDescription New()
    {
        SurfaceDescription surfaceDescription;
        
        surfaceDescription.Albedo = 1.0;
        surfaceDescription.Normal = float3(0,0,1);
        surfaceDescription.Metallic = 0.0;
        surfaceDescription.Emission = float3(0.0, 0.0, 0.0);
        surfaceDescription.Roughness = 0.5;
        surfaceDescription.Occlusion = 1.0;
        surfaceDescription.Alpha = 1.0;
        surfaceDescription.AlphaClipThreshold = 0.5;
        surfaceDescription.Reflectance = 0.5;

        surfaceDescription.GSAAVariance = 0.15;
        surfaceDescription.GSAAThreshold = 0.1;

        surfaceDescription.Anisotropy = 0.0;
        surfaceDescription.Tangent = float3(1,1,1);
        surfaceDescription.SpecularOcclusion = 1.0;

        return surfaceDescription;
    }

    half3 Albedo;
    float3 Normal;
    half Metallic;
    half3 Emission;
    half Roughness;
    half Occlusion;
    half Alpha;
    half AlphaClipThreshold;
    half Reflectance;
    half GSAAVariance;
    half GSAAThreshold;
    half3 Tangent;
    half Anisotropy;
    half SpecularOcclusion;
};
