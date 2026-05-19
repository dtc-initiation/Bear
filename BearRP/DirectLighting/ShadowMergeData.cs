using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace BearRP.DirectLighting;

public class ShadowMergeData {
    public TextureHandle ShadowMapHandle;
    public Material ShadowMapMaterial = null!;
}