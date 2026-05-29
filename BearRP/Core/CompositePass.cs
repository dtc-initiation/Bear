using BearRP.Core.Interface;
using BearRP.Utils;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace BearRP.Core;

public class CompositePass : IBearPass {
    private Material _compositeMaterial = null!;
    
    public void Record(RenderGraph renderGraph, BearRPContext context) {
        BearRPUtils.GetOrLoadMaterial(ref _compositeMaterial, "Bear/Composite");
        
        using (var builder = renderGraph.AddRasterRenderPass<CompositeData>("CompositePass", out var passData)) {
            passData.CompositeMaterial =  _compositeMaterial;
            passData.Albedo = context.GBufferTextures.Albedo;
            passData.DirectLighting = context.DiTextures.LightBuffer;
            passData.IndirectLighting = context.GiTextures.GiOutput;
            passData.Emission = context.GBufferTextures.Emission;
            passData.Output = context.OutputTexture;
            
            builder.AllowPassCulling(false);
            builder.AllowGlobalStateModification(true);
            builder.UseTexture(passData.Albedo);
            builder.UseTexture(passData.DirectLighting);
            builder.UseTexture(passData.IndirectLighting);
            builder.UseTexture(passData.Emission);
            builder.SetRenderAttachment(passData.Output, 0, AccessFlags.Write);
            builder.SetRenderFunc<CompositeData>(CompositePassInternal);
        }
    }

    private static void CompositePassInternal(CompositeData data, RasterGraphContext context) {
        context.cmd.SetGlobalTexture(ShaderIDs.DirectLighting, data.DirectLighting);
        context.cmd.SetGlobalTexture(ShaderIDs.IndirectLighting, data.IndirectLighting);
        context.cmd.SetGlobalTexture(ShaderIDs.EmissionMap, data.Emission);
        Blitter.BlitTexture(context.cmd, data.Albedo, new Vector4(1, 1, 0, 0), data.CompositeMaterial, 0);
    }
}