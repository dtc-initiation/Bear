using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace BearRP.Features.IndirectLighting;

public class JFAData {
    // Configs
    public int PassNumber;
    
    // Input
    public TextureHandle EmissionTexture;
    public Material JfaMaterial = null!;
    
    // Internal Variables
    public Vector2 InternalResolution;
    public int JfaTOffset;
    public int MaxSteps;
    public int RayCount;
    
    // Outputs
    public TextureHandle JfaInput;
    public TextureHandle JfaOutput;
    public TextureHandle DistanceField;
}