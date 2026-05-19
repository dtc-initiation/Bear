using System.Collections.Generic;
using UnityEngine;
using Wandering.Utils;
using Wandering.World;

namespace Wandering.Pathfinding.DataStructure;

public class NavGraph {
    private readonly int _maxLinksPerCell;
    private readonly int _cellsPerLayer;
    private readonly CellStore<NavFlag> _navFlags;
    private readonly NavLink[] _navLinks;
    private readonly NavConfig _navConfig;
    private readonly NavGraphUpdateRule[] _navGraphUpdateRules;

    public NavGraph(WorldContext context, int maxLinksPerCell, NavConfig navConfig) {
        _maxLinksPerCell = maxLinksPerCell;
        _cellsPerLayer = context.GridInfo.CellsPerLayer;
        _navFlags = new CellStore<NavFlag>(context.GridInfo.NumLayers, context.GridInfo.Width, context.GridInfo.Height);
        _navLinks = new NavLink[context.GridInfo.NumLayers * context.GridInfo.CellsPerLayer * _maxLinksPerCell];
        _navGraphUpdateRules = new NavGraphUpdateRule[] {
            new NavGraphUpdateRule.FloorUpdateRule(),
            new NavGraphUpdateRule.LadderUpdateRule(),
            new NavGraphUpdateRule.StairUpdateRule()
        };
        _navConfig = navConfig;
    }

    public void BuildGraph(WorldContext context) {
        for (int layer = 0; layer < context.GridInfo.NumLayers; layer++) {
            for (int cell = 0; cell < context.GridInfo.CellsPerLayer; cell++) {
                UpdateCell(context, layer, cell);
            }
        }
    }
    
    public bool HasFlag(int layer, int cellNum, NavFlag flag) {
        NavFlag currentFlag = _navFlags.Get(layer, cellNum);
        return (currentFlag & flag) != 0;
    }

    public void SetFlag(int layer, int cellNum, NavFlag flag, bool value) {
        NavFlag currentFlag = _navFlags.Get(layer, cellNum);
        _navFlags.Set(layer, cellNum, value ? currentFlag | flag : currentFlag & ~flag);
    }

    public IEnumerable<NavLink> GetLinkAt(int layer, int cell, NavFlag flag) {
        int linkIndex = (layer * _cellsPerLayer + cell) * _maxLinksPerCell;
        for (int i = 0; i < _maxLinksPerCell; i++) {
            NavLink link = _navLinks[linkIndex + i];

            if (link.NavCost == -1) {
                yield break;
            }

            if (link.StartNavFlag == flag) {
                yield return link;
            }
        }
    }

    private void UpdateCell(WorldContext context, int layer, int cell) {
        UpdateValidFlags(context, layer, cell);
        UpdateCellLinks(context, layer, cell);
    }
    
    private void UpdateValidFlags(WorldContext context, int layer, int cell) {
        foreach (NavGraphUpdateRule rule in _navGraphUpdateRules) {
            rule.Update(context, this, layer, cell);
        }
    }

    private void UpdateCellLinks(WorldContext context, int layer, int cell) {
        int linkIndex = (layer * _cellsPerLayer + cell) * _maxLinksPerCell;
        int numLinks = 0;
        foreach (NavTransition transition in _navConfig.transitions) {
            if (numLinks >= _maxLinksPerCell - 1) {
                break;
            }
            Vector2Int targetLayerCell = transition.IsValid(context, this, layer, cell);
            if (targetLayerCell.x != -1 && targetLayerCell.y != -1) {
                _navLinks[linkIndex] = new NavLink(
                    targetLayerCell.x, 
                    targetLayerCell.y,
                    transition.startNavFlag,
                    transition.endNavFlag, 
                    transition.navCost,
                    transition.TransitionId
                    );
                linkIndex++;
                numLinks++;
            }
        }
        _navLinks[linkIndex] = NavLink.Sentinel;
    }
}