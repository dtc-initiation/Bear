using UnityEditor.Animations;
using UnityEngine;
using Wandering.Pathfinding.DataStructure;
using Wandering.World;

namespace Wandering.Pathfinding;

public struct Transition {
    public bool TransitionStarted;
    public bool TransitionFinished;
    public readonly PathStep Step;
    public readonly Vector3 TargetPosition;
    
    public Transition(PathStep step, Vector3 targetPosition) {
        Step = step;
        TransitionStarted = false;
        TransitionFinished = false;
        TargetPosition = targetPosition;
    }
    
}

public class TransitionDriver {
    private Transform _transitionTransform;
    private AnimatorController _animController;
    private WorldContext _worldContext;
    private Transition _currentTransition;
    
    private readonly float _arrivalThreshold;
    private float _baseSpeed;
    private float _currentSpeed;
    
    
    public NavFlag TransitionNavFlag => _currentTransition.Step.TargetNavFlag;
    public bool InTransition => !_currentTransition.TransitionFinished && _currentTransition.TransitionStarted;
    public bool TransitionFinished => _currentTransition.TransitionFinished;
    
    public TransitionDriver(float baseSpeed, Transform transform, AnimatorController animController, WorldContext worldContext) {
        _transitionTransform = transform;
        _animController = animController;
        _worldContext = worldContext;
        _arrivalThreshold = 0.05f;
        _baseSpeed = baseSpeed;
        _currentSpeed = 1f;
    }
    
    public void BeginStep(PathStep step) {
        var targetPosition = _worldContext.GridInfo.CellToPos(step.TargetLayer, step.TargetCell);
        _currentTransition = new Transition(step, targetPosition);
        _currentTransition.TransitionStarted = true;
    }
    
    public void Tick(float deltaTime) {
        Vector3 currentPos = _transitionTransform.position;
        float distance = Vector3.Distance(currentPos, _currentTransition.TargetPosition);
        
        if (distance <= _arrivalThreshold) {
            _currentTransition.TransitionFinished = true;
        } else {
            float d = deltaTime * _currentSpeed;
            _transitionTransform.position = Vector3.MoveTowards(currentPos, _currentTransition.TargetPosition, d);
        }
    }

    public void EndStep() {
        _transitionTransform.position = _currentTransition.TargetPosition;
    }
}