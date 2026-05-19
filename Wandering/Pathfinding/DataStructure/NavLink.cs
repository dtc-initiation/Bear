namespace Wandering.Pathfinding.DataStructure;

public struct NavLink {
    public readonly int TargetLayer;
    public readonly int TargetCell;
    public readonly NavFlag StartNavFlag;
    public readonly NavFlag EndNavFlag;
    public readonly int NavCost;
    public readonly string TransitionId;

    public NavLink(
        int targetLayer,
        int targetCell,
        NavFlag startNavFlag,
        NavFlag endNavFlag,
        int navCost,
        string transitionId) {
        TargetLayer = targetLayer;
        TargetCell = targetCell;
        StartNavFlag = startNavFlag;
        EndNavFlag = endNavFlag;
        NavCost = navCost;
        TransitionId = transitionId;
    }

    public static readonly NavLink Sentinel = new NavLink(-1, -1, NavFlag.Null, NavFlag.Null, -1, "");
}