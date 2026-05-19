using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace BearRP.GBuffer;

public class GBufferData {
    public TextureHandle GBuffer0_Albedo;
    public TextureHandle GBuffer1_Normal;
    public TextureHandle GBuffer2_Depth;
    public RendererListHandle RendererList;
}