using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace BearRP.Core;

public class DeferredLightingData {
    public Mesh BaseQuad = null!;
    public Material DeferredLightMaterial = null!;
    public MaterialPropertyBlock Mpb = null!;
    public ComputeBuffer? LightData = null!;
    public int InstanceCount;
    public TextureHandle Shadowmap;
    public TextureHandle LightBuffer;
}