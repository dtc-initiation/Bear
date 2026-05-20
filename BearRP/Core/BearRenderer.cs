using System;
using BearRP.DirectLighting;
using BearRP.Features.IndirectLighting;
using BearRP.GBuffer;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

namespace BearRP.Core;

public class BearRenderer {
    private BearRPContext _context;
    private CameraSetupPass _cameraSetupPass;
    private DeferredLightingPass _deferredLightingPass;
    private GBufferPass _gBufferPass;
    
    private RenderFeature[] _renderFeatures;
    
    public BearRenderer() {
        _context = new BearRPContext();
        _cameraSetupPass = new CameraSetupPass();
        var deferredConfig = new DeferredLightingConfig(
            BearRP.RPAsset.deferredMaterial
        );
        _deferredLightingPass = new DeferredLightingPass(deferredConfig);
        _gBufferPass = new GBufferPass();
        
        
        SetupRenderFeatures();
    }

    private void SetupRenderFeatures() {
        // Indirect Lighting Render Feature
        var rcConfig = new RcGiConfig(
            BearRP.RPAsset.distanceFieldMaterial,
            BearRP.RPAsset.radianceCascadeMaterial,
            BearRP.RPAsset.numberOfCascades,
            BearRP.RPAsset.cascade0RayCount,
            BearRP.RPAsset.cascade0ProbeDensity,
            BearRP.RPAsset.cascade0Offset,
            BearRP.RPAsset.skyboxOn,
            BearRP.RPAsset.skyColor,
            BearRP.RPAsset.sunColor,
            BearRP.RPAsset.sunAngle,
            BearRP.RPAsset.sunAngularRadius,
            BearRP.RPAsset.sunIntensity
        );
        
        var naiveConfig = new NaiveGiConfig(
            BearRP.RPAsset.DistanceFieldMaterial,
            BearRP.RPAsset.NaiveMaterial,
            BearRP.RPAsset.naiveGIRayCount,
            BearRP.RPAsset.naiveMaxSteps
        );
        
        _renderFeatures = new RenderFeature[1] {
            new IndirectLightingRenderFeature(rcConfig, naiveConfig)
        };
    }

    public void PassShadowData(LightData lightData, ShadowData shadowData) {
        _context.PassShadowData(lightData, shadowData);
    }
    
    
    public void Render(RenderGraph renderGraph, ScriptableRenderContext unityContext, Camera camera) {
        unityContext.SetupCameraProperties(camera);

        try {
            bool contextReady = SetupContext(unityContext, camera);
            if (!contextReady) {
                return; 
            }
            
            renderGraph.BeginRecording(_context.RGParams);
            _context.CreateGBufferTextures(renderGraph);
            _context.CreateGITextures(renderGraph);
            _context.CreateLightBuffer(renderGraph);

            _cameraSetupPass.Record(renderGraph, _context);
            _gBufferPass.Record(renderGraph, _context);
            foreach (RenderFeature feature in _renderFeatures) {
                feature.ValidateFeature(renderGraph, _context);
                feature.BeginFeature(renderGraph, _context);
                feature.Record(renderGraph, _context);
            }

            AddDebugBlitPass(renderGraph, _context.GBufferTextures);

            renderGraph.EndRecordingAndExecute();
            unityContext.ExecuteCommandBuffer(_context.CommandBuffer);
            unityContext.Submit();

        } catch (Exception e) {
            if (renderGraph.ResetGraphAndLogException(e)) {
                throw;
            }
        } finally {
            CommandBufferPool.Release(_context.CommandBuffer);
            _context.CommandBuffer = null!;
        }
        
        
    }

    private bool SetupContext(ScriptableRenderContext unityContext, Camera camera) {
        _context.Setup(unityContext, camera);
        return true;
    }

    private bool AddGBufferPass(RenderGraph renderGraph, Camera camera) {
        using (var builder = renderGraph.AddRasterRenderPass<GBufferData>("GBuffer", out var passData)) {
            // Setting up data
            var textures = _context.GBufferTextures;
            builder.SetRenderAttachment(textures.Albedo, 0, AccessFlags.Write);
            builder.SetRenderAttachment(textures.Normal, 1, AccessFlags.Write);
            builder.SetRenderAttachmentDepth(textures.Depth, AccessFlags.Write);

            CameraData data = camera.GetOrAddCameraData();
            CullingResults cullingResults = data.CullingResults; 

            ShaderTagId tagId = new ShaderTagId("GBuffer");
            var sortingSettings = new SortingSettings(camera);
            var drawSettings = new DrawingSettings(tagId, sortingSettings);
            var filterSettings = FilteringSettings.defaultValue;

            var rendererListParams = new RendererListParams(cullingResults, drawSettings, filterSettings);
            passData.RendererList = renderGraph.CreateRendererList(rendererListParams);

            builder.UseRendererList(passData.RendererList);
            builder.SetRenderFunc<GBufferData>(GBufferAction);
        }
        return true;
    }

    private static void GBufferAction(GBufferData passData, RasterGraphContext context) {
        context.cmd.DrawRendererList(passData.RendererList);
    }
    
    private bool AddDebugBlitPass(RenderGraph renderGraph, GBufferTextures gBuffer) {
        TextureHandle outputValue;
        switch (BearRP.RPAsset.DebugOutput) {
            case (DebugOutputMode.Final):
                outputValue = gBuffer.Albedo;
                break;
            case (DebugOutputMode.Albedo):
                outputValue = gBuffer.Albedo;
                break;
            case (DebugOutputMode.Normal):
                outputValue = gBuffer.Normal;
                break;
            case (DebugOutputMode.Depth):
                outputValue = gBuffer.Depth;
                break;
            case (DebugOutputMode.Emissive):
                outputValue = _context.GiTextures.OutputTexture.IsValid() ? _context.GiTextures.OutputTexture : gBuffer.Albedo;
                break;
            default:
                outputValue = gBuffer.Albedo;
                break;
        }
        var importParams = new ImportResourceParams {
            clearOnFirstUse = true,
            clearColor = Color.black
        };
        var camTarget = _context.Camera.targetTexture;
        var backbufferDesc = new RenderTargetInfo {
            width = camTarget != null ? camTarget.width : Screen.width,
            height = camTarget != null ? camTarget.height : Screen.height,
            volumeDepth = 1,
            msaaSamples = 1,
            format = GraphicsFormat.R8G8B8A8_SRGB
        };
        var backbuffer = renderGraph.ImportBackbuffer(BuiltinRenderTextureType.CameraTarget, backbufferDesc, importParams);

        if (_context.Camera.cameraType != CameraType.Game) {
            var blitParams = new RenderGraphUtils.BlitMaterialParameters(outputValue, backbuffer, BearRP.RPAsset.BlitMaterial, 0);
            renderGraph.AddBlitPass(blitParams, "DebugBlit");
            return true;
        }
        
        using (var builder = renderGraph.AddRasterRenderPass<DebugBlitData>("DebugBlit", out var passData)) {
            passData.Source = outputValue;
            passData.Material = BearRP.RPAsset.BlitMaterial;
            passData.ViewPort = ComputeCenteredViewport(_context.Camera, _context.BearCamera);
            
            builder.UseTexture(passData.Source);
            builder.SetRenderAttachment(backbuffer, 0, AccessFlags.WriteAll);
            builder.SetRenderFunc<DebugBlitData>((data, rgContext) => {
                rgContext.cmd.SetViewport(data.ViewPort);
                Blitter.BlitTexture(rgContext.cmd, data.Source, new Vector4(1, 1, 0, 0), data.Material, 0);
            });
        }
        
        return true;
    }

    private Rect ComputeCenteredViewport(Camera camera, BearCamera bearCamera) {
        Rect rect = camera.pixelRect;
        if (bearCamera.TryGetScaledResolution(out int sw, out int sh)) {
            rect.x += (rect.width - sw) * 0.5f;
            rect.y += (rect.height - sh) * 0.5f;
            rect.width = sw;
            rect.height = sh;
        }

        if (rect.width % 2f != 0f) {
            rect.width++;
        }

        if (rect.height % 2f != 0f) {
            rect.height++;
        }
        return rect;
    }

    public void Dispose() {
        _context.Dispose();
    }
}