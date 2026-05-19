using System.Collections.Generic;

namespace Wandering.Pathfinding.DataStructure;

public readonly struct PathResult {
    public readonly bool Success;
    public readonly int TotalCost;
    public readonly List<PathStep> ResultPath;
    
    public PathResult(bool success, int totalCost, List<PathStep> resultPath) {
        Success = success;
        TotalCost = totalCost;
        ResultPath = resultPath;
    }
}