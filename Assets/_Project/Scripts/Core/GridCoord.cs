

public struct GridCoord
{
    public int x;
    public int z;
    
    public GridCoord(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    public static GridCoord Zero => new(0, 0);
    
    public static GridCoord operator +(GridCoord a, GridCoord b) => new(a.x + b.x, a.z + b.z);
    public static GridCoord operator -(GridCoord a, GridCoord b) => new(a.x - b.x, a.z - b.z);
    public static bool      operator ==(GridCoord a, GridCoord b) => a.x == b.x && a.z == b.z;
    public static bool      operator !=(GridCoord a, GridCoord b) => !(a == b);
    
    public override string ToString() => $"({x}, {z})";

    public bool IsSameColumn(GridCoord other) => x == other.x;
    public bool IsSameRow(GridCoord other) => z == other.z;

    public override bool Equals(object obj)
    {
        return obj is GridCoord coord && this == coord;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(x, z);
    }
}
