using UnityEngine;

namespace BearRP.Debug;

public class BearRpHUD : MonoBehaviour {
    private readonly FrameTiming[] _timings = new FrameTiming[1];
    
    private void OnGUI() {
        FrameTimingManager.CaptureFrameTimings();
        uint n = FrameTimingManager.GetLatestTimings(1, _timings);
        if (n == 0) { GUILayout.Label("Frame timing unavailable"); return; }

        double cpuMs = _timings[0].cpuFrameTime;
        double gpuMs = _timings[0].gpuFrameTime;
        double frameMs = System.Math.Max(cpuMs, gpuMs);
        GUILayout.Label($"CPU: {cpuMs:F2} ms");
        GUILayout.Label($"GPU: {gpuMs:F2} ms");
        GUILayout.Label($"FPS: {(frameMs > 0 ? 1000.0 / frameMs : 0):F1}");
    }
}