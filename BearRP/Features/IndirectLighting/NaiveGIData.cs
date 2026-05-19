using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace BearRP.Features.IndirectLighting;

public class NaiveGIData {
    // INput
    public Material NaiveGIMaterial = null!;
    public TextureHandle EmissionInput;
    public TextureHandle DistanceField;
    
    // Internal Variables
    public Vector2 InternalResolution;
    public int RayCount;
    public int MaxSteps;
    
    // Output
    public TextureHandle EmissionOutput;
}