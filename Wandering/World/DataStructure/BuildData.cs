using System;
using Wandering.Utils;

namespace Wandering.World.DataStructure;

[Flags]
public enum BuildFlag {
    Block = 1 << 0,
    Door = 1 << 1,
    Wall = 1 << 2,
    Ladder = 1 << 3,
    Stairs = 1 << 4
}

public class BuildData {
    private readonly CellStore<BuildFlag> _buildFlags;

    public BuildData(int numLayers, int width, int height) {
        _buildFlags = new CellStore<BuildFlag>(numLayers, width, height);
    }

    public bool HasFlag(int layer, int cellNum, BuildFlag flag) {
        BuildFlag currentFlag = _buildFlags.Get(layer, cellNum);
        return (currentFlag & flag) != 0;
    }

    public void SetFlag(int layer, int cellNum, BuildFlag flag, bool value) {
        BuildFlag currentFlag = _buildFlags.Get(layer, cellNum);
        _buildFlags.Set(layer, cellNum, value ? currentFlag | flag : currentFlag & ~flag);
    }
    
}