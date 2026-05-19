using UnityEngine.Rendering.RenderGraphModule;

namespace BearRP.Core;

public abstract class RenderFeature {
    public abstract void ValidateFeature(RenderGraph renderGraph, BearRPContext context);
    public abstract void BeginFeature(RenderGraph renderGraph, BearRPContext context);
    public virtual void EarlyRecord(RenderGraph renderGraph, BearRPContext context) {  }
    public virtual void Record(RenderGraph renderGraph, BearRPContext context) {  }
    public virtual void LateRecord(RenderGraph renderGraph, BearRPContext context) {  }
}