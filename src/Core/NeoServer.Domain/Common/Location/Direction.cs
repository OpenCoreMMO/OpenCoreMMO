namespace NeoServer.Domain.Common.Location;

public enum Direction : byte
{
    North = 0,
    East = 1,
    South = 2,
    West = 3,
    SouthWest,
    SouthEast,
    NorthWest,
    NorthEast,
    None = 255
}

public static class DirectionExtensions
{
    private const byte DIRECTION_MASK = 0b0000_0111;
    private const byte DRUNK_FLAG = 0b1000_0000;

    public static Direction MakeDrunk(this Direction dir)
    {
        return (Direction)((byte)dir | DRUNK_FLAG);
    }

    public static bool IsDrunk(this Direction dir)
    {
        return ((byte)dir & DRUNK_FLAG) != 0;
    }

    public static Direction GetOriginalDirection(this Direction dir)
    {
        return (Direction)((byte)dir & DIRECTION_MASK);
    }
} 