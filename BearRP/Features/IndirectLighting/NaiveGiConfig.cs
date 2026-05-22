using UnityEngine;

namespace BearRP.Features.IndirectLighting;

public readonly struct NaiveGiConfig {
    public readonly int RayCount;
    public readonly int MaxRaySteps;

    public NaiveGiConfig(
        int rayCount,
        int maxRaySteps
        ) {
        RayCount = rayCount;
        MaxRaySteps = maxRaySteps;
    }

}