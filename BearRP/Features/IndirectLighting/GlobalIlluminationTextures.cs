using UnityEngine.Rendering.RenderGraphModule;

namespace BearRP.Features.IndirectLighting;

public struct GlobalIlluminationTextures {
    public TextureHandle OcclusionTexture;
    public TextureHandle InputTexture;
    public TextureHandle OutputTexture;
    public TextureHandle DiffuseOutputTexture;

    public TextureHandle JfaPing;
    public TextureHandle JfaPong;
}