using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace BearRP.Features.IndirectLighting;

public class DiffuseData {
    public Material DiffuseMaterial = null!;
    public int NumPass;
    public int DiffuseOffset;
    public TextureHandle Albedo;
    public TextureHandle Occlusion;
    public TextureHandle Emission;
    public TextureHandle IndirectLighting;
    public TextureHandle DiffuseOutput;
}