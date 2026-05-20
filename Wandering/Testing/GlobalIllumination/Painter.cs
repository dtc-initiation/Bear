using System;
using BearRP;
using Sirenix.OdinInspector;
using UnityEngine;
using Wandering.Player;

namespace Wandering.Testing.GlobalIllumination;

public class Painter : MonoBehaviour {
    public enum PaintShape {
        Circle,
        Square
    }
    
    [Header("Fixed Settings")]
    [SerializeField] private Material paintMaterial = null!;
    [SerializeField] private PlayerInputReader inputReader = null!;
    [SerializeField] private RenderTexture targetTexture = null!;
    
    [Header("Varaible Settings")]
    [SerializeField] private PaintShape shape = PaintShape.Circle;
    [SerializeField, Range(0f, 30f)] private float radius = 5f;
    [SerializeField, Range(0f, 1f)] private float friction = 0.1f;
    [SerializeField] public Color currentColor = Color.navajoWhite;
    
    private Vector2 _internalResolution;
    private UIMouse _uiMouse = null!;
    private bool _isDrawing;
    private Vector2 _currentMousePosition;
    private Vector2 _currentPosition;
    private Vector2 _prevPosition;
    private RenderTexture _tempRT;
    
    private Vector2 _lastFromSent;
    private Vector2 _lastToSent;
    
    public bool  DebugIsDrawing    => _isDrawing;
    public Color   DebugColor      => currentColor;
    public float   DebugRadiusSq   => radius * radius;
    public Vector2 DebugResolution => _internalResolution;
    public Vector2 DebugFrom => _lastFromSent;
    public Vector2 DebugTo   => _lastToSent;
    public PaintShape Shape => shape;
    
    private void Awake() {
        var cam = Camera.main;
        var bearCam = cam.GetOrAddBearCamera();
        _internalResolution = new Vector2(bearCam.GetPixelWidth(), bearCam.GetPixelHeight());
        _tempRT = new RenderTexture(targetTexture.descriptor);
        
        bool created = _tempRT.Create();
        paintMaterial.SetVector("_Resolution", _internalResolution);
        paintMaterial.SetFloat("_ShapeMode", (float)shape);
        
        var uiMouse = GetComponent<UIMouse>();
        if (uiMouse == null) {
            uiMouse = gameObject.AddComponent<UIMouse>();
        }
        _uiMouse = uiMouse;
        
        // Register callbacks
        inputReader.ClickPressed += ToggleDraw;
        inputReader.MouseMoved += OnMouseMove;
    }
    
    public void SetShape(PaintShape newShape) { 
        shape = newShape;
        paintMaterial.SetFloat("_ShapeMode", (float)newShape); 
    }
    
    private void ToggleDraw(bool pressed) {
        if (pressed) {
            StartPaint();
            return;
        } 
        FinishPaint();
    }

    private void StartPaint() {
        _isDrawing = true;
        paintMaterial.SetFloat("_IsDrawing", 1.0f);
        paintMaterial.SetColor("_Color", currentColor);
        paintMaterial.SetFloat("_RadiusSquared", radius * radius);
        
        Vector2 mousePosition = _uiMouse.GetMouseUIPosition();
        _currentMousePosition =  ConvertMousePositionOrigin(mousePosition);
        _currentPosition = _currentMousePosition;
        _prevPosition = _currentMousePosition;
        
        OnMouseMove(mousePosition);
    }

    private void OnMouseMove(Vector2 _) {
        if (!_isDrawing) { return; }
        
        Vector2 mousePosition = _uiMouse.GetMouseUIPosition();
        _currentMousePosition = ConvertMousePositionOrigin(mousePosition);

        Vector2 ui = _uiMouse.GetMouseUIPosition();
        PainterDebugHUD.LastUIMouse = ui;
        PainterDebugHUD.LastConvertedMouse = _currentMousePosition;
        
        if (_currentMousePosition.x < 0 || _currentMousePosition.x >= _internalResolution.x) {
            FinishPaint();
        }
        if (_currentMousePosition.y < 0 || _currentMousePosition.y >= _internalResolution.y) {
            FinishPaint();
        }
        UpdateMaterial();
    }

    private void UpdateMaterial() {
        var targetPosition = _currentMousePosition;
        var dist = Vector2.Distance(_currentPosition, targetPosition);
        if (dist > 0) {
            var dir = new Vector2(
                (targetPosition.x - _currentPosition.x) /  dist,
                (targetPosition.y - _currentPosition.y) /  dist
            );
            var length = Mathf.Max(dist - radius, 0);
            var ease = 1 - (float)Math.Pow(friction, 1f / 60 * 10);

            _currentPosition.x += dir.x * length * ease;
            _currentPosition.y += dir.y * length * ease;
        } else {
            _currentPosition = targetPosition;
        }

        _lastFromSent = _prevPosition;
        _lastToSent   = _currentPosition;
        paintMaterial.SetVector("_From", _lastFromSent);
        paintMaterial.SetVector("_To",   _lastToSent);
        
        // paintMaterial.SetVector("_From", _prevPosition);
        // paintMaterial.SetVector("_To", _currentPosition);

        Graphics.Blit(targetTexture, _tempRT, paintMaterial);
        Graphics.Blit(_tempRT, targetTexture);
        _prevPosition = _currentPosition;
    }
    
    private void FinishPaint() {
        if (!_isDrawing) { return; }
        _isDrawing = false;
        paintMaterial.SetFloat("_IsDrawing", 0.0f);
    }

    private Vector2 ConvertMousePositionOrigin(Vector2 mousePixelPosition) {
        mousePixelPosition.x = Mathf.Floor(mousePixelPosition.x);
        mousePixelPosition.y = Mathf.Floor(mousePixelPosition.y);
        return new Vector2(
            mousePixelPosition.x + (float) _internalResolution.x/2,
            mousePixelPosition.y + (float) _internalResolution.y/2
            );
    }
}