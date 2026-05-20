using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace BearRP.DirectLighting;

public class ShadowMergeData {
    public int NumPass;
    public TextureHandle WideHandle;
    public Material ShadowMapMaterial = null!;
}