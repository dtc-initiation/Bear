using UnityEngine;

namespace BearRP.Features.IndirectLighting;

public readonly struct NaiveGiConfig {
    public readonly Material DistanceFieldMaterial;
    public readonly Material NaiveGiMaterial;
    public readonly int RayCount;
    public readonly int MaxRaySteps;

    public NaiveGiConfig(
        Material distanceFieldMaterial,
        Material naiveGiMaterial,
        int rayCount,
        int maxRaySteps
        ) {
        DistanceFieldMaterial = distanceFieldMaterial;
        NaiveGiMaterial = naiveGiMaterial;
        RayCount = rayCount;
        MaxRaySteps = maxRaySteps;
    }

}