using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace BearRP.Features.IndirectLighting;

public class JFAData {
    // Configs
    public int PassNumber;
    
    // Input
    public TextureHandle OcclusionTexture;
    public Material JfaMaterial = null!;
    
    // Internal Variables
    public int JfaTOffset;
    public int MaxSteps;
    public int RayCount;
    
    // Outputs
    public TextureHandle JfaInput;
    public TextureHandle JfaOutput;
    public TextureHandle DistanceField;
}