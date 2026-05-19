namespace Wandering.Pathfinding.DataStructure;

public readonly struct PathQuery {
    public readonly int StartingLayer;
    public readonly int StartingCell;
    public readonly NavFlag StartingNavFlag;
    public readonly int TargetLayer;
    public readonly int TargetCell;

    public PathQuery(int startingLayer, int startingCell, NavFlag startingNavFlag, int targetLayer, int targetCell) {
        StartingLayer = startingLayer;
        StartingCell = startingCell;
        StartingNavFlag = startingNavFlag;
        TargetLayer = targetLayer;
        TargetCell = targetCell;
    }

    public bool IsMatch(int layer, int cell) {
        return (TargetLayer ==  layer && TargetCell == cell);
    }
}