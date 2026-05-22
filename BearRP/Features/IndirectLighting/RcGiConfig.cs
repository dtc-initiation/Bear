using Unity.Profiling;
using UnityEngine;

namespace BearRP.Features.IndirectLighting;

public readonly struct RcGiConfig {
    public readonly int NumberOfCascades;
    public readonly int Cascade0RayCount;
    public readonly float Cascade0Range;
    public readonly float Cascade0ProbeDensity;
    
    // Skybox Related
    public readonly bool SkyBoxOn;
    public readonly Color SkyColor;
    public readonly Color SunColor;
    public readonly float SunAngle;
    public readonly float SunAngularRadius;
    public readonly float SunIntensity;
    
    public RcGiConfig(
        int numberOfCascades,
        int cascade0RayCount,
        float cascade0ProbeDensity,
        float cascade0Range,
        bool skyboxOn,
        Color skyColor,
        Color sunColor,
        float sunAngle,
        float sunAngularRadius,
        float sunIntensity
        ) {
        NumberOfCascades = numberOfCascades;
        Cascade0RayCount = cascade0RayCount;
        Cascade0ProbeDensity = cascade0ProbeDensity;
        Cascade0Range = cascade0Range;
        SkyBoxOn = skyboxOn;
        SkyColor = skyColor;
        SunColor = sunColor;
        SunAngle = sunAngle;
        SunAngularRadius = sunAngularRadius;
        SunIntensity = sunIntensity;
    }
}