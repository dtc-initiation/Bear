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

    public static int ClickEvents;
    public static int MoveEvents;
    public static int StartPaintCalls;
    public static int FinishPaintCalls;
    public static int UpdateMaterialCalls;
    public static int BlitCalls;

    public static Vector2 LastRawMouse;
    public static Vector2 LastUIMouse;
    public static Vector2 LastConvertedMouse;

    public static string LastNote = "";

    private void OnGUI() {
        GUILayout.BeginArea(new Rect(10, 10, 520, 640), GUI.skin.box);
        GUILayout.Label($"Frame {Time.frameCount}  t={Time.time:F2}");
        GUILayout.Space(4);
        GUILayout.Label("— Mouse pipeline —");
        GUILayout.Label($"Raw (Screen):   {LastRawMouse}");
        GUILayout.Label($"UI  (internal): {LastUIMouse}");
        GUILayout.Label($"Converted:      {LastConvertedMouse}");
        GUILayout.Label($"Note: {LastNote}");
        GUILayout.EndArea();
    }
}