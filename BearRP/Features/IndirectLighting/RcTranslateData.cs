using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace BearRP.Features.IndirectLighting;

public class RcTranslateData {
    public Material RadianceCascadeMaterial;
    public TextureHandle Cascade0;
    public TextureHandle GIOutput;
    public Vector2 Cascade0Resolution;
    public Vector2 InternalResolution;
}