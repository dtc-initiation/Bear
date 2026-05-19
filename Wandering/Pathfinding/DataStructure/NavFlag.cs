using System;

namespace Wandering.Pathfinding.DataStructure;

[Flags]
public enum NavFlag {
    Floor = 1,
    Ladder = 1 << 1,
    Stairs = 1 << 2,
    Empty = 1 << 3,
    Null = 1 << 4
}