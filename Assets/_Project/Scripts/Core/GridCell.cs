

public class GridCell
{
    public enum OccupationType {
        Empty,
        Building,
        Belt
    }

    public GridCoord gridCoord;
    public OccupationType occupationType;
    public object? building;
    public object? beltSegment;
}