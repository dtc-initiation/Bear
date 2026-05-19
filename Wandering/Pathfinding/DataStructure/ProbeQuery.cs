namespace Wandering.Pathfinding.DataStructure;

public readonly struct ProbeQuery {
    public readonly int StartingLayer;
    public readonly int StartingCell;
    public readonly NavFlag StartingNavFlag;

    public ProbeQuery(int startingLayer, int startingCell, NavFlag startingNavFlag) {
        StartingLayer = startingLayer;
        StartingCell = startingCell;
        StartingNavFlag = startingNavFlag;
    }
}