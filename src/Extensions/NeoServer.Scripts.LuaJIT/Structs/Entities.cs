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

    public struct PositionLua
    {
        public ushort x;
        public ushort y;
        public byte z;

        public PositionLua(ushort initX, ushort initY, byte initZ)
        {
            x = initX;
            y = initY;
            z = initZ;
        }

        public static bool AreInRange(PositionLua p1, PositionLua p2, int deltax, int deltay)
        {
            return GetDistanceX(p1, p2) <= deltax && GetDistanceY(p1, p2) <= deltay;
        }

        public static bool AreInRange(PositionLua p1, PositionLua p2, int deltax, int deltay, int deltaz)
        {
            return GetDistanceX(p1, p2) <= deltax && GetDistanceY(p1, p2) <= deltay && GetDistanceZ(p1, p2) <= deltaz;
        }

        public static int GetOffsetX(PositionLua p1, PositionLua p2)
        {
            return p1.x - p2.x;
        }

        public static int GetOffsetY(PositionLua p1, PositionLua p2)
        {
            return p1.y - p2.y;
        }

        public static int GetOffsetZ(PositionLua p1, PositionLua p2)
        {
            return p1.z - p2.z;
        }

        public static int GetDistanceX(PositionLua p1, PositionLua p2)
        {
            return Math.Abs(GetOffsetX(p1, p2));
        }

        public static int GetDistanceY(PositionLua p1, PositionLua p2)
        {
            return Math.Abs(GetOffsetY(p1, p2));
        }

        public static int GetDistanceZ(PositionLua p1, PositionLua p2)
        {
            return Math.Abs(GetOffsetZ(p1, p2));
        }

        public static int GetDiagonalDistance(PositionLua p1, PositionLua p2)
        {
            return Math.Max(GetDistanceX(p1, p2), GetDistanceY(p1, p2));
        }

        public static double GetEuclideanDistance(PositionLua p1, PositionLua p2)
        {
            int dx = GetDistanceX(p1, p2);
            int dy = GetDistanceY(p1, p2);
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public override string ToString()
        {
            return $"( {GetX()}, {GetY()}, {GetZ()} )";
        }

        public int GetX()
        {
            return x;
        }

        public int GetY()
        {
            return y;
        }

        public int GetZ()
        {
            return z;
        }
    }

    public static class PositionHasher
    {
        public static int GetHashCode(PositionLua p)
        {
            return (int)p.x | (p.y << 16) | (p.z << 32);
        }
    }

    public static class PositionExtensions
    {
        public static bool Equals(PositionLua p1, PositionLua p2)
        {
            return p1.x == p2.x && p1.y == p2.y && p1.z == p2.z;
        }

        public static PositionLua Add(PositionLua p1, PositionLua p2)
        {
            return new PositionLua((ushort)(p1.x + p2.x), (ushort)(p1.y + p2.y), (byte)(p1.z + p2.z));
        }

        public static PositionLua Subtract(PositionLua p1, PositionLua p2)
        {
            return new PositionLua((ushort)(p1.x - p2.x), (ushort)(p1.y - p2.y), (byte)(p1.z - p2.z));
        }
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

    public class ThingLua()
    {
        #region Members

        public PositionLua Position { get; set; }

        #endregion

        #region Properties

        #endregion

        #region Public Methods

        #endregion
    }

    public class CreatureLua : ThingLua
    {
        #region Constructors

        public CreatureLua(string name, PositionLua position)
        {
            SetId();
            Name = name; Position = position;
        }

        #endregion

        #region Properties

        public int Id { get; set; }
        public string Name { get; set; }

        #endregion

        #region Public Methods

        public virtual void SetId() { }

        public virtual CreatureLua GetPlayer() => null;
        public virtual CreatureLua GetNpc() => null;
        public virtual CreatureLua GetMonster() => null;

        #endregion
    }

    public class PlayerLua(int guid, string name, int level, PositionLua position) : CreatureLua(name, position)
    {
        #region Members

        #endregion

        #region Properties

        public int PlayerFirstID => 0x10000000;
        public int PlayerLastID => 0x50000000;

        public int Guid { get; set; } = guid;

        public int Level { get; set; } = level;

        #endregion

        #region Public Methods

        public override void SetId()
        {
            base.SetId();

            // guid = player id from database
            if (Id == 0 && Guid != 0)
            {
                Id = PlayerFirstID + guid;
                if (Id == int.MaxValue)
                {
                    //Logger.GetInstance().Error($"[SetId] Player {Name} has max 'id' value of uint32_t");
                }
            }
        }

        public override PlayerLua GetPlayer() => this;

        #endregion
    }

    public class ItemLua() : ThingLua
    {
        #region Members

        #endregion

        #region Properties

        public ushort Id { get; set; }
        public string Name { get; set; }

        #endregion

        #region Public Methods

        #endregion
    }
}
