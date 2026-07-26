

public class GridCell
{
    public enum OccupationType {
        empty,
        building,
        belt
    }

    public GridCoord gridCoord;
    public OccupationType occupationType;
    public object? building;
}