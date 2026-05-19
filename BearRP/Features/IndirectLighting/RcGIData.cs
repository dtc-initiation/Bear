using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace BearRP.Features.IndirectLighting;

public class RcGIData {
    public Material RadianceCascadeMaterial;
    
    public Vector2 InternalResolution;
    public Vector2 CascadeResolution;
    public Vector2 ProbeDensity;
    public int CascadeCount;
    public int CascadeIndex;
    public float CascadeIntervalLength;
    
    public TextureHandle Emission;
    public TextureHandle DistanceField;
    public TextureHandle CascadeN0;
    public TextureHandle CascadeN1;
    
    // Skybox
    public bool SkyBoxOn;
    public Color SunColor;
    public Color SkyColor;
    public float SunAngle;
    public float SunAngularRadius;
    public float SunIntensity;
}