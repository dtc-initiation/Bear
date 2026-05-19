using Wandering.World.DataStructure;

namespace Wandering.World;

public class WorldContext {
    public readonly GridInfo GridInfo;
    public readonly BuildData BuildData;

    public WorldContext(int numLayers, int width, int height) {
        GridInfo = new GridInfo(numLayers, width, height);
        BuildData = new BuildData(numLayers, width, height);
    }
}