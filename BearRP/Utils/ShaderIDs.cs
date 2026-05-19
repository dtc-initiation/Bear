using UnityEngine;

namespace BearRP.Utils;

public static class ShaderIDs {
    // Common
    public static readonly int CameraInfo = GetID("_CameraInfo");
    public static readonly int InternalResolution = GetID("_InternalResolution");
    public static readonly int Tau = GetID("_Tau");
    public static readonly int Pi = GetID("_Pi");
    public static readonly int Epsilon = GetID("_Epsilon");
    
    // Direct Lighting related
    public static readonly int LightBuffer = GetID("_LightBuffer");
    public static readonly int LightCount = GetID("_LightCount");
    
    // GI Related
    public static readonly int Emission = GetID("_Emission");
    public static readonly int DistanceField = GetID("_DistanceField");
    
    // Naive Related
    public static readonly int JfaTOffset = GetID("_JfaTOffset");
    public static readonly int NaiveGIRayCount = GetID("_NaiveGIRayCount");
    public static readonly int NaiveGIMaxSteps = GetID("_NaiveGIMaxSteps");
    
    // Radiance Cascade Related
    public static readonly int CascadeResolution = GetID("_CascadeResolution");
    public static readonly int ProbeDensity = GetID("_ProbeDensity");
    public static readonly int CascadeCount = GetID("_CascadeCount");
    public static readonly int CascadeIndex = GetID("_CascadeIndex");
    public static readonly int CascadeIntervalLength = GetID("_CascadeIntervalLength");
    public static readonly int CascadeN0 = GetID("_CascadeN0");
    public static readonly int CascadeN1 = GetID("_CascadeN1");
    
    // Radiance Cascade Skybox Related
    public static readonly int SkyboxOn = GetID("_SkyboxOn");
    public static readonly int SkyColor = GetID("_SkyColor");
    public static readonly int SunColor = GetID("_SunColor");
    public static readonly int SunAngle = GetID("_SunAngle");
    public static readonly int SunAngularRadius = GetID("_SunAngularRadius");
    public static readonly int SunIntensity = GetID("_SunIntensity");
    private static int GetID(string shaderVariable) {
        return Shader.PropertyToID(shaderVariable);
    }
}