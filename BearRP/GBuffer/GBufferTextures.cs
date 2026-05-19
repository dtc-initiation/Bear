using UnityEngine.Rendering.RenderGraphModule;

namespace BearRP.GBuffer;

public struct GBufferTextures {
    public TextureHandle Albedo;
    public TextureHandle Normal;
    public TextureHandle Depth;
}