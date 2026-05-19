namespace Wandering.Pathfinding.DataStructure;

public readonly struct PathStep {
    public readonly int TargetLayer;
    public readonly int TargetCell;
    public readonly NavFlag TargetNavFlag;
    public readonly string TransitionID;
    public PathStep(int targetLayer, int targetCell, NavFlag targetNavFlag, string transitionId) {
        TargetLayer = targetLayer;
        TargetCell = targetCell;
        TargetNavFlag = targetNavFlag;
        TransitionID = transitionId;
    }
}