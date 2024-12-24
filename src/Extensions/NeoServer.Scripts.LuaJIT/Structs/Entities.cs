namespace NeoServer.Scripts.LuaJIT.Structs
{
    public enum DirectionType : byte
    {
        DIRECTION_NORTH = 0,
        DIRECTION_EAST = 1,
        DIRECTION_SOUTH = 2,
        DIRECTION_WEST = 3,

        DIRECTION_DIAGONAL_MASK = 4,
        DIRECTION_SOUTHWEST = DIRECTION_DIAGONAL_MASK | 0,
        DIRECTION_SOUTHEAST = DIRECTION_DIAGONAL_MASK | 1,
        DIRECTION_NORTHWEST = DIRECTION_DIAGONAL_MASK | 2,
        DIRECTION_NORTHEAST = DIRECTION_DIAGONAL_MASK | 3,

        DIRECTION_LAST = DIRECTION_NORTHEAST,
        DIRECTION_NONE = 8
    }

    public static class DirectionExtensions
    {
        private static readonly Dictionary<DirectionType, string> DirectionStrings = new Dictionary<DirectionType, string>
    {
        { DirectionType.DIRECTION_NORTH, "North" },
        { DirectionType.DIRECTION_EAST, "East" },
        { DirectionType.DIRECTION_WEST, "West" },
        { DirectionType.DIRECTION_SOUTH, "South" },
        { DirectionType.DIRECTION_SOUTHWEST, "South-West" },
        { DirectionType.DIRECTION_SOUTHEAST, "South-East" },
        { DirectionType.DIRECTION_NORTHWEST, "North-West" },
        { DirectionType.DIRECTION_NORTHEAST, "North-East" }
    };

        public static string ToDirectionString(this DirectionType dir)
        {
            if (DirectionStrings.TryGetValue(dir, out var result))
            {
                return result;
            }

            return dir.ToString();
        }
    }
}
