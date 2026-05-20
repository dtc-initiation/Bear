using BearRP.Core.Interface;
using BearRP.Utils;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace BearRP.Core;

public class CameraSetupPass : IBearPass {
    public void Record(RenderGraph renderGraph, BearRPContext context) {
        using (var builder = renderGraph.AddRasterRenderPass<CameraData>("CameraSetupPass", out var passData)) {
            
            var camera = context.Camera;
            var renderingToBackBuffer = (camera.cameraType == CameraType.Game) && (camera.targetTexture == null);
            float x = 1f;
            float nearClipPlane = camera.nearClipPlane;
            float farClipPlane = camera.farClipPlane;
            float z = 0;
            var camInfo = new Vector4(x, nearClipPlane, farClipPlane, z);
            if (SystemInfo.graphicsUVStartsAtTop && renderingToBackBuffer) {
                camInfo.x *= -1;
            }

            passData.CameraInfo = camInfo;
            
            builder.AllowPassCulling(false);
            builder.AllowGlobalStateModification(true);
            builder.SetRenderFunc((CameraData data, RasterGraphContext context) => {
                context.cmd.SetGlobalVector(ShaderIDs.CameraInfo, passData.CameraInfo);
            });
        }
    }
}