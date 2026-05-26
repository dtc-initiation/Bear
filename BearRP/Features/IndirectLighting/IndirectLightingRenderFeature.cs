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
    private Material _distanceFieldMaterial;
    private Material _naiveMaterial;
    private Material _cascadeMaterial;
    
    // Configs
    private RcGiConfig RcGiConfig;
    private NaiveGiConfig NaiveGiConfig;
    
    // Textures
    private TextureHandle[] CascadeTH;
    private TextureHandle JfaPingTH;
    private TextureHandle JfaPongTH;

    public IndirectLightingRenderFeature(RcGiConfig rcGiConfig, NaiveGiConfig naiveGiConfig) {
        RcGiConfig = rcGiConfig;
        NaiveGiConfig = naiveGiConfig;
        CascadeTH = new TextureHandle[RcGiConfig.NumberOfCascades];
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
        var inputDesc = context.GiTextures.InputTexture.GetDescriptor(renderGraph);
        var jfaPingDesc = new TextureDesc(inputDesc.width, inputDesc.height) {
            name = "jfaPing",
            clearBuffer = true,
            clearColor = new Color(0f, 0f, 0f, 0f),
            colorFormat = GraphicsFormat.R16G16_SFloat,
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        var jfaPongDesc = new TextureDesc(inputDesc.width, inputDesc.height) {
            name = "jfaPong",
            clearBuffer = true,
            clearColor = new Color(0f, 0f, 0f, 0f),
            colorFormat = GraphicsFormat.R16G16_SFloat,
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        JfaPingTH = renderGraph.CreateTexture(jfaPingDesc);
        JfaPongTH = renderGraph.CreateTexture(jfaPongDesc);

        context.GiTextures.JfaPing = JfaPingTH;
        context.GiTextures.JfaPong = JfaPongTH;
    }

    private void CreateRadianceCascades(RenderGraph renderGraph, BearRPContext context) {
        for (int cascadeNum = 0; cascadeNum < RcGiConfig.NumberOfCascades; cascadeNum++) {
            int rayCount = (int) Mathf.Pow(2.0f, cascadeNum);
            var probeCount = Mathf.Pow(0.5f, cascadeNum) * context.BearCamera.GetPixelResolution() / RcGiConfig.Cascade0ProbeDensity;
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
            CascadeTH[cascadeNum] = renderGraph.CreateTexture(desc);
        }
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
        
        using (var builder = renderGraph.AddRasterRenderPass<JFAData>("JFA Seed", out var passData)) {
            passData.PassNumber = 0;
            passData.EmissionTexture = context.GiTextures.InputTexture;
            passData.JfaMaterial = _distanceFieldMaterial;
            passData.JfaInput = JfaPingTH;
            
            builder.UseTexture(passData.EmissionTexture);
            builder.SetRenderAttachment(passData.JfaInput, 0, AccessFlags.Write);
            builder.AllowGlobalStateModification(true);
            builder.AllowPassCulling(false);
            builder.SetRenderFunc<JFAData>(PopulateJfaSeed);
        }
    }

    private static void PopulateJfaSeed(JFAData passData, RasterGraphContext context) {
        Blitter.BlitTexture(context.cmd, passData.EmissionTexture, new Vector4(1, 1, 0, 0), passData.JfaMaterial, passData.PassNumber);
    }

    private (TextureHandle, TextureHandle) AddPingPongPass(RenderGraph renderGraph, BearRPContext context) {
        TextureHandle jfaInput = JfaPingTH;
        TextureHandle jfaOutput = JfaPongTH;
        var inputDesc = jfaInput.GetDescriptor(renderGraph);
        int dimensions = Mathf.Max(inputDesc.width, inputDesc.height);
        int numPass = Mathf.CeilToInt(Mathf.Log(dimensions, 2f));
        for (int i = 0; i < numPass; i++) {
            using (var builder = renderGraph.AddRasterRenderPass<JFAData>($"JFA Pass {i}", out var passData)) {
                passData.PassNumber = 1;
                passData.JfaMaterial = _distanceFieldMaterial;
                passData.JfaInput = jfaInput;
                passData.JfaOutput = jfaOutput;
                
                passData.JfaTOffset = (int) Mathf.Pow(2, numPass - i - 1);
                passData.RayCount = NaiveGiConfig.RayCount;
                passData.MaxSteps = NaiveGiConfig.MaxRaySteps;
                
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
        using (var builder = renderGraph.AddRasterRenderPass<JFAData>("Distance Field", out var passData)) {
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
        using (var builder = renderGraph.AddRasterRenderPass<NaiveGIData>("Naive Gathering Pass", out var passData)) {
            passData.NaiveGIMaterial = _naiveMaterial;
            passData.EmissionInput = context.GiTextures.InputTexture;
            passData.DistanceField = dfOutputTexture;
            passData.EmissionOutput = context.GiTextures.OutputTexture;
            passData.RayCount =  NaiveGiConfig.RayCount;
            passData.MaxSteps = NaiveGiConfig.MaxRaySteps;
            
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
        AddRadianceCascadePass(renderGraph, context, distanceField);
        AddMipCascade0Pass(renderGraph, context);
        AddTranslateCascade0Pass(renderGraph, context);
    }

    private void AddRadianceCascadePass(RenderGraph renderGraph, BearRPContext context, TextureHandle dfOutputTexture) {
        BearRPUtils.GetOrLoadMaterial(ref _cascadeMaterial, "Bear/RadianceCascades");
        
        for (int cascadeNum = RcGiConfig.NumberOfCascades - 1; cascadeNum >= 0; cascadeNum--) {
            using (var builder = renderGraph.AddRasterRenderPass<RcGIData>($"Radiance Cascade {cascadeNum} Pass", out var passData)) {
                var desc = CascadeTH[cascadeNum].GetDescriptor(renderGraph);
                // Materials
                passData.RadianceCascadeMaterial = _cascadeMaterial;
                
                // Internal Variables
                passData.CascadeResolution = new Vector2(desc.width, desc.height);
                passData.ProbeDensity = new Vector2(RcGiConfig.Cascade0ProbeDensity, RcGiConfig.Cascade0ProbeDensity);
                passData.CascadeCount = RcGiConfig.NumberOfCascades;
                passData.CascadeIndex = cascadeNum;
                passData.CascadeIntervalLength = RcGiConfig.Cascade0Range;

                // Textures
                passData.Emission = context.GiTextures.InputTexture;
                passData.DistanceField = dfOutputTexture;
                passData.CascadeN0 = CascadeTH[passData.CascadeIndex];

                bool unBound = (cascadeNum < passData.CascadeCount - 1);
                passData.CascadeN1 = unBound ? CascadeTH[cascadeNum + 1] : default;
                
                // Sun related
                passData.SkyBoxOn = RcGiConfig.SkyBoxOn;
                passData.SkyColor = RcGiConfig.SkyColor;
                passData.SunColor = RcGiConfig.SunColor;
                passData.SunAngle = RcGiConfig.SunAngle;
                passData.SunAngularRadius = RcGiConfig.SunAngularRadius;
                passData.SunIntensity = RcGiConfig.SunIntensity;
                
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
        using (var builder = renderGraph.AddUnsafePass<RcMipData>("RC Cascade0 Mips", out var passData)) {
            passData.Cascade0 = CascadeTH[0];
            builder.UseTexture(passData.Cascade0, AccessFlags.ReadWrite);
            builder.AllowPassCulling(false);
            builder.SetRenderFunc<RcMipData>(MipCascade0);
        }
    }
    
    private static void MipCascade0(RcMipData passData, UnsafeGraphContext context) {
        context.cmd.GenerateMips(passData.Cascade0);
    }
    
    private void AddTranslateCascade0Pass(RenderGraph renderGraph, BearRPContext context) {
        using (var builder = renderGraph.AddRasterRenderPass<RcTranslateData>("Rc Final Mip", out var passData)) {
            passData.RadianceCascadeMaterial = _cascadeMaterial;
            passData.Cascade0 = CascadeTH[0];
            passData.GIOutput = context.GiTextures.OutputTexture;

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
    
    #endregion
}