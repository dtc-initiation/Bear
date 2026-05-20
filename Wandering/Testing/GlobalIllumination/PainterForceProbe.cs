using BearRP;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Wandering.Testing.GlobalIllumination;

public class PainterForceProbe : MonoBehaviour {
    [SerializeField] private Material paintMaterial;
    [SerializeField] private RenderTexture target;
    [SerializeField] private int checkerCells = 16;

    private Vector2 _internalResolution;
    private RenderTexture _probeTemp;
    private Texture2D testPattern;

    private void Awake() {
        var cam = Camera.main;
        var bearCam = cam.GetOrAddBearCamera();
        _internalResolution = new Vector2(bearCam.GetPixelWidth(), bearCam.GetPixelHeight());
        testPattern = BuildCheckerboard((int)_internalResolution.x, (int)_internalResolution.y, checkerCells);
    }

    private void OnEnable() {
        if (target != null) {
            _probeTemp = new RenderTexture(target.descriptor) { name = "ProbeTempRT" };
            _probeTemp.Create();
        }
    }

    void OnDisable() {
        if (_probeTemp != null) {
            _probeTemp.Release();
            Destroy(_probeTemp);
            _probeTemp = null;
        }
    }

    static Texture2D BuildCheckerboard(int w, int h, int cells) {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false, true) {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            name = "DebugCheckerboard"
        };
        int cellW = Mathf.Max(1, w / cells);
        int cellH = Mathf.Max(1, h / cells);
        var px = new Color32[w * h];
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++) {
            bool on = ((x / cellW) + (y / cellH)) % 2 == 0;
            // Corners tinted so orientation is obvious
            Color32 c = on ? new Color32(235, 235, 235, 255) : new Color32(40, 40, 40, 255);
            if (x < 16 && y < 16) c = new Color32(255, 0, 0, 255); // bottom-left = red
            else if (x >= w - 16 && y < 16) c = new Color32(0, 255, 0, 255); // bottom-right = green
            else if (x < 16 && y >= h - 16) c = new Color32(0, 0, 255, 255); // top-left = blue
            else if (x >= w - 16 && y >= h - 16) c = new Color32(255, 255, 0, 255); // top-right = yellow
            px[y * w + x] = c;
        }

        tex.SetPixels32(px);
        tex.Apply(false, false);
        return tex;
    }

    private void Update() {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.f8Key.wasPressedThisFrame && testPattern != null) {
            Graphics.Blit(testPattern, target);
        }

        if (kb.f7Key.wasPressedThisFrame && paintMaterial != null && _probeTemp != null) {
            paintMaterial.SetFloat("_IsDrawing", 1f);
            paintMaterial.SetVector("_From", new Vector2(50, 50));
            paintMaterial.SetVector("_To", new Vector2(400, 220));
            paintMaterial.SetFloat("_RadiusSquared", 400f);
            paintMaterial.SetColor("_Color", Color.red);
            paintMaterial.SetVector("_BlitScaleBias", new Vector4(1, 1, 0, 0));
            Graphics.Blit(target, _probeTemp, paintMaterial);
            Graphics.Blit(_probeTemp, target);
        }

        if (kb.f9Key.wasPressedThisFrame && paintMaterial != null) {
            var active = RenderTexture.active;
            RenderTexture.active = target;
            GL.Clear(true, true, new Color(0.0f, 0.0f, 0.0f, 0f));
            paintMaterial.SetVector("_BlitScaleBias", new Vector4(1, 1, 0, 0));
            RenderTexture.active = active;
        }
    }
}