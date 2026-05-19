using Wandering.Pathfinding.DataStructure;

namespace Wandering.Pathfinding;

public readonly struct ProbeNode {
    public readonly int Layer;
    public readonly int Cell;
    public readonly int Cost;
    public readonly NavFlag BestFlag;
    public readonly int QueryId;

    public ProbeNode(int layer, int cell, int cost, NavFlag bestFlag, int queryId) {
        Layer = layer;
        Cell = cell;
        Cost = cost;
        BestFlag = bestFlag;
        QueryId = queryId;
    }
}