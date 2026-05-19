using UnityEngine.Rendering.RenderGraphModule;

namespace BearRP.Core.Interface;

public interface IBearPass {
    void Record(RenderGraph renderGraph, BearRPContext context);
}