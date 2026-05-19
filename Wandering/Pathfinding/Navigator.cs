using System;
using System.Collections.Generic;
using System.Linq;
using BearCore;
using UnityEngine;
using Wandering.Pathfinding.DataStructure;
using Wandering.Pathfinding.Events;
using Wandering.World;

namespace Wandering.Pathfinding;

public interface IApproachable {
    int Layer { get; }
    int Cell { get; }
    Vector2Int ApproachOffsets { get; }
    int[] ApproachCells { get; }
}

public enum NavigationStatus {
    Idle,
    Navigating,
    Arrived,
    Failed
}

public class Navigator {
    private readonly WorldContext _worldContext;
    private readonly PathFinder _pathFinder;
    private readonly PathProber _pathProber;
    private readonly PathGrid _pathGrid;
    private readonly NavGraph _navGraph;
    private readonly Transform _navTransform;
    
    
    private PathQuery _pathQuery;
    private List<PathStep> _currentPath;
    private bool _probeValid;
    private int _currentLayer;
    private int _currentCell;
    private NavFlag _currentNavFlag;
    
    private IApproachable? _navTarget;
    private int _navTargetLayer;
    private int _navTargetCell;
    private NavigationStatus _navStatus;
    
    public NavigationStatus Status => _navStatus;
    public int CurrentCell => _currentCell;
    public NavFlag CurrentNavFlag => _currentNavFlag;
    public List<PathStep> CurrentPath => _currentPath;
    
    public Navigator(Transform transform, WanderingWorld? wanderingWorld) {
        if (wanderingWorld == null) {
            throw new InvalidOperationException("Navigator: World not initialized");
        }
        
        _navTransform = transform;
        _worldContext = wanderingWorld.WorldContext;
        _navGraph = wanderingWorld.NavGraph;
        _pathGrid = new PathGrid(
            _worldContext.GridInfo.NumLayers,
            _worldContext.GridInfo.Width,
            _worldContext.GridInfo.Height
        );
        _pathFinder = new PathFinder(_pathGrid);
        _pathProber = new PathProber();
        _navStatus = NavigationStatus.Idle;
        _currentLayer = _worldContext.GridInfo.PosToLayer(transform.position);
        _currentCell = _worldContext.GridInfo.PosToCell(transform.position);
        _currentNavFlag = NavFlag.Floor;
        _currentPath = new List<PathStep>();
        
        EventBus.Subscribe<PathProbeCompleted>(OnPathProbeComplete);
    }
    
    public void OnPathProbeComplete(PathProbeCompleted evt) {
        _probeValid = evt.IsProbeValid;
    }

    public void GoTo(IApproachable target) {
        SetTarget(target);
        GoTo(target.Layer, target.Cell, target.ApproachCells);
    }

    private void SetTarget(IApproachable target) {
        _navTarget = target;
        _navStatus = NavigationStatus.Navigating;
    }
    
    public void GoTo(int targetLayer, int targetCell, int[] approachCells) {
        if (TryComputeBestCell(targetLayer, targetCell, approachCells, out int bestCell)) {
            _navTargetLayer = targetLayer;
            _navTargetCell = bestCell;
            _pathQuery = new PathQuery(_currentLayer,  _currentCell, _currentNavFlag, _navTargetLayer, _navTargetCell);
            UpdateNavigation();
            return;
        };
        _navStatus = NavigationStatus.Failed;
    }

    private bool TryComputeBestCell(int targetLayer, int targetCell, int[] approachCells, out int bestCell) {
        var approachList = approachCells.ToList();
        approachList.Add(targetCell);
        bestCell = -1;
        
        int bestCost = int.MaxValue;
        foreach (int candidateCell in approachList) {
            var cost = GetNavigationCost(targetLayer, candidateCell);
            if ((cost != -1) && (cost < bestCost)) {
                bestCost = cost;
                bestCell = candidateCell;
            }
        }

        if (bestCell == -1) {
            return false;
        }
        return true;
    }
    
    public void UpdateNavigation() {
        // Check target reachability
        var canReach = GetNavigationCost(_navTargetLayer, _navTargetCell) != -1;
        if (!canReach) {
            Stop();
            _navStatus = NavigationStatus.Failed;
            return;
        }
        
        // Get current cell
        _currentLayer = _worldContext.GridInfo.PosToLayer(_navTransform.position);
        _currentCell = _worldContext.GridInfo.PosToCell(_navTransform.position);

        // Check if arrived
        var cellArrived = _currentCell == _navTargetCell;
        var layerArrived = _currentLayer == _navTargetLayer;
        if (cellArrived && layerArrived) {
            Stop();
            _navStatus = NavigationStatus.Arrived;
            return;
        }
        
        // Validate Path
        if (!ValidateCurrentPath()) {
            RebuildPath();
        }
    }

    private bool ValidateCurrentPath() {
        if (_currentPath.Count < 1) {
            return false;
        }
        
        bool pathValid = ValidatePathLinks(_currentPath);
        return pathValid;
    }

    private bool ValidatePathLinks(List<PathStep> path) {
        var tmpLayer = _currentLayer;
        var tmpCell = _currentCell;
        var tmpNavFlag = _currentNavFlag;
        
        foreach (var pathStep in path) {
            bool linkExists = false;
            foreach (NavLink link in _navGraph.GetLinkAt(tmpLayer, tmpCell, tmpNavFlag)) {
                linkExists = link.IsTargetValid(pathStep.TargetLayer,  pathStep.TargetCell, pathStep.TargetNavFlag);
                if (linkExists) {
                    break;
                }
            }

            if (!linkExists) {
                return false;
            }
            
            tmpLayer = pathStep.TargetLayer;
            tmpCell = pathStep.TargetCell;
            tmpNavFlag = pathStep.TargetNavFlag;
        }
        return true;
    }

    private void RebuildPath() {
        if (!TryBuildFromCache(out List<PathStep> pathFromCache)) {
            var pathResult = _pathFinder.FindPath(_pathQuery, _navGraph);
            if (pathResult.Success) {
                _currentPath = pathResult.ResultPath;
            } else {
                _navStatus = NavigationStatus.Failed;
            }
        } else {
            _currentPath = pathFromCache;
        }
    }

    private bool TryBuildFromCache(out List<PathStep> path) {
        path = _pathGrid.BuildPath(_pathQuery, _currentNavFlag);
        if (path.Count == 0) {
            return false;
        }

        if (!ValidatePathLinks(path)) {
            path.Clear();
            return false;
        }

        return true;
    }

    public void UpdateNavFlag(NavFlag flag) {
        _currentNavFlag = flag;
    }
    
    public void Cancel() {
        Stop();
    }
    
    private void Stop() {
        _navTarget = null;
        _currentPath.Clear();
        _navStatus = NavigationStatus.Idle;
    }

    public int GetNavigationCost(int layer, int cell) {
        return _pathGrid.GetProbeCost(layer, cell);
    }

    public PathStep PopStep() {
        var nextStep = _currentPath[0];
        _currentPath.RemoveAt(0);
        return nextStep;
    }

}