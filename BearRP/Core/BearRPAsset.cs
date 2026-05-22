using UnityEngine;
using UnityEngine.Rendering;

namespace BearRP.Core;

[CreateAssetMenu(fileName = "BearRPAsset", menuName = "Rendering/BearRPAsset")]
public class BearRPAsset : RenderPipelineAsset<BearRP> {
    [Header("DebugOutput")]
    [SerializeField] private DebugOutputMode debugOutputMode = DebugOutputMode.Final;

    [Header("Culling")] 
    [SerializeField] private bool useSharedCulling = true;
    
    [Header("Materials")]
    [SerializeField] private Material blitMaterial = null!;

    [Header("Direct Lighting Settings")]
    [SerializeField] public int angularResolution = 1024;
    
    [Header("Global Illumination Settings")]
    [SerializeField] public GIGatherMethod gatherMethod = GIGatherMethod.Naive;
    
    [Header("Naive Illumination Settings")]
    [SerializeField] public int naiveGIRayCount = 8;
    [SerializeField] public int naiveMaxSteps = 32;
    
    [Header("Radiance Cascade Settings")]
    [SerializeField, Range(3, 6)] public int numberOfCascades = 4;
    [SerializeField] public int cascade0RayCount = 4;
    [SerializeField] public float cascade0ProbeDensity = 1f;
    [SerializeField, Range(0, 100)] public float cascade0Offset = 1f;

    [Header("Radiance Cascade SkyBox")] 
    [SerializeField] public bool skyboxOn = false;
    [SerializeField] public Color skyColor = Color.lightSkyBlue;
    [SerializeField] public Color sunColor = new Color(1f, 1f, 0.86f, 1f);
    [SerializeField, Range(0, Mathf.PI)] public float sunAngle = Mathf.PI / 2f;
    [SerializeField, Range(0, 1f)] public float sunAngularRadius = 0.15f;
    [SerializeField, Range(0, 10f)] public float sunIntensity = 1f;
    
    public DebugOutputMode DebugOutput => debugOutputMode;
    public bool UseSharedCulling => useSharedCulling;
    public Material BlitMaterial => blitMaterial;
    
    protected override RenderPipeline CreatePipeline() {
        return new BearRP(this);
    }
}