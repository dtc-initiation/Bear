using BearCore.Manager;
using BearRP;
using BearRP.Core;
using UnityEngine;

namespace Wandering.Player;

public class UIMouse : MonoBehaviour {
    public Vector2 currentPosition;
    
    [SerializeField] private PlayerInputReader inputReader = null!;

    private CameraManager _cameraManager = null!;

    private void Update() {
        UpdateMouseUIPosition();
    }
    
    private void Awake() {
        _cameraManager = Manager.Get<CameraManager>();
    }
    
    private void UpdateMouseUIPosition() {
        currentPosition = GetMouseViewPosition(_cameraManager.gameCamera);
    }
    
    public Vector2 GetMouseUIPosition() {
        return GetMouseViewPosition(_cameraManager.gameCamera);
    }
    
    private Vector2 GetMouseViewPosition(Camera camera) {
        BearCamera bC = camera.GetOrAddBearCamera();
        currentPosition = bC.TransformMousePosition(inputReader.MousePosition);
        return currentPosition;
    }
}