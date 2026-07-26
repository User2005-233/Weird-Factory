using UnityEngine;
using System.Collections.Generic;

public class GridManager: MonoSingleton<GridManager>
{
    Dictionary<GridCoord, GridCell> gridCells = new Dictionary<GridCoord, GridCell>();
    
    int gridWidth = 20;
    int gridHeight = 20;
    
    public bool IsInBound(GridCoord coord)
    {
        return coord.x >= 0 && coord.x < gridWidth && coord.z >= 0 && coord.z < gridHeight;
    }

    public GridCell GetCell(GridCoord coord)
    {
        return gridCells[coord];
    }

    public bool IsOccupied(GridCoord coord)
    {
        return gridCells[coord].occupationType != GridCell.OccupationType.empty;
    }

    public bool CanPlaceAt(GridCoord coord)
    {
        return IsInBound(coord) && !IsOccupied(coord);
    }

}
