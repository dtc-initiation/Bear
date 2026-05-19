namespace Wandering.Utils;

public class CellStore<T> where T : struct {
    private readonly T[] _data;
    private readonly int _cellPerLayer;

    public CellStore(int numLayers, int width, int height) {
        _cellPerLayer = width * height;
        _data = new T[numLayers * _cellPerLayer];
    }

    public T Get(int layer, int cell) {
        return _data[_cellPerLayer * layer + cell];
    }

    public void Set(int layer, int cell, T val) {
        _data[_cellPerLayer * layer + cell] = val;
    }
    
}