using UnityEngine.Rendering.RenderGraphModule;

namespace BearRP.Features.IndirectLighting;

public struct GlobalIlluminationTextures {
    public TextureHandle InputTexture;
    public TextureHandle OutputTexture;

    public TextureHandle JfaPing;
    public TextureHandle JfaPong;
}