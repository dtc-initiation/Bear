using BearRP.DirectLighting;
using BearRP.Features;
using BearRP.Features.IndirectLighting;
using BearRP.GBuffer;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace BearRP.Core;

public class BearRPContext {
    public BearRPAsset? BearRPAsset;
    public RenderGraphParameters RGParams;
    
    public Camera Camera = null!;
    public BearCamera BearCamera = null!;
    public CameraData CameraData = null!;
    public CommandBuffer CommandBuffer = null!;

    public LightData LightData;
    public ShadowData ShadowData;
    public ScriptableRenderContext UnityContext;
    
    public GBufferTextures GBufferTextures;
    public DirectIlluminationTextures DiTextures;
    public GlobalIlluminationTextures GiTextures;  

    public TextureHandle OutputTexture;
    
    public void PassShadowData(LightData lightData, ShadowData shadowData) {
        LightData = lightData;
        ShadowData = shadowData;
    }
    
    public void Setup(ScriptableRenderContext unityContext, Camera camera) {
        UnityContext = unityContext;
        CommandBuffer = CommandBufferPool.Get();
        Camera = camera;
        BearCamera = camera.GetOrAddBearCamera();
        CameraData = camera.GetOrAddCameraData();
        SetupRenderGraph();
    }

    private void SetupRenderGraph() {
        RGParams = new RenderGraphParameters() {
            commandBuffer = CommandBuffer,
            scriptableRenderContext = UnityContext,
            currentFrameIndex = Time.frameCount
        };
    }

    public void CreateLightBuffer(RenderGraph renderGraph) {
        var shadowMapDesc = new ImportResourceParams() {
            clearOnFirstUse = false,
            discardOnLastUse = false
        };
        DiTextures.ShadowMap = renderGraph.ImportTexture(ShadowData._smTHInternal, shadowMapDesc);
        
        
        var lightBufferDesc = new TextureDesc(BearCamera.GetPixelWidth(), BearCamera.GetPixelHeight()) {
            name = "LightBuffer",
            clearBuffer = true,
            clearColor = Color.black,
            colorFormat = GraphicsFormat.R16G16B16A16_UNorm,
            filterMode = FilterMode.Point
        };
        DiTextures.LightBuffer = renderGraph.CreateTexture(lightBufferDesc);
    }
    
    public void CreateGBufferTextures(RenderGraph renderGraph) {
        var albedoDesc = new TextureDesc(BearCamera.GetPixelWidth(), BearCamera.GetPixelHeight()) {
            name = "GBuffer_Albedo",
            clearBuffer = true,
            clearColor = Color.black,
            colorFormat = GraphicsFormat.R16G16B16A16_UNorm,
            filterMode = FilterMode.Point
        };
        GBufferTextures.Albedo = renderGraph.CreateTexture(albedoDesc);

        var normalDesc = new TextureDesc(BearCamera.GetPixelWidth(), BearCamera.GetPixelHeight()) {
            name = "GBuffer_Normal",
            clearBuffer = true,
            clearColor = new Color(0.5f, 0.5f, 1f, 1f),
            colorFormat = GraphicsFormat.R16G16B16A16_UNorm,
            filterMode = FilterMode.Point
        };
        GBufferTextures.Normal = renderGraph.CreateTexture(normalDesc);

        var emissionDesc = new TextureDesc(BearCamera.GetPixelWidth(), BearCamera.GetPixelHeight()) {
            name = "GBuffer_Emission",
            clearBuffer = true,
            clearColor = new Color(0f, 0f, 0f, 0f),
            colorFormat = GraphicsFormat.R16G16B16A16_UNorm
        };
        GBufferTextures.Emission = renderGraph.CreateTexture(emissionDesc);
        
        var depthDesc = new TextureDesc(BearCamera.GetPixelWidth(), BearCamera.GetPixelHeight()) {
            name = "GBuffer_Depth",
            clearBuffer = true,
            colorFormat = GraphicsFormat.D24_UNorm_S8_UInt,
            filterMode = FilterMode.Point
        };
        GBufferTextures.Depth = renderGraph.CreateTexture(depthDesc);
    }
    
    public void CreateGITextures(RenderGraph renderGraph) {
        GiTextures.InputTexture = GBufferTextures.Emission;
        TextureDesc outputDesc = new TextureDesc(BearCamera.GetPixelWidth(), BearCamera.GetPixelHeight()) {
            name = "Gi Output",
            clearBuffer = true,
            clearColor = new Color(0f, 0f, 0f, 0f),
            colorFormat = GraphicsFormat.R16G16B16A16_SFloat,
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        GiTextures.OutputTexture = renderGraph.CreateTexture(outputDesc);
    }

    public void CreateOutputTextures(RenderGraph renderGraph) {
        TextureDesc outputDec = new TextureDesc(BearCamera.GetPixelWidth(), BearCamera.GetPixelHeight()) {
            name = "Output",
            clearBuffer = true,
            clearColor = new Color(0f, 0f, 0f, 1f),
            colorFormat = GraphicsFormat.R16G16B16A16_UNorm,
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        OutputTexture = renderGraph.CreateTexture(outputDec);
    }
    
    public void Dispose() {
    }
}