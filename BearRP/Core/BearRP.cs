using System;
using System.Collections.Generic;
using System.Diagnostics;
using BearRP.DirectLighting;
using BearRP.Utils;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace BearRP.Core;

public class BearRP : RenderPipeline {
    public static BearRPAsset RPAsset { get; private set; } = null!;
    
    private readonly BearRenderer _renderer;
    private readonly RenderGraph _renderGraph;

    public static CullingResults? SharedCullingResults;
    public static Dictionary<Light, BearLight> BearLightLookup = new Dictionary<Light, BearLight>();
    public static Dictionary<Camera, BearCamera> BearCameraLookup = new Dictionary<Camera, BearCamera>();
    public static Dictionary<Camera, CameraData> CameraDataLookup = new Dictionary<Camera, CameraData>();

    public static float LastFrameMs = 0f;
    public static float SmoothedFps = 0f;
    private readonly Stopwatch _frameStopwatch = new Stopwatch();
    
    // Temporary for debug purposes only
    public static int SharedCullingLightCount = 0;
    public static Dictionary<Camera, int> CameraLightCount = new Dictionary<Camera, int>();
    public static int MeshVertexSize = 0;

    public readonly LightData LightData;
    public readonly ShadowData ShadowData;
    
    public BearRP(BearRPAsset rpAsset) {
        RPAsset = rpAsset;
        _renderGraph = new RenderGraph();
        _renderer = new BearRenderer();
        LightData = new LightData(RPAsset.maxNumberOfLights);
        ShadowData = new ShadowData(RPAsset.maxNumberOfLights, (int) RPAsset.angularResolution);
    }

    protected override void Render(ScriptableRenderContext context, List<Camera> cameras) {
        RenderGraphParameters rgParams = new  RenderGraphParameters() {
            commandBuffer = CommandBufferPool.Get(),
            scriptableRenderContext = context,
            currentFrameIndex = Time.frameCount
        };
        
        BeginFrame(context);
        SetupAndCull(context, cameras);
        
        // Setup lighting Data 
        LightData.RegisterVisibleLights(SharedCullingResults, cameras);
        ShadowData.ReleaseDeadLights();
        ShadowData.AssignLightIds(LightData.VisibleLights);
        LightData.PopulateLight(ShadowData.LightIdLookup);
        
        // Draw onto shadowmap
        ShadowData.UpdateShadowMesh();
        
        try {
            _renderGraph.BeginRecording(rgParams);
            
            SetupShaderGlobals();
            ShadowData.RecordShadowMapPass(_renderGraph, context, LightData);
            
            _renderGraph.EndRecordingAndExecute();
            context.ExecuteCommandBuffer(rgParams.commandBuffer);
            context.Submit();
        }
        catch (Exception e) {
            if (_renderGraph.ResetGraphAndLogException(e)) {
                throw;
            }
        }
        finally {
            CommandBufferPool.Release(rgParams.commandBuffer);
        }
        _renderer.PassShadowData(LightData, ShadowData);
        
        foreach (Camera camera in cameras) {
            BeginCameraRendering(context, camera);
            _renderer.Render(_renderGraph, context, camera); 
            EndCameraRendering(context, camera);
        }
        EndFrame();
    }

    private void BeginFrame(ScriptableRenderContext context) {
        SharedCullingResults = null;
        
        // Debug Purposes only
        SharedCullingLightCount = 0;
        CameraLightCount.Clear();
    }

    private void SetupShaderGlobals() {
        using (var builder = _renderGraph.AddUnsafePass<BearGlobals>("GlobalConstants", out var passData)) {
            
            builder.AllowGlobalStateModification(true);
            builder.AllowPassCulling(false);
            builder.SetRenderFunc<BearGlobals>((data, context) => {
                context.cmd.SetGlobalFloat(ShaderIDs.Tau, Mathf.PI * 2);
                context.cmd.SetGlobalFloat(ShaderIDs.Pi, Mathf.PI);
                context.cmd.SetGlobalFloat(ShaderIDs.Epsilon, 0.001f);
            });
        }
    }

    private void SetupAndCull(ScriptableRenderContext context, List<Camera> cameras) {
        bool mainExists = cameras.Contains(Camera.main);
        bool shareCull = RPAsset.UseSharedCulling && mainExists;
        if (shareCull) {
            if (Camera.main.TryGetCullingParameters(out ScriptableCullingParameters cullingParams)) {
                SharedCullingResults = context.Cull(ref cullingParams);
                
                // Debug Purposes only
                SharedCullingLightCount = SharedCullingResults.Value.visibleLights.Length;
            }
        }

        foreach (Camera camera in cameras) {
            camera.SetupAndCull(context);
        }
    }

    private void EndFrame() {
    }
    
    protected override void Dispose(bool disposing) {
        _renderGraph.Cleanup();
        _renderer.Dispose();
        BearCameraLookup.Clear();
        CameraDataLookup.Clear();

        ShadowData.Dispose();
        LightData.Dispose();
        
        // Debug purposes only
        CameraLightCount.Clear();
    }
}