using UnityEngine.Rendering.RenderGraphModule;

namespace BearRP.Features.IndirectLighting;

public struct GlobalIlluminationTextures {
    public TextureHandle Occlusion;
    public TextureHandle GiInput;
    public TextureHandle GiOutput;

    public TextureHandle JfaPing;
    public TextureHandle JfaPong;
}