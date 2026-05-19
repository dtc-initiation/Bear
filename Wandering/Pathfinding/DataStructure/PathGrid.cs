using System;
using System.Collections.Generic;

namespace Wandering.Pathfinding.DataStructure;

public class PathGrid {
    private readonly PathNode[] _pathNodes;
    private readonly ProbeNode[] _probeNodes;
    private readonly int _numNavTypes;
    private int _serialNumber;
    private int _nodesPerLayer;
    
    public PathGrid(int numLayers, int width, int height) {
        _nodesPerLayer = width * height;
        _numNavTypes = Enum.GetValues(typeof(NavFlag)).Length;
        _pathNodes = new PathNode[numLayers * _nodesPerLayer * _numNavTypes];
        _probeNodes = new ProbeNode[numLayers * _nodesPerLayer];
    }

    public int NewSerialNumber() {
        _serialNumber++;
        return _serialNumber;
    }

    private int IndexState(int layer, int cell, NavFlag nodeFlag) {
        return ((layer * _nodesPerLayer + cell) * _numNavTypes) + nodeFlag.Index();
    }

    private int IndexProbe(int layer, int cell) {
        return layer * _nodesPerLayer + cell;
    }
    
    public bool TryGetPathNode(int layer, int cell, NavFlag flag, out PathNode node) {
        node = _pathNodes[IndexState(layer, cell, flag)];
        return node.QueryId == _serialNumber;
    }

    public bool TryGetProbeNode(int layer, int cell, out ProbeNode node) {
        node = _probeNodes[IndexProbe(layer, cell)];
        return node.QueryId == _serialNumber;
    }

    public void SetNode(PathNode node) {
        _pathNodes[IndexState(node.Layer, node.Cell, node.CellNavFlag)] = node;
    }

    public void SetProbe(ProbeNode node) {
        _probeNodes[IndexProbe(node.Layer, node.Cell)] = node;
    }

    public int GetCost(int layer, int cell, NavFlag flag) {
        bool idMatch = TryGetPathNode(layer, cell, flag, out PathNode node);
        return idMatch ? node.Cost : -1;
    }

    public int GetProbeCost(int layer, int cell) {
        bool idMatch = TryGetProbeNode(layer, cell, out ProbeNode node);
        return idMatch ? node.Cost : -1;
    }
    
    public List<PathStep> BuildPath(PathQuery query, NavFlag flag) {
        var steps = new List<PathStep>();
        if (!TryGetPathNode(query.TargetLayer, query.TargetCell, flag, out PathNode node)) {
            return steps;
        }

        while ((node.Cell != query.StartingCell) && (node.Layer != query.StartingLayer)) {
            var step = new PathStep(node.Layer, node.Cell, node.CellNavFlag, node.TransitionId);
            steps.Add(step);
            
            bool layerBroken = node.ParentLayer == -1;
            bool cellBroken = node.Cell == -1;
            bool broken = !TryGetPathNode(node.ParentLayer, node.ParentCell, node.ParentCellNavFlag, out node);
            if (layerBroken || cellBroken || broken) {
                steps.Clear();
                return steps;
            }
        }
        steps.Reverse();
        return steps;
    }
}