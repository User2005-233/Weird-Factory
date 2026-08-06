using UnityEngine;
using System.Collections.Generic;

public class GridManager: MonoSingleton<GridManager>
{
    Dictionary<GridCoord, GridCell> gridCells = new Dictionary<GridCoord, GridCell>();
    
    [SerializeField] int gridWidth = 20;
    [SerializeField] int gridHeight = 20;

    protected override void Awake()
    {
        base.Awake();
        EventBus.Subscribe<TestEvent>(OnTestEvent);
    }

    private void OnTestEvent(TestEvent obj)
    {
        Debug.Log($"TestEvent received with value: {obj.value}");
    }

    public bool IsInBound(GridCoord coord)  
    {
        return coord.x >= 0 && coord.x < gridWidth && coord.z >= 0 && coord.z < gridHeight;
    }

    public GridCell GetCell(GridCoord coord)
    {
        if (gridCells.TryGetValue(coord, out var cell))
        {
            return cell;
        }
        return new GridCell { gridCoord = coord, occupationType = GridCell.OccupationType.Empty };
    }

    public bool IsOccupied(GridCoord coord)
    {
        return gridCells.ContainsKey(coord);
    }

    public bool CanPlaceAt(GridCoord coord, BuildingDefinition definition)
    {
        for (int x = 0; x < definition.footprint.x; x++)
        {
            for (int z = 0; z < definition.footprint.y; z++)
            {
                GridCoord offset = new GridCoord(x, z);
                GridCoord target = coord + offset;
                if (!IsInBound(target) || IsOccupied(target))
                {
                    return false;
                }
            }
        }
        return true;
    }
    
    private GridCoord GetDirectionOffset(GridDirection direction)
    {
        return GridDirectionExtensions.ToOffset(direction);
    }

    public bool PlaceBuilding(GridCoord coord, BuildingDefinition definition, object? obj)
    {
        for (int x = 0; x < definition.footprint.x; x++)
        {
            for (int z = 0; z < definition.footprint.y; z++)
            {
                GridCoord offset = new GridCoord(x, z);
                GridCoord target = coord + offset;
                gridCells[target] = new GridCell { occupationType = GridCell.OccupationType.Building, building = obj };
            }
        }
        return true;
    }

    public void RemoveBuilding(GridCoord coord)
    {
        gridCells.Remove(coord);
    }
    
    public GridCoord GetNeighbour(GridCoord coord, GridDirection direction)
    {
        return coord + GetDirectionOffset(direction);
    }

    public bool CanPlaceBeltAt(GridCoord coord)
    {
        if (!IsInBound(coord)) return false;
        if (gridCells.TryGetValue(coord, out var cell))
            return cell.occupationType == GridCell.OccupationType.Empty;
        return true;
    }

    public void PlaceBelt(GridCoord coord, BeltSegment segment)
    {
        gridCells[coord] = new GridCell
        {
            gridCoord = coord,
            occupationType = GridCell.OccupationType.Belt,
            beltSegment = segment
        };
    }

    public void RemoveBelt(GridCoord coord)
    {
        gridCells.Remove(coord);
    }

    public Building GetBuildingAt(GridCoord coord)
    {
        if (gridCells.TryGetValue(coord, out var cell) &&
            cell.occupationType == GridCell.OccupationType.Building)
            return cell.building as Building;
        return null;
    }
}
