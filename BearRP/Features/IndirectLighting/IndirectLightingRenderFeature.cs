using BearRP.Core;
using BearRP.Utils;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

namespace BearRP.Features.IndirectLighting;

public class IndirectLightingRenderFeature : RenderFeature {
    // Materials
    private Material _distanceFieldMaterial = null!;
    private Material _naiveMaterial = null!;
    private Material _cascadeMaterial = null!;
    
    // Configs
    private readonly RcGiConfig _rcGiConfig;
    private readonly NaiveGiConfig _naiveGiConfig;
    
    // Textures
    private readonly TextureHandle[] _cascadeTh;
    private TextureHandle _jfaPingTh;
    private TextureHandle _jfaPongTh;
    private TextureHandle _diffusePing;
    private TextureHandle _diffusePong;
    
    // Performance metrics
    private static readonly ProfilingSampler s_JfaSeed      = new("GI.JfaSeed");
    private static readonly ProfilingSampler s_JfaPingPong  = new("GI.JfaPingPong");
    private static readonly ProfilingSampler s_DistanceField= new("GI.DistanceField");
    private static readonly ProfilingSampler s_NaiveGather  = new("GI.NaiveGather");
    private static readonly ProfilingSampler s_RcGather     = new("GI.RcGather");
    private static readonly ProfilingSampler s_RcMip        = new("GI.RcMip");
    private static readonly ProfilingSampler s_RcTranslate  = new("GI.RcTranslate");
    private static readonly ProfilingSampler s_RcDiffuse    = new("GI.Diffuse");

    public IndirectLightingRenderFeature(RcGiConfig rcGiConfig, NaiveGiConfig naiveGiConfig) {
        _rcGiConfig = rcGiConfig;
        _naiveGiConfig = naiveGiConfig;
        _cascadeTh = new TextureHandle[_rcGiConfig.NumberOfCascades];
    }
    
    public override void ValidateFeature(RenderGraph renderGraph, BearRPContext context) {
        
    }

    public override void BeginFeature(RenderGraph renderGraph, BearRPContext context) {
        switch (Core.BearRP.RPAsset.gatherMethod) {
            case (GIGatherMethod.Naive):
                CreateDistanceFieldTextures(renderGraph, context);
                break;
            case (GIGatherMethod.RadianceCascade):
                CreateDistanceFieldTextures(renderGraph, context);
                CreateRadianceCascades(renderGraph, context);
                break;
        }
        
    }

    private void CreateDistanceFieldTextures(RenderGraph renderGraph, BearRPContext context) {
        var inputDesc = context.GiTextures.GiInput.GetDescriptor(renderGraph);
        var jfaPingDesc = new TextureDesc(inputDesc.width, inputDesc.height) {
            name = "jfaPing",
            clearBuffer = true,
            clearColor = new Color(0f, 0f, 0f, 0f),
            colorFormat = GraphicsFormat.R32G32_SFloat,
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        var jfaPongDesc = new TextureDesc(inputDesc.width, inputDesc.height) {
            name = "jfaPong",
            clearBuffer = true,
            clearColor = new Color(0f, 0f, 0f, 0f),
            colorFormat = GraphicsFormat.R32G32_SFloat,
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        _jfaPingTh = renderGraph.CreateTexture(jfaPingDesc);
        _jfaPongTh = renderGraph.CreateTexture(jfaPongDesc);
    }

    private void CreateRadianceCascades(RenderGraph renderGraph, BearRPContext context) {
        for (int cascadeNum = 0; cascadeNum < _rcGiConfig.NumberOfCascades; cascadeNum++) {
            int rayCount = (int) Mathf.Pow(2.0f, cascadeNum);
            var probeCount = Mathf.Pow(0.5f, cascadeNum) * context.BearCamera.GetPixelResolution() / _rcGiConfig.Cascade0ProbeDensity;
            var cascadeRes = probeCount * rayCount;
            var desc = new TextureDesc((int) cascadeRes.x, (int) cascadeRes.y) {
                name = $"Radiance Cascade {cascadeNum}",
                clearBuffer = true,
                clearColor = new Color(0f, 0f, 0f, 0f),
                colorFormat = GraphicsFormat.R16G16B16A16_SFloat,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                useMipMap = (cascadeNum == 0)
            };
            _cascadeTh[cascadeNum] = renderGraph.CreateTexture(desc);
        }
        
        TextureDesc diffusePingDesc = new TextureDesc(context.BearCamera.GetPixelWidth(), context.BearCamera.GetPixelHeight()) {
            name = "Gi_Diffuse_Ping",
            clearBuffer = true,
            clearColor = new Color(0f, 0f, 0f, 0f),
            colorFormat = GraphicsFormat.R16G16B16A16_UNorm,
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        TextureDesc diffusePongDesc = new TextureDesc(context.BearCamera.GetPixelWidth(), context.BearCamera.GetPixelHeight()) {
            name = "Gi_Diffuse_Pong",
            clearBuffer = true,
            clearColor = new Color(0f, 0f, 0f, 0f),
            colorFormat = GraphicsFormat.R16G16B16A16_UNorm,
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        _diffusePing = renderGraph.CreateTexture(diffusePingDesc);
        _diffusePong = renderGraph.CreateTexture(diffusePongDesc);
    }

    public override void Record(RenderGraph renderGraph, BearRPContext context) {
        switch (Core.BearRP.RPAsset.gatherMethod) {
            case (GIGatherMethod.Naive):
                RecordNaivePass(renderGraph, context);
                break;
            case (GIGatherMethod.RadianceCascade):
                RecordRadianceCascades(renderGraph, context);
                break;
        }
    }

    #region JFA
    private void AddSeedPass(RenderGraph renderGraph, BearRPContext context) {
        BearRPUtils.GetOrLoadMaterial(ref _distanceFieldMaterial, "Bear/DistanceField");
        
        using (var builder = renderGraph.AddRasterRenderPass<JFAData>("JFA Seed", out var passData, s_JfaSeed)) {
            passData.PassNumber = 0;
            passData.OcclusionTexture = context.GBufferTextures.Occlusion;
            passData.JfaMaterial = _distanceFieldMaterial;
            passData.JfaInput = _jfaPingTh;
            
            builder.UseTexture(passData.OcclusionTexture);
            builder.SetRenderAttachment(passData.JfaInput, 0, AccessFlags.Write);
            builder.AllowGlobalStateModification(true);
            builder.AllowPassCulling(false);
            builder.SetRenderFunc<JFAData>(PopulateJfaSeed);
        }
    }

    private static void PopulateJfaSeed(JFAData passData, RasterGraphContext context) {
        Blitter.BlitTexture(context.cmd, passData.OcclusionTexture, new Vector4(1, 1, 0, 0), passData.JfaMaterial, passData.PassNumber);
    }

    private (TextureHandle, TextureHandle) AddPingPongPass(RenderGraph renderGraph, BearRPContext context) {
        TextureHandle jfaInput = _jfaPingTh;
        TextureHandle jfaOutput = _jfaPongTh;
        var inputDesc = jfaInput.GetDescriptor(renderGraph);
        int dimensions = Mathf.Max(inputDesc.width, inputDesc.height);
        int numPass = Mathf.CeilToInt(Mathf.Log(dimensions, 2f));
        for (int i = 0; i < numPass; i++) {
            using (var builder = renderGraph.AddRasterRenderPass<JFAData>($"JFA Pass {i}", out var passData, s_JfaPingPong)) {
                passData.PassNumber = 1;
                passData.JfaMaterial = _distanceFieldMaterial;
                passData.JfaInput = jfaInput;
                passData.JfaOutput = jfaOutput;
                
                passData.JfaTOffset = (int) Mathf.Pow(2, numPass - i - 1);
                passData.RayCount = _naiveGiConfig.RayCount;
                passData.MaxSteps = _naiveGiConfig.MaxRaySteps;
                
                builder.UseTexture(passData.JfaInput);
                builder.SetRenderAttachment(passData.JfaOutput, 0,  AccessFlags.Write);
                builder.AllowGlobalStateModification(true);
                builder.AllowPassCulling(false);
                builder.SetRenderFunc<JFAData>(JumpFloodFill);
            }
            (jfaInput, jfaOutput) = (jfaOutput, jfaInput);
        }
        return (jfaInput, jfaOutput);
    }
    
    private static void JumpFloodFill(JFAData passData, RasterGraphContext context) {
        context.cmd.SetGlobalInt(ShaderIDs.JfaTOffset, passData.JfaTOffset);
        context.cmd.SetGlobalInt(ShaderIDs.NaiveGIMaxSteps, passData.MaxSteps);
        context.cmd.SetGlobalInt(ShaderIDs.NaiveGIRayCount, passData.RayCount);
        Blitter.BlitTexture(context.cmd, passData.JfaInput, new Vector4(1, 1, 0, 0), passData.JfaMaterial, passData.PassNumber);
    }

    private TextureHandle AddDistanceFieldPass(RenderGraph renderGraph, BearRPContext context, TextureHandle jfaOutputTexture, TextureHandle dfOutputTexture) {
        using (var builder = renderGraph.AddRasterRenderPass<JFAData>("Distance Field", out var passData, s_DistanceField)) {
            passData.PassNumber = 2;
            passData.JfaMaterial = _distanceFieldMaterial;
            passData.JfaOutput = jfaOutputTexture;
            passData.DistanceField = dfOutputTexture;
            
            builder.UseTexture(passData.JfaOutput);
            builder.SetRenderAttachment(dfOutputTexture, 0, AccessFlags.Write);
            builder.AllowGlobalStateModification(true);
            builder.AllowPassCulling(false);
            builder.SetGlobalTextureAfterPass(in passData.DistanceField, ShaderIDs.DistanceField);
            builder.SetRenderFunc<JFAData>(ComputeDistanceField);
        }
        return dfOutputTexture;
    }

    private static void ComputeDistanceField(JFAData passData, RasterGraphContext context) {
        Blitter.BlitTexture(context.cmd, passData.JfaOutput, new Vector4(1, 1, 0, 0), passData.JfaMaterial, passData.PassNumber);
    }
    

    #endregion

    #region Naive
    // Naive Global Illumination
    private void RecordNaivePass(RenderGraph renderGraph, BearRPContext context) {
        AddSeedPass(renderGraph, context);
        var (jfaFinal, jfaScratch) = AddPingPongPass(renderGraph, context);
        var distanceField = AddDistanceFieldPass(renderGraph, context, jfaFinal, jfaScratch);
        AddNaiveRadiancePass(renderGraph, context, distanceField);
        
    }
    private void AddNaiveRadiancePass(RenderGraph renderGraph, BearRPContext context, TextureHandle dfOutputTexture) {
        BearRPUtils.GetOrLoadMaterial(ref _naiveMaterial, "Bear/NaiveGI");
        
        // The output pass
        using (var builder = renderGraph.AddRasterRenderPass<NaiveGIData>("Naive Gathering Pass", out var passData, s_NaiveGather)) {
            passData.NaiveGIMaterial = _naiveMaterial;
            passData.EmissionInput = context.GiTextures.GiInput;
            passData.DistanceField = dfOutputTexture;
            passData.EmissionOutput = context.GiTextures.GiOutput;
            passData.RayCount =  _naiveGiConfig.RayCount;
            passData.MaxSteps = _naiveGiConfig.MaxRaySteps;
            
            builder.UseTexture(passData.EmissionInput);
            builder.UseTexture(passData.DistanceField);
            builder.SetRenderAttachment(passData.EmissionOutput, 0, AccessFlags.Write);
            builder.AllowGlobalStateModification(true);
            builder.AllowPassCulling(false);
            builder.SetRenderFunc<NaiveGIData>(GatherNaiveRadiance);
        }
    }

    private static void GatherNaiveRadiance(NaiveGIData passData, RasterGraphContext context) {
        context.cmd.SetGlobalInt(ShaderIDs.NaiveGIRayCount, passData.RayCount);
        context.cmd.SetGlobalInt(ShaderIDs.NaiveGIMaxSteps, passData.MaxSteps);
        context.cmd.SetGlobalTexture(ShaderIDs.DistanceField, passData.DistanceField);
        Blitter.BlitTexture(context.cmd, passData.EmissionInput, new Vector4(1, 1, 0, 0), passData.NaiveGIMaterial, 0);
    }
    #endregion

    #region RadianceCascades

    private void RecordRadianceCascades(RenderGraph renderGraph, BearRPContext context) {
        AddSeedPass(renderGraph, context);
        var (jfaFinal, jfaScratch) = AddPingPongPass(renderGraph, context);
        var distanceField = AddDistanceFieldPass(renderGraph, context, jfaFinal, jfaScratch);
        AddRcGather(renderGraph, context, distanceField, context.GiTextures.GiInput);
        AddBouncePass(renderGraph, context, distanceField);
    }

    private void AddRcGather(RenderGraph renderGraph, BearRPContext context, TextureHandle distanceField, TextureHandle emission) {
        AddRadianceCascadePass(renderGraph, context, distanceField, emission);
        AddMipCascade0Pass(renderGraph, context);
        AddTranslateCascade0Pass(renderGraph, context);
    }

    private void AddBouncePass(RenderGraph renderGraph, BearRPContext context, TextureHandle distanceField) {
        TextureHandle diffuseInput = _diffusePing;
        TextureHandle diffuseOutput = _diffusePong;
        for (int numBounce = 0; numBounce < _rcGiConfig.BounceCount; numBounce++) {
            AddDiffusePass(renderGraph, context, diffuseOutput);
            AddRcGather(renderGraph, context, distanceField, diffuseOutput);
            (diffuseInput, diffuseOutput) = (diffuseOutput, diffuseInput);
        }
    }
    
    private void AddRadianceCascadePass(RenderGraph renderGraph, BearRPContext context, TextureHandle dfOutputTexture, TextureHandle emission) {
        BearRPUtils.GetOrLoadMaterial(ref _cascadeMaterial, "Bear/RadianceCascades");
        
        for (int cascadeNum = _rcGiConfig.NumberOfCascades - 1; cascadeNum >= 0; cascadeNum--) {
            using (var builder = renderGraph.AddRasterRenderPass<RcGIData>($"Radiance Cascade {cascadeNum} Pass", out var passData, s_RcGather)) {
                var desc = _cascadeTh[cascadeNum].GetDescriptor(renderGraph);
                // Materials
                passData.RadianceCascadeMaterial = _cascadeMaterial;
                
                // Internal Variables
                passData.CascadeResolution = new Vector2(desc.width, desc.height);
                passData.ProbeDensity = new Vector2(_rcGiConfig.Cascade0ProbeDensity, _rcGiConfig.Cascade0ProbeDensity);
                passData.CascadeCount = _rcGiConfig.NumberOfCascades;
                passData.CascadeIndex = cascadeNum;
                passData.CascadeIntervalLength = _rcGiConfig.Cascade0Range;

                // Textures
                passData.Emission = emission;
                passData.DistanceField = dfOutputTexture;
                passData.CascadeN0 = _cascadeTh[passData.CascadeIndex];

                bool unBound = (cascadeNum < passData.CascadeCount - 1);
                passData.CascadeN1 = unBound ? _cascadeTh[cascadeNum + 1] : default;
                
                // Sun related
                passData.SkyBoxOn = _rcGiConfig.SkyBoxOn;
                passData.SkyColor = _rcGiConfig.SkyColor;
                passData.SunColor = _rcGiConfig.SunColor;
                passData.SunAngle = _rcGiConfig.SunAngle;
                passData.SunAngularRadius = _rcGiConfig.SunAngularRadius;
                passData.SunIntensity = _rcGiConfig.SunIntensity;
                
                builder.UseTexture(passData.Emission);
                builder.UseTexture(passData.DistanceField);
                if (unBound) builder.UseTexture(passData.CascadeN1);
                builder.AllowGlobalStateModification(true);
                builder.SetRenderAttachment(passData.CascadeN0, 0, AccessFlags.Write);
                builder.SetRenderFunc<RcGIData>(GatherCascades);
            }
        }
    }

    private static void GatherCascades(RcGIData passData, RasterGraphContext context) {
        // Cascades
        context.cmd.SetGlobalVector(ShaderIDs.CascadeResolution, passData.CascadeResolution);
        context.cmd.SetGlobalVector(ShaderIDs.ProbeDensity,      passData.ProbeDensity);
        context.cmd.SetGlobalInt(ShaderIDs.CascadeIndex,            passData.CascadeIndex);
        context.cmd.SetGlobalInt(ShaderIDs.CascadeCount,            passData.CascadeCount);
        context.cmd.SetGlobalFloat(ShaderIDs.CascadeIntervalLength, passData.CascadeIntervalLength);
        context.cmd.SetGlobalTexture(ShaderIDs.DistanceField,       passData.DistanceField);
        
        // Skybox
        context.cmd.SetGlobalInt(ShaderIDs.SkyboxOn,            passData.SkyBoxOn ? 1 : 0);
        context.cmd.SetGlobalFloat (ShaderIDs.SunAngle,         passData.SunAngle);
        context.cmd.SetGlobalFloat (ShaderIDs.SunAngularRadius, passData.SunAngularRadius);
        context.cmd.SetGlobalFloat (ShaderIDs.SunIntensity,     passData.SunIntensity);
        context.cmd.SetGlobalVector(ShaderIDs.SunColor,         passData.SunColor);
        context.cmd.SetGlobalVector(ShaderIDs.SkyColor,         passData.SkyColor);
        
        if (passData.CascadeIndex < passData.CascadeCount - 1) context.cmd.SetGlobalTexture(ShaderIDs.CascadeN1, passData.CascadeN1);
        Blitter.BlitTexture(context.cmd, passData.Emission, new Vector4(1, 1, 0, 0), passData.RadianceCascadeMaterial, 0);
    }

    private void AddMipCascade0Pass(RenderGraph renderGraph, BearRPContext context) {
        using (var builder = renderGraph.AddUnsafePass<RcMipData>("RC Cascade0 Mips", out var passData, s_RcMip)) {
            passData.Cascade0 = _cascadeTh[0];
            builder.UseTexture(passData.Cascade0, AccessFlags.ReadWrite);
            builder.AllowPassCulling(false);
            builder.SetRenderFunc<RcMipData>(MipCascade0);
        }
    }
    
    private static void MipCascade0(RcMipData passData, UnsafeGraphContext context) {
        context.cmd.GenerateMips(passData.Cascade0);
    }
    
    private void AddTranslateCascade0Pass(RenderGraph renderGraph, BearRPContext context) {
        using (var builder = renderGraph.AddRasterRenderPass<RcTranslateData>("Rc Final Mip", out var passData, s_RcTranslate)) {
            passData.RadianceCascadeMaterial = _cascadeMaterial;
            passData.Cascade0 = _cascadeTh[0];
            passData.GIOutput = context.GiTextures.GiOutput;

            var desc = passData.Cascade0.GetDescriptor(renderGraph);
            passData.Cascade0Resolution = new Vector2(desc.width, desc.height);

            builder.UseTexture(passData.Cascade0);
            builder.SetRenderAttachment(passData.GIOutput, 0, AccessFlags.Write);
            builder.AllowGlobalStateModification(true);
            builder.SetRenderFunc<RcTranslateData>(TranslateCascade0);
        }
    }

    private static void TranslateCascade0(RcTranslateData passData, RasterGraphContext context) {
        context.cmd.SetGlobalVector(ShaderIDs.CascadeResolution, passData.Cascade0Resolution);
        Blitter.BlitTexture(context.cmd, passData.Cascade0, new Vector4(1, 1, 0, 0), passData.RadianceCascadeMaterial, 1);
    }

    private void AddDiffusePass(RenderGraph renderGraph, BearRPContext context, TextureHandle diffuseOutput) {
        using (var builder = renderGraph.AddRasterRenderPass<DiffuseData>("Rc Diffuse", out var passData, s_RcDiffuse)) {
            passData.DiffuseMaterial = _cascadeMaterial;
            passData.NumPass = 2;
            passData.DiffuseOffset = 1;
            passData.Albedo = context.GBufferTextures.Albedo;
            passData.Occlusion = context.GBufferTextures.Occlusion;
            passData.Emission = context.GiTextures.GiInput;
            passData.IndirectLighting = context.GiTextures.GiOutput;
            passData.DiffuseOutput = diffuseOutput;            
            
            builder.AllowGlobalStateModification(true);
            builder.AllowPassCulling(false);
            builder.UseTexture(passData.Albedo);
            builder.UseTexture(passData.Occlusion);
            builder.UseTexture(passData.Emission);
            builder.UseTexture(passData.IndirectLighting);
            builder.SetRenderAttachment(passData.DiffuseOutput, 0, AccessFlags.Write);
            builder.SetRenderFunc<DiffuseData>(Diffuse);
        }
    }

    private void Diffuse(DiffuseData data, RasterGraphContext context) {
        context.cmd.SetGlobalTexture(ShaderIDs.Albedo, data.Albedo);
        context.cmd.SetGlobalTexture(ShaderIDs.OcclusionMap, data.Occlusion);
        context.cmd.SetGlobalTexture(ShaderIDs.EmissionMap, data.Emission);
        context.cmd.SetGlobalFloat(ShaderIDs.DiffuseOffset, data.DiffuseOffset);
        Blitter.BlitTexture(context.cmd, data.IndirectLighting, new Vector4(1, 1, 0, 0), data.DiffuseMaterial, data.NumPass);
    }
    
    #endregion
}