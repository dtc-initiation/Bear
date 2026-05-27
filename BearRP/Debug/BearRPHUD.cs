using UnityEngine;
using UnityEngine.Profiling;

namespace BearRP.Debug;

public class BearRpHUD : MonoBehaviour {
    private readonly FrameTiming[] _timings = new FrameTiming[1];
    private static readonly (string label, string sampler)[] Rows = {
        ("JFA Seed",    "GI.JfaSeed"),
        ("JFA PingPong","GI.JfaPingPong"),
        ("Distance Fld","GI.DistanceField"),
        ("Naive GI",    "GI.NaiveGather"),
        ("RC Gather",   "GI.RcGather"),
        ("RC Mip",      "GI.RcMip"),
        ("RC Translate","GI.RcTranslate"),
    };
    private Recorder[] _recorders = null!;
    private void OnEnable() {
        _recorders = new Recorder[Rows.Length];
        for (int i = 0; i < Rows.Length; i++) {
            _recorders[i] = Recorder.Get(Rows[i].sampler);
            _recorders[i].enabled = true;
        }
    }
    
    private void OnGUI() {
        FrameTimingManager.CaptureFrameTimings();
        uint n = FrameTimingManager.GetLatestTimings(1, _timings);
        if (n == 0) { GUILayout.Label("Frame timing unavailable"); return; }

        double cpuMs = _timings[0].cpuFrameTime;
        double gpuMs = _timings[0].gpuFrameTime;
        double frameMs = System.Math.Max(cpuMs, gpuMs);
        
        GUILayout.BeginArea(new Rect(10, 10, 260, 180*2), GUI.skin.box);
        GUILayout.Label($"CPU: {cpuMs:F2} ms");
        GUILayout.Label($"GPU: {gpuMs:F2} ms");
        GUILayout.Label($"FPS: {(frameMs > 0 ? 1000.0 / frameMs : 0):F1}");
        for (int i = 0; i < Rows.Length; i++) {
            double gpuMss = _recorders[i].gpuElapsedNanoseconds / 1_000_000.0;
            GUILayout.Label($"{Rows[i].label,-14} {gpuMss,6:F3} ms");
        }
        GUILayout.EndArea();
    }
}