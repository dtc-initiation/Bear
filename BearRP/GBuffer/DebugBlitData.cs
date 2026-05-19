using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace BearRP.GBuffer;

public class DebugBlitData {
    public TextureHandle Source;
    public Material Material = null!;
    public Rect ViewPort;
}