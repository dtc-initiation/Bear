using System;
using System.Collections.Generic;
using BearRP.Utils;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace BearRP.DirectLighting;

public class ShadowData {
    private readonly Stack<int> _openIds;
    public readonly Dictionary<Light, int> LightIdLookup;
    private readonly HashSet<Light> _deadLights;
    private Mesh _blockerMesh;
    
    // Textures and Handles
    // Initial Pass Texture and Material
    private Material _shadowMapMaterial;
    private RenderTexture _smWide;
    private RTHandle _smWideTHInternal;
    private TextureHandle _smWideTH;

    private RenderTexture _sm;
    private RTHandle _smTHInternal;
    public TextureHandle _smTH;
    
    // Empty MPB for instanced draw call
    private MaterialPropertyBlock _mpb;
    
    public ShadowData(Material shadowMapMaterial, int maximumLightCount, int angularResolution) {
        _shadowMapMaterial = shadowMapMaterial;
        _openIds = new Stack<int>();
        LightIdLookup = new Dictionary<Light, int>();
        _deadLights = new HashSet<Light>();
            
        _smWide = new RenderTexture(
            (int)(angularResolution * 1.5f),
            maximumLightCount, 
            16,
            RenderTextureFormat.Depth
        );
        _smWide.Create();
        _smWideTHInternal = RTHandles.Alloc(_smWide);

        _sm = new RenderTexture(
            angularResolution,
            maximumLightCount,
            16,
            RenderTextureFormat.Depth
        );
        _sm.Create();
        _smTHInternal = RTHandles.Alloc(_smTH);
        
        _mpb = new MaterialPropertyBlock();
    }

    public void RecordShadowMapPass(RenderGraph renderGraph, ScriptableRenderContext context, LightData lightData) {
        if (lightData.VisibleLights.Count == 0) {
            return;
        }
        
        RenderGraphParameters rgParams = new  RenderGraphParameters() {
            commandBuffer = CommandBufferPool.Get(),
            scriptableRenderContext = context,
            currentFrameIndex = Time.frameCount
        };
        try {
            renderGraph.BeginRecording(rgParams);
            var wideImportParams = new ImportResourceParams() {
                clearOnFirstUse = true
            };
            _smWideTH = renderGraph.ImportTexture(_smWideTHInternal, wideImportParams);
            
            using (var builder = renderGraph.AddRasterRenderPass<ShadowmapData>("Shadowmap Pass", out var passData)) {
                passData.NumPass = 0;
                passData.ShadowMesh = _blockerMesh;
                passData.ShadowmapMaterial = _shadowMapMaterial;
                passData.LightBuffer = lightData.LightBuffer;
                passData.InstanceCount = lightData.VisibleLights.Count;
                passData.Mpb = _mpb;

                builder.AllowPassCulling(false);
                builder.AllowGlobalStateModification(true);
                builder.SetRenderAttachmentDepth(_smWideTH, AccessFlags.Write);
                builder.SetRenderFunc<ShadowmapData>(ShadowmapPass);
            }

            var importParams = new ImportResourceParams() {
                clearOnFirstUse = true
            };
            _smTH = renderGraph.ImportTexture(_smTHInternal, importParams);

            using (var builder = renderGraph.AddRasterRenderPass<ShadowMergeData>("Shadowmap Merge Pass", out var passData)) {
                passData.NumPass = 1;
                passData.ShadowMapMaterial = _shadowMapMaterial;
                passData.WideHandle = _smWideTH;
                
                builder.AllowPassCulling(false);
                builder.AllowGlobalStateModification(true);
                builder.SetRenderAttachmentDepth(_smTH, AccessFlags.Write);
                builder.SetRenderFunc<ShadowMergeData>(ShadowMergePass);
            }
            
            
            renderGraph.EndRecordingAndExecute();
            context.ExecuteCommandBuffer(rgParams.commandBuffer);
            context.Submit();
            
        } catch (Exception e) {
            if (renderGraph.ResetGraphAndLogException(e)) {
                throw;
            }
        } finally {
            CommandBufferPool.Release(rgParams.commandBuffer);
        }
    }

    private static void ShadowmapPass(ShadowmapData data, RasterGraphContext context) {
        context.cmd.SetGlobalFloat(ShaderIDs.Tau, Mathf.PI * 2f);
        context.cmd.SetGlobalFloat(ShaderIDs.Pi, Mathf.PI);
        data.Mpb.SetBuffer(ShaderIDs.LightBuffer, data.LightBuffer);
        data.Mpb.SetFloat(ShaderIDs.LightCount, data.InstanceCount);
        
        context.cmd.DrawMeshInstancedProcedural(data.ShadowMesh, 0, data.ShadowmapMaterial, data.NumPass, data.InstanceCount, data.Mpb);
    }

    private static void ShadowMergePass(ShadowMergeData data, RasterGraphContext context) {
        Blitter.BlitTexture(context.cmd, data.WideHandle, new Vector4(1, 1, 0, 0), data.ShadowMapMaterial, data.NumPass);
    }

    public void AssignLightIds(HashSet<Light> visibleLights) {
        foreach (Light light in visibleLights) {
            int id = GetOrAssignId(light);
            if (id == -1) {
                break;
            }
        }
    }
    
    public int GetOrAssignId(Light light) {
        int id;
        if (LightIdLookup.TryGetValue(light, out id)) {
            return id;
        }

        if (_openIds.Count <= 0) {
            return -1;
        }
        
        id = _openIds.Pop();
        LightIdLookup.Add(light, id);
        return id;
    }
    
    public void ReleaseDeadLights() {
        foreach (var kvp in LightIdLookup) {
            if (kvp.Key == null || kvp.Key.Equals(null) || !kvp.Key.isActiveAndEnabled) {
                _deadLights.Add(kvp.Key);
            }
        }

        foreach (Light deadLight in _deadLights) {
            _openIds.Push(LightIdLookup[deadLight]);
            LightIdLookup.Remove(deadLight);
        }
        _deadLights.Clear();
    }
    
    public void UpdateShadowMesh() {
        var blockers = GameObject.FindObjectsByType<EdgeCollider2D>(FindObjectsInactive.Exclude);
        _blockerMesh = BuildShadowMesh(blockers);
    }
    
    private Mesh BuildShadowMesh(IReadOnlyList<EdgeCollider2D> colliders) {
        float startingTime = Time.time;
        int totalEdges = 0;
        for (int j = 0; j < colliders.Count; j++) {
            totalEdges += colliders[j].edgeCount;
        }
        int vertCount = totalEdges * 2;
        var positions = new Vector3[vertCount];
        var partners = new Vector2[vertCount];
        var indices = new int[vertCount];

        int vertexIdx = 0;
        foreach (EdgeCollider2D collider in colliders) {
            var points = collider.points;
            var transform = collider.transform;
            
            for (int i = 0; i < collider.points.Length - 1; i++) {
                Vector2 pointA = transform.TransformPoint(points[i]);
                Vector2 pointB = transform.TransformPoint(points[i + 1]);
                
                positions[vertexIdx] = pointA;
                partners[vertexIdx] = pointB;
                indices[vertexIdx] = vertexIdx;
                
                positions[vertexIdx + 1] = pointB;
                partners[vertexIdx + 1] = pointA;
                indices[vertexIdx + 1] = vertexIdx + 1;

                vertexIdx += 2;
            }
        }

        var mesh = new Mesh {
            name = "ShadowCasters",
            indexFormat = IndexFormat.UInt16,
        };
        mesh.SetVertices(positions);
        mesh.SetUVs(0, partners);
        mesh.SetIndices(indices, MeshTopology.Lines, 0);
        
        float FinishedTime = Time.time;
        // Debug.Log($"Time Taken : {FinishedTime - startingTime}");
        return mesh;
    }

    public void Dispose() {
        _smWideTHInternal.Release();
        _smTHInternal.Release();
    }
}