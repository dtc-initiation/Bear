using UnityEngine;
using Wandering.Pathfinding;
using Wandering.Pathfinding.DataStructure;

namespace Wandering.World;

public class WanderingWorld : MonoBehaviour {
    [SerializeField] private int numLayers = 3;
    [SerializeField] private int width = 24;
    [SerializeField] private int height = 42;
    [SerializeField] private int maxLinksPerCell = 32;
    [SerializeField] private NavConfig navConfig = null!;

    public int NumLayers => numLayers;
    public int CellsPerLayer => width * height;

    public WorldContext WorldContext = null!;
    public NavGraph NavGraph = null!;
    
    public void InitializeWorld() {
        WorldContext = new WorldContext(numLayers, width, height);
        NavGraph = new NavGraph(WorldContext, maxLinksPerCell, navConfig);

    }
    
    
}