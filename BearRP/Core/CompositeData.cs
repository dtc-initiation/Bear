using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace BearRP.Core;

public class CompositeData {
    public Material CompositeMaterial = null!;
    public TextureHandle Albedo;
    public TextureHandle DirectLighting;
    public TextureHandle IndirectLighting;
    public TextureHandle Emission;
    public TextureHandle Final;
}