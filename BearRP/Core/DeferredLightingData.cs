using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace BearRP.Core;

public class DeferredLightingData {
    public Vector2 InternalResolution;
    public Mesh BaseQuad = null!;
    public Material DeferredLightMaterial = null!;
    public MaterialPropertyBlock Mpb = null!;
    public ComputeBuffer? LightData = null!;
    public int InstanceCount;
    public int MaxLightCount;
    public TextureHandle NormalMap;
    public TextureHandle Shadowmap;
    public TextureHandle LightBuffer;
}