using UnityEngine.Rendering.RenderGraphModule;

namespace BearRP.Features.IndirectLighting;

public struct GlobalIlluminationTextures {
    public TextureHandle GiInput;
    public TextureHandle GiOutput;
}