using Wandering.Pathfinding.DataStructure;
using Wandering.Utils;

namespace Wandering.Pathfinding;

public static class NavExtensions {
    public static int Index(this NavFlag flag) {
        return BitOperations.TrailingZeroCount((int) flag);
    }

    public static bool IsTargetValid(this NavLink link, int layer, int cell, NavFlag navFlag) {
        bool layerEqual = link.TargetLayer == layer;
        if (!layerEqual) {
            return false;
        }
        
        bool cellEqual = link.TargetCell == cell;
        if (!cellEqual) {
            return false;
        }
        
        bool flagEqual = link.EndNavFlag == navFlag;
        if (!flagEqual) {
            return false;
        }

        return true;
    }
    
}