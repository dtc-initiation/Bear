using System;
using System.Threading;
using BearCore;
using UnityEngine;
using Wandering.Pathfinding.DataStructure;
using Wandering.Pathfinding.Events;

namespace Wandering.Pathfinding;

public class PathProber {
    private readonly OpenList _openPathNodeList;
    private readonly CancellationTokenSource _cts;

    public PathProber() {
        _cts = new CancellationTokenSource();
        _openPathNodeList = new();
    }

    public async Awaitable ProbePathAsync(ProbeQuery query, NavGraph navGraph, PathGrid pathGrid) {
        try {
            var token = _cts.Token;
            
            await Awaitable.BackgroundThreadAsync();

            token.ThrowIfCancellationRequested();
            ProbePath(query, navGraph, pathGrid);
            EventBus.Raise(new PathProbeCompleted(true));

            await Awaitable.MainThreadAsync();
            
        } catch (OperationCanceledException) {
            Debug.LogError("PathProber process canceled");
        } catch (Exception e) {
            Debug.LogException(e);
        }
    } 

    public void ProbePath(ProbeQuery query, NavGraph navGraph, PathGrid pathGrid) {
        var newSerialNumber = pathGrid.NewSerialNumber();
        var origin = PathNode.CreateOrigin(query.StartingLayer, query.StartingCell, query.StartingNavFlag, newSerialNumber);
        var originProbe = new ProbeNode(query.StartingLayer, query.StartingCell, 0, query.StartingNavFlag, newSerialNumber); 
        
        pathGrid.SetNode(origin);
        pathGrid.SetProbe(originProbe);
        _openPathNodeList.Clear();
        _openPathNodeList.Enqueue(origin.Cost, origin);

        while (_openPathNodeList.Count > 0) {
            var currentNode = _openPathNodeList.Dequeue().Value;
            var storedStateCost = pathGrid.GetCost(currentNode.Layer, currentNode.Cell, currentNode.CellNavFlag);
            if (currentNode.Cost > storedStateCost) {
                continue;
            }

            foreach (NavLink navLink in navGraph.GetLinkAt(currentNode.Layer, currentNode.Cell, currentNode.CellNavFlag)) {
                int newCost = currentNode.Cost + navLink.NavCost;
                int targetStateCost = pathGrid.GetCost(navLink.TargetLayer, navLink.TargetCell, navLink.EndNavFlag);

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

                int storedProbeCost = pathGrid.GetProbeCost(navLink.TargetLayer, navLink.TargetCell);
                if ((storedProbeCost == -1) || (newCost < storedProbeCost)) {
                    var childProbeNode = new ProbeNode(
                        navLink.TargetLayer,
                        navLink.TargetCell,
                        newCost,
                        navLink.EndNavFlag,
                        newSerialNumber
                        );
                    pathGrid.SetProbe(childProbeNode);
                }
                pathGrid.SetNode(childNode);
                
                _openPathNodeList.Enqueue(childNode.Cost, childNode);
            }
        }
        
    }
}