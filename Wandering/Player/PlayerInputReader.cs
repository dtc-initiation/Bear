using System;
using PIA;
using UnityEngine;
using UnityEngine.InputSystem;
using Wandering.Testing.GlobalIllumination;

namespace Wandering.Player;

[CreateAssetMenu(fileName="Player/InputReader", menuName="Wandering/InputReader")]
public class PlayerInputReader : ScriptableObject, PlayerInputActions.IPlayerActions {
    private PlayerInputActions _inputActions = null!;
    
    public event Action<bool> ClickPressed = delegate { };
    public event Action<Vector2> MouseMoved = delegate { };
    public Vector2 MousePosition { get; private set; }
    
    private void OnEnable() {
        _inputActions = new PlayerInputActions();
        _inputActions.Player.SetCallbacks(this);
        _inputActions.Player.Enable();
    }

    public void OnPoint(InputAction.CallbackContext context) {
        MousePosition = context.ReadValue<Vector2>();
        PainterDebugHUD.MoveEvents++;
        PainterDebugHUD.LastRawMouse = MousePosition;
        MouseMoved.Invoke(MousePosition);
    } 

    public void OnClick(InputAction.CallbackContext context) {
        PainterDebugHUD.ClickEvents++;
        
        switch (context.phase) {
            case InputActionPhase.Started:
                ClickPressed.Invoke(true);
                break;
            case InputActionPhase.Canceled:
                ClickPressed.Invoke(false);
                break;
        }
    }

    private void OnDisable() {
        _inputActions.Disable();
    }
}