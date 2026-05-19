namespace Wandering.Pathfinding.Events;

public readonly struct PathProbeCompleted {
    public readonly bool IsProbeValid;

    public PathProbeCompleted(bool isProbeValid) {
        IsProbeValid = isProbeValid;
    }
}