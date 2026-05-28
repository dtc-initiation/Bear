using BearRP.Core;
using BearRP.DirectLighting;
using UnityEngine;
using UnityEngine.Rendering;

namespace BearRP;

public static class CameraExtensions {
    public static BearCamera GetOrAddBearCamera(this Camera camera) {
        if (Core.BearRP.BearCameraLookup.TryGetValue(camera, out BearCamera bC)) {
            return bC;
        }
        
        bC = camera.GetComponent<BearCamera>();
        if (bC == null) {
            bC = camera.gameObject.AddComponent<BearCamera>();
            
        }
        bC.SetCamera(camera);
        Core.BearRP.BearCameraLookup.Add(camera, bC);
        return bC;
    }

    public static CameraData GetOrAddCameraData(this Camera camera) {
        if (Core.BearRP.CameraDataLookup.TryGetValue(camera, out CameraData data)) {
            return data;
        };
        CameraData newData = new CameraData();
        Core.BearRP.CameraDataLookup.Add(camera, newData);
        return newData;
    }

    public static void SetupAndCull(this Camera camera, ScriptableRenderContext context) {
        CameraData data = GetOrAddCameraData(camera);
        if (camera.cameraType == CameraType.Game) {
            camera.GetOrAddBearCamera().ApplyCameraAspect();
        }
        if (camera.TryGetCullingParameters(camera, out ScriptableCullingParameters cullingParams)) {
            data.CullingResults = context.Cull(ref cullingParams);
            
            // Debug purpose only
            Core.BearRP.CameraLightCount.Add(camera, data.CullingResults.visibleLights.Length);
        };
    }
}

public static class LightExtensions {
    public static BearLight TryGetBearLight(this Light light) {
        BearLight bearLight;
        bool cached = Core.BearRP.BearLightLookup.TryGetValue(light, out bearLight);
        if (cached) {
            return bearLight;
        }
        bool exists = light.gameObject.TryGetComponent<BearLight>(out bearLight);
        if (!exists) {
            bearLight = light.gameObject.AddComponent<BearLight>();
        }
        bearLight.Light = light;
        Core.BearRP.BearLightLookup.Add(light, bearLight);
        return bearLight;
    }
}
