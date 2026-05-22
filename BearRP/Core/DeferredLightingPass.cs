using BearRP.Core.Interface;
using BearRP.DirectLighting;
using BearRP.Utils;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace BearRP.Core;

public class DeferredLightingPass : IBearPass {
    private Material _deferredMaterial;
    private MaterialPropertyBlock _mpb;
    private readonly Mesh _quad;
    
    public DeferredLightingPass() {
        _quad = BearRPUtils.CreateQuad();
        _mpb = new MaterialPropertyBlock();
    }
    
    public void Record(RenderGraph renderGraph, BearRPContext context) {
        if (context.LightData.VisibleLights.Count == 0) {
            return;
        }
        BearRPUtils.GetOrLoadMaterial(ref _deferredMaterial, "Bear/Deferred");
        
        using (var builder = renderGraph.AddRasterRenderPass<DeferredLightingData>("Deferred Lighting Pass", out var passData)) {
            passData.BaseQuad = _quad;
            passData.DeferredLightMaterial = _deferredMaterial;
            passData.Mpb = _mpb;
            passData.LightData = context.LightData.LightBuffer;
            passData.InstanceCount = context.LightData.VisibleLights.Count;
            passData.MaxLightCount = context.ShadowData.MaxLightCount;
            passData.Shadowmap = context.DiTextures.ShadowMap;
            passData.LightBuffer = context.DiTextures.LightBuffer;

            builder.AllowPassCulling(false);
            builder.AllowGlobalStateModification(true);
            builder.UseTexture(passData.Shadowmap);
            builder.SetRenderAttachment(passData.LightBuffer, 0, AccessFlags.Write);    
            builder.SetRenderFunc<DeferredLightingData>(DeferredPass);
        }
    }

    private static void DeferredPass(DeferredLightingData data, RasterGraphContext context) {
        data.Mpb.SetBuffer(ShaderIDs.LightBuffer, data.LightData);
        data.Mpb.SetFloat(ShaderIDs.MaxLightCount, data.MaxLightCount);
        data.Mpb.SetFloat(ShaderIDs.LightCount, data.InstanceCount);
        data.Mpb.SetFloat(ShaderIDs.Tau, Mathf.PI * 2f);
        context.cmd.SetGlobalTexture(ShaderIDs.ShadowMap, data.Shadowmap);
        context.cmd.DrawMeshInstancedProcedural(data.BaseQuad, 0, data.DeferredLightMaterial, 0, data.InstanceCount, data.Mpb);
    }
}