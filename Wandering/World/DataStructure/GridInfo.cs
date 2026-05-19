using UnityEngine;

namespace Wandering.World.DataStructure;

public readonly struct GridInfo {
    public readonly int Width;
    public readonly int Height;
    public readonly int NumLayers;
    public readonly int CellsPerLayer;
    
    public GridInfo(int layer, int width, int height) {
        Width = width;
        Height = height;
        NumLayers = layer;
        CellsPerLayer = Width * Height;
    }
    
    public int OffsetCell(int cell, Vector2Int offset) {
        Vector2Int cellCoords = CellToXY(cell);
        cellCoords += offset;

        return XYToCell(cellCoords.x, cellCoords.y);
    }

    #region Is_
    public bool IsCellValid(int cell) {
        return cell >= 0 && cell < Width *  Height;
    }

    public bool IsCellHorizontalMax(int cell) {
        return cell % Width == Width - 1;
    }

    public bool IsCellHorizontalMin(int cell) {
        return cell % Width == 0;
    }

    public bool IsCellVerticalMax(int cell) {
        return cell / Width == Height;
    }

    public bool IsCellVerticalMin(int cell) {
        return cell / Width == 0;
    }

    public bool IsOffsetValid(int cell, Vector2Int offset) {
        Vector2Int cellCoords = CellToXY(cell);
        cellCoords += offset;

        if (cellCoords.x < 0) { return false; }
        if (cellCoords.x >= Width) { return false; }
        if (cellCoords.y < 0) { return false; }
        if (cellCoords.y >= Height) { return false; }
        return true;
    }

    public bool IsOffsetValid(int layer, int cell, int layerOffset, Vector2Int offset, out int offsetLayer, out int offsetCell) {
        offsetCell = -1;
        offsetLayer = layer + layerOffset;
        if (offsetLayer < 0 || offsetLayer >= NumLayers) {
            return false; 
        }
        
        Vector2Int cellCoords = CellToXY(cell);
        cellCoords += offset;

        if (cellCoords.x < 0) { return false; }
        if (cellCoords.x >= Width) { return false; }
        if (cellCoords.y < 0) { return false; }
        if (cellCoords.y >= Height) { return false; }

        offsetCell = OffsetCell(cell, offset);
        return true;
    }
    #endregion
    
    #region CellTo_
    public Vector2Int CellToXY(int cell) {
        return new Vector2Int(cell % Width, cell / Width);
    }

    public Vector3 CellToPos(int layer, int cell) {
        return new Vector3(cell % Width + 0.5f, (int)(cell / Width) + 0.5f, LayerToZ(layer));
    }

    public Vector3 CellToPos(int cell) {
        return new Vector3(cell % Width + 0.5f, (int)(cell / Width) + 0.5f, 0f);
    }
    #endregion

    #region XXTo_
    public int XYToCell(int x, int y) {
        return x + y * Width;
    }
    #endregion

    #region LayerTo_

    public int LayerToZ(int layer) {
        return layer;
    }
    #endregion
    
    #region Cell_
    public int CellAbove(int cell) {
        return cell + Width;
    }

    public int CellBelow(int cell) {
        return cell - Width;
    }

    public int CellRight(int cell) {
        return cell + 1;
    }

    public int CellLeft(int cell) {
        return cell - 1;
    }
    #endregion
    
    #region PosTo_
    public int PosToLayer(Vector3 pos) {
        return 0;
    }
    
    public int PosToCell(Vector3 pos) {
        return 0;
    }
    #endregion
}