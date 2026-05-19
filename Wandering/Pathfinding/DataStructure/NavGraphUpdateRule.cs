using Wandering.World;
using Wandering.World.DataStructure;

namespace Wandering.Pathfinding.DataStructure;

public abstract class NavGraphUpdateRule {
    private bool IsClear(WorldContext context, int layer, int cel) {
        if (!context.GridInfo.IsCellValid(cel)) {
            return false;
        }
        
        int cellAbove = context.GridInfo.CellAbove(cel);
        if (!context.GridInfo.IsCellValid(cellAbove)) {
            return false; 
        }

        return !context.BuildData.HasFlag(layer, cel, BuildFlag.Block) && 
               !context.BuildData.HasFlag(layer, cellAbove, BuildFlag.Block);
    }
    
    public abstract void Update(WorldContext context, NavGraph navGraph, int layer, int cell);

    public class FloorUpdateRule : NavGraphUpdateRule {
        public override void Update(WorldContext context, NavGraph navGraph, int layer, int cell) {
            bool isFloor = IsClear(context, layer, cell) && IsAnchored(context, layer, cell);
            navGraph.SetFlag(layer, cell, NavFlag.Floor, isFloor);
        }

        private bool IsAnchored(WorldContext context, int layer, int cell) {
            int cellBelow = context.GridInfo.CellBelow(cell);
            if (!context.GridInfo.IsCellValid(cellBelow)) {
                return false;
            }
            return context.BuildData.HasFlag(layer, cellBelow, BuildFlag.Block);
        }
    }

    public class LadderUpdateRule : NavGraphUpdateRule {
        public override void Update(WorldContext context, NavGraph navGraph, int layer, int cell) {
            bool isLadder = IsClear(context, layer, cell) && HasLadder(context, layer, cell);
            navGraph.SetFlag(layer, cell, NavFlag.Ladder, isLadder);
        }

        public bool HasLadder(WorldContext context, int layer, int cell) {
            int cellAbove = context.GridInfo.CellAbove(cell);
            return context.BuildData.HasFlag(layer, cell, BuildFlag.Ladder) &&
                   context.BuildData.HasFlag(layer, cellAbove, BuildFlag.Ladder);
            
        }
    }

    public class StairUpdateRule : NavGraphUpdateRule {
        public override void Update(WorldContext context, NavGraph navGraph, int layer, int cell) {
            bool isStair = IsClear(context, layer, cell) && HasStair(context, layer, cell);
            navGraph.SetFlag(layer, cell, NavFlag.Stairs, isStair);
        }

        public bool HasStair(WorldContext context, int layer, int cell) {
            return context.BuildData.HasFlag(layer, cell, BuildFlag.Stairs); 
        }
    }
}