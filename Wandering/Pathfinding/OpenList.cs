using System.Collections.Generic;
using Wandering.Pathfinding.DataStructure;
using Wandering.Utils;

namespace Wandering.Pathfinding;

public class OpenList {
    public PriorityQueues.HotMinHeap<PathNode> MinHeap;
        
    public OpenList() {
        MinHeap = new ();
    }

    public int Count => MinHeap.Count;
        
    public void Enqueue(int priority, PathNode cell) {
        MinHeap.Enqueue(priority, cell);
    }

    public KeyValuePair<int, PathNode> Dequeue() {
        return MinHeap.Dequeue();
    }

    public OpenList Clear() {
        MinHeap.Clear();
        return this;
    }
}