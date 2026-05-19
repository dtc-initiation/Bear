using System;
using System.Collections.Generic;
using UnityEngine;
using Wandering.Pathfinding.DataStructure;
using Wandering.World;

namespace Wandering.Pathfinding;

[CreateAssetMenu(fileName = "NavConfig", menuName = "Wandering/NavConfig")]
public class NavConfig : ScriptableObject {
    public string configName = null!;
    public List<NavTransition> transitions = null!;
}

[Serializable]
public struct NavTransition {
    public int layerDisplacement;
    public Vector2Int directionDisplacement;
    public NavFlag startNavFlag;
    public NavFlag endNavFlag;
    public int navCost;
    
    [NonSerialized] 
    public string TransitionId;

    public NavTransition(
        int layerDisplacement, 
        Vector2Int directionDisplacement,
        NavFlag startNavFlag,
        NavFlag endNavFlag,
        int navCost
    ) {
        this.layerDisplacement = layerDisplacement;
        this.directionDisplacement = directionDisplacement;
        this.startNavFlag = startNavFlag;
        this.endNavFlag = endNavFlag;
        this.navCost = navCost;
        this.TransitionId = $"{layerDisplacement.ToString()}_" +
                            $"{directionDisplacement.x.ToString()}_" +
                            $"{directionDisplacement.y.ToString()}_" +
                            $"{startNavFlag.ToString()}_" +
                            $"{endNavFlag.ToString()}";
    }

    public Vector2Int IsValid(WorldContext context, NavGraph navGraph, int layer, int cell) {
        Vector2Int targetLayerCell = new Vector2Int(-1, -1);
        if (context.GridInfo.IsOffsetValid(layer, cell, layerDisplacement, directionDisplacement, out int offsetLayer, out int offsetCell)) {
            if (navGraph.HasFlag(layer, cell, startNavFlag) && navGraph.HasFlag(offsetLayer, offsetCell, endNavFlag)) {
                targetLayerCell.x = offsetLayer;
                targetLayerCell.y = offsetCell;
            }
        };
        return targetLayerCell;
    }
    
}
