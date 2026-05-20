using System.Linq;
using BearRP.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

namespace Wandering.Testing.GlobalIllumination;

public class PainterDebugHUD : MonoBehaviour {
    [SerializeField] private Painter painter;
    [SerializeField] private Material paintMaterial;
    [SerializeField] private RenderTexture targetTexture;
    [SerializeField] private BearRPAsset bearAsset;

    public static Vector2 LastRawMouse;
    public static Vector2 LastUIMouse;
    public static Vector2 LastConvertedMouse;

    private void OnGUI() {
        GUILayout.BeginArea(new Rect(10, 10, 520, 640), GUI.skin.box);
        GUILayout.Label($"Frame {Time.frameCount}  t={Time.time:F2}");
        GUILayout.Space(4);
        GUILayout.Label("— Mouse pipeline —");
        GUILayout.Label($"Raw (Screen):   {LastRawMouse}");
        GUILayout.Label($"UI  (internal): {LastUIMouse}");
        GUILayout.Label($"Converted:      {LastConvertedMouse}");

        if (BearRP.Core.BearRP.RPAsset) {
            GUILayout.Label($"S.Cull Count : {BearRP.Core.BearRP.SharedCullingLightCount}");
        }
        else {
            foreach (var typeGroup in BearRP.Core.BearRP.CameraLightCount
                         .Where(kv => kv.Key != null)
                         .GroupBy(kv => kv.Key.cameraType)) {
                int typeTotal = typeGroup.Sum(kv => kv.Value);
                GUILayout.Label($"{typeGroup.Key}: {typeTotal}");
                foreach (var kv in typeGroup) {
                    GUILayout.Label($"   {kv.Key.name}: {kv.Value}");
                }
            }
        }
        
        GUILayout.Label($"SM Vertex Size : {BearRP.Core.BearRP.MeshVertexSize}");
        
        GUILayout.EndArea();
    }
}