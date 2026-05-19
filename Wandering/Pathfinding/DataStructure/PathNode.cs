namespace Wandering.Pathfinding.DataStructure;

public readonly struct PathNode {
    public readonly int Layer;
    public readonly int Cell;
    public readonly NavFlag CellNavFlag;

    public readonly int ParentLayer;
    public readonly int ParentCell;
    public readonly NavFlag ParentCellNavFlag;

    public readonly string TransitionId;
    public readonly int Cost;
    public readonly int QueryId;

    public PathNode(int layer, int cell, NavFlag cellNavFlag, int parentLayer, int parentCell, NavFlag parentCellNavFlag, string transitionId, int cost, int queryId) {
        Layer = layer;
        Cell = cell;
        CellNavFlag = cellNavFlag;
        ParentLayer = parentLayer;
        ParentCell = parentCell;
        ParentCellNavFlag = parentCellNavFlag;
        TransitionId = transitionId;
        Cost = cost;
        QueryId = queryId;
    }

    public static PathNode CreateOrigin(int layer, int cell, NavFlag cellNavFlag, int queryId) {
        return new PathNode(layer, cell, cellNavFlag, -1, -1, NavFlag.Null, "", 0, queryId);
    }

    public PathNode CreateChild(int childLayer, int childCell, NavFlag childNavFlag, string transitionId, int newCost, int queryId) {
        return new PathNode(childLayer, childCell, childNavFlag, Layer, Cell, CellNavFlag, transitionId, newCost, queryId);
    }
}