using System.Collections.Generic;
using System.Runtime.InteropServices;
using BearRP.Utils;
using BearRP.Core;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace BearRP.DirectLighting;

public class LightData {
    // Lighting related
    public readonly HashSet<Light> VisibleLights;
    private readonly int _maximumLightCount;
    
    private readonly LightInfo[] _lightInfoScratch;
    public readonly ComputeBuffer LightBuffer;
    
    public LightData(int maximumLightCount) {
        VisibleLights = new HashSet<Light>();
        _maximumLightCount = maximumLightCount;
        
        _lightInfoScratch = new LightInfo[_maximumLightCount];
        LightBuffer = new  ComputeBuffer(maximumLightCount, Marshal.SizeOf<LightInfo>(), ComputeBufferType.Structured);
    }

    public void RegisterVisibleLights(CullingResults? sharedCullingRes, List<Camera> cameras) {
        VisibleLights.Clear();
        if (Core.BearRP.RPAsset.UseSharedCulling && sharedCullingRes.HasValue) {
            foreach (VisibleLight vLight in sharedCullingRes.Value.visibleLights) {
                VisibleLights.Add(vLight.light);
            }
        } else {
            foreach (Camera camera in cameras) {
                CameraData cData = camera.GetOrAddCameraData();
                foreach (VisibleLight vLight in cData.CullingResults.visibleLights) {
                    VisibleLights.Add(vLight.light);
                }
            }
        }
    }
    
    public void PopulateLight(Dictionary<Light, int> lightIdLookup) {
        // TODO Robust Dense instanceID to Sparse LightId mismatch fix.
        int denseIdx = 0;
        foreach (Light light in VisibleLights) {
            var id = lightIdLookup.GetValueOrDefault(light, -1);
            BearLight bearLight = light.TryGetBearLight();

            var p = bearLight.transform.position;
            var color = bearLight.Intensity * bearLight.Color;

            _lightInfoScratch[denseIdx] = new LightInfo(
                new Vector4(p.x, p.y, p.z, id),
                new Vector4(color.r, color.g, color.b, bearLight.Distance),
                new Vector4(bearLight.ForwardDir.x, bearLight.ForwardDir.y, bearLight.ConeAngleSize, 1)
            );
            denseIdx++;
        }
        LightBuffer.SetData(_lightInfoScratch);
    }

    public void Dispose() {
        LightBuffer.Release();
    }
}