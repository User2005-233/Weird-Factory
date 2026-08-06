

public enum GridDirection
{
    North = 0,
    East = 1,
    South = 2,
    West = 3
}

public static class GridDirectionExtensions{
    
    public static GridDirection Opposite(this GridDirection direction)
    {
        return direction switch
        {
            GridDirection.North => GridDirection.South,
            GridDirection.East => GridDirection.West,
            GridDirection.South => GridDirection.North,
            GridDirection.West => GridDirection.East,
            _ => GridDirection.North
        };
    }

    public static GridCoord ToOffset(this GridDirection direction)
    {
        return direction switch
        {
            GridDirection.North => new GridCoord(0, 1),
            GridDirection.East => new GridCoord(1, 0),
            GridDirection.South => new GridCoord(0, -1),
            GridDirection.West => new GridCoord(-1, 0),
            _ => new GridCoord(0, 0)
        };
    }

    public static GridDirection TurnLeft(this GridDirection direction)
    {
        return direction switch
        {
            GridDirection.North => GridDirection.West,
            GridDirection.East => GridDirection.North,
            GridDirection.South => GridDirection.East,
            GridDirection.West => GridDirection.South,
            _ => GridDirection.North
        };
    }

    public static GridDirection TurnRight(this GridDirection direction)
    {
        return direction switch
        {
            GridDirection.North => GridDirection.East,
            GridDirection.East => GridDirection.South,
            GridDirection.South => GridDirection.West,
            GridDirection.West => GridDirection.North,
            _ => GridDirection.North
        };
    }
}
