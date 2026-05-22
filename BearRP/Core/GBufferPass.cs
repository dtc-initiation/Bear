using BearRP.Core.Interface;
using BearRP.GBuffer;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace BearRP.Core;

public class GBufferPass : IBearPass {
    public void Record(RenderGraph renderGraph, BearRPContext context) {
        AddGBufferPass(renderGraph, context);
    }

    private void AddGBufferPass(RenderGraph renderGraph, BearRPContext context) {
        using (var builder = renderGraph.AddRasterRenderPass<GBufferData>("GBuffer", out var passData)) {
            var textures = context.GBufferTextures;
            builder.SetRenderAttachment(textures.Albedo, 0, AccessFlags.Write);
            builder.SetRenderAttachment(textures.Normal, 1, AccessFlags.Write);
            builder.SetRenderAttachment(textures.Emission, 2, AccessFlags.Write);
            builder.SetRenderAttachmentDepth(textures.Depth, AccessFlags.Write);

            CameraData cData = context.CameraData;
            CullingResults cullRes = cData.CullingResults;

            ShaderTagId tagId = new ShaderTagId("GBuffer");
            var sortingSettings = new SortingSettings(context.Camera);
            var drawSettings = new DrawingSettings(tagId, sortingSettings);
            var filterSettings = FilteringSettings.defaultValue;
            
            var rendererListParams = new RendererListParams(cullRes, drawSettings, filterSettings);
            passData.RendererList = renderGraph.CreateRendererList(rendererListParams);

            builder.UseRendererList(passData.RendererList);
            builder.SetRenderFunc<GBufferData>(GBufferPassInternal);
        }
    }
    
    private void GBufferPassInternal(GBufferData data, RasterGraphContext context) {
        context.cmd.DrawRendererList(data.RendererList);
    }
}