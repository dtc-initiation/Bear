using System.Collections.Generic;
using Wandering.Pathfinding.DataStructure;
using Wandering.Utils;

namespace Wandering.Pathfinding;

public class PathFinder {
    private PathGrid _pathGrid;
    private OpenList _openPathNodeList;

    public PathFinder(PathGrid pathGrid) {
        _pathGrid = pathGrid;
        _openPathNodeList = new OpenList();
    }
    
    public PathResult FindPath(PathQuery query, NavGraph navGraph) {
        var newSerialNumber = _pathGrid.NewSerialNumber();
        var origin = PathNode.CreateOrigin(query.StartingLayer, query.StartingCell, query.StartingNavFlag, newSerialNumber);
        
        _pathGrid.SetNode(origin);
        _openPathNodeList.Clear();
        _openPathNodeList.Enqueue(origin.Cost, origin);

        while (_openPathNodeList.Count > 0) {
            var currentNode = _openPathNodeList.Dequeue().Value;
            var storedStateCost = _pathGrid.GetCost(currentNode.Layer, currentNode.Cell, currentNode.CellNavFlag);
            if (currentNode.Cost > storedStateCost) {
                continue;
            }

            if (query.IsMatch(currentNode.Layer, currentNode.Cell)) {
                var steps = _pathGrid.BuildPath(query, currentNode.CellNavFlag);
                return new PathResult(true, currentNode.Cost, steps);
            };

            foreach (NavLink navLink in navGraph.GetLinkAt(currentNode.Layer, currentNode.Cell, currentNode.CellNavFlag)) {
                int newCost = currentNode.Cost + navLink.NavCost;
                int targetStateCost = _pathGrid.GetCost(navLink.TargetLayer, navLink.TargetCell, navLink.EndNavFlag);

                if ((targetStateCost != -1) && (targetStateCost <= newCost)) {
                    continue;
                }

                PathNode childNode = currentNode.CreateChild(
                    navLink.TargetLayer,
                    navLink.TargetCell,
                    navLink.EndNavFlag,
                    navLink.TransitionId,
                    newCost, 
                    newSerialNumber
                    );
                
                _pathGrid.SetNode(childNode);
                _openPathNodeList.Enqueue(childNode.Cost, childNode);
            }
        }

        return new PathResult();
    }

    public int FindCost(PathQuery query, NavGraph graph) {
        return FindPath(query, graph).TotalCost;
    }
}