using BearCore.Manager;
using UnityEditor.Animations;
using UnityEngine;
using Wandering.Pathfinding;
using Wandering.Pathfinding.DataStructure;

namespace Wandering.Character;

public class Character : MonoBehaviour {
    [SerializeField] private float baseMoveSpeed = 1f;
    [SerializeField] private AnimatorController _animatorController = null!;
    
    private Navigator _navigator = null!;
    private TransitionDriver _transitionDriver = null!;
    
    protected virtual void Awake() {
        _navigator = new Navigator(transform, GameplayManager.Instance!.World);
        _transitionDriver = new TransitionDriver(baseMoveSpeed, transform, _animatorController, GameplayManager.Instance.World!.WorldContext);
    }

    private void Update() {
        float deltaTime = Time.deltaTime;
        SelectChore(deltaTime);
        Navigate(deltaTime);
    }

    private void SelectChore(float deltaTime) {
        
    }
    
    private void Navigate(float deltaTime) {
        if (_navigator.Status == NavigationStatus.Navigating && !_transitionDriver.InTransition) {
            PathStep nextStep = _navigator.PopStep();
            _transitionDriver.BeginStep(nextStep);
        }

        if (_navigator.Status == NavigationStatus.Navigating && _transitionDriver.InTransition) {
            _transitionDriver.Tick(deltaTime);
        }

        if (_navigator.Status == NavigationStatus.Navigating && _transitionDriver.TransitionFinished) {
            _transitionDriver.EndStep();
            _navigator.UpdateNavFlag(_transitionDriver.TransitionNavFlag);
            _navigator.UpdateNavigation();
        }
    }
    
    public void GoTo(IApproachable target) {
        _navigator.GoTo(target);
    }
}