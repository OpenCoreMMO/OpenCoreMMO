using System;

namespace NeoServer.Game.Common.Location.Structs;

/// <summary>
///     Represents a 3D point with integer coordinates.
/// </summary>
/// <remarks>
///     It's important to note that our coordinate system is "cartesian-like".
///     That means that our origin is at (X=0, Y=0, Z=0).
///     A positive <see cref="X" /> means that the coordinate is to the west of the origin, while
///     a negative <see cref="X" /> that the coordinate is to the east of the origin.
///     A positive <see cref="Y" /> means that the coordinate is to the north of the origin, while
///     a negative <see cref="Y" /> means that the coordinate is to the south of the origin.
///     A zero <see cref="Z" /> means that the coordinate is on the ground fllor,
///     while a positive <see cref="Z" /> means that the coordinate is above
///     the ground floor and a negative <see cref="Z" /> means that the coordinate
///     is underground.
///     This is different from TFS / OTX, where the X and Y values are positive only, with
///     the origin being on the top-left corner of the map.
///     Also, the Z coordinate of TFS / OTX must be [0, 15], with 7 representing the ground
///     floor.
///     Be careful when converting between the two systems (e.g.: when parsing a .otbm map).
/// </remarks>
public readonly struct Coordinate : IEquatable<Coordinate>
{
    /// <summary>
    ///     The x coordinate of this instance.
    /// </summary>
    public readonly int X;

    /// <summary>
    ///     The y coordinate of this instance.
    /// </summary>
    public readonly int Y;

    /// <summary>
    ///     The z coordinate of this instance, used to represent the "floor height".
    /// </summary>
    public readonly sbyte Z;

    /// <summary>
    ///     Creates a new instance of <see cref="Coordinate" />.
    /// </summary>
    /// <remarks>
    ///     It's important to note that our coordinate system is "cartesian-like".
    ///     That means that our origin is at (X=0, Y=0, Z=0).
    ///     A positive <see cref="X" /> means that the coordinate is to the west of the origin, while
    ///     a negative <see cref="X" /> that the coordinate is to the east of the origin.
    ///     A positive <see cref="Y" /> means that the coordinate is to the north of the origin, while
    ///     a negative <see cref="Y" /> means that the coordinate is to the south of the origin.
    ///     A zero <see cref="Z" /> means that the coordinate is on the ground fllor,
    ///     while a positive <see cref="Z" /> means that the coordinate is above
    ///     the ground floor and a negative <see cref="Z" /> means that the coordinate
    ///     is underground.
    ///     This is different from TFS / OTX, where the X and Y values are positive only, with
    ///     the origin being on the top-left corner of the map.
    ///     Also, the Z coordinate of TFS / OTX must be [0, 15], with 7 representing the ground
    ///     floor.
    ///     Be careful when converting between the two systems (e.g.: when parsing a .otbm map).
    /// </remarks>
    public Coordinate(int x, int y, sbyte z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public Coordinate(Location location)
    {
        X = location.X;
        Y = location.Y;
        Z = (sbyte)location.Z;
    }

    public Coordinate()
    {
        X = 0;
        Y = 0;
        Z = 0;
    }

    public Location Location => new((ushort)X, (ushort)Y, (byte)Z);

    /// <summary>
    ///     Creates a new <see cref="Coordinate" /> whose coordinates are equal to this instance's
    ///     plus the provided offsets.
    /// </summary>
    public Coordinate Translate(int xOffset, int yOffset, sbyte zOffset)
    {
        return new Coordinate(
            X + xOffset,
            Y + yOffset,
            (sbyte)(Z + zOffset));
    }

    /// <summary>
    ///     Returns a string representation of this instance.
    /// </summary>
    public override string ToString()
    {
        return $"{{X={X}, Y={Y}, Z={Z}}}";
    }

    /// <summary>
    ///     Returns true if the other <see cref="Coordinate" /> has the same <see cref="X" />,
    ///     <see cref="Y" /> and <see cref="Z" /> values as this one.
    ///     Returns false otherwise.
    /// </summary>
    public bool Equals(Coordinate other)
    {
        return X == other.X && Y == other.Y && Z == other.Z;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }

    public static Coordinate operator +(Coordinate location1, Coordinate location2)
    {
        return new Coordinate(
            location1.X + location2.X,
            location1.Y + location2.Y,
            (sbyte)(location1.Z + location2.Z)
        );
    }

    public static Coordinate operator -(Coordinate location1, Coordinate location2)
    {
        return new Coordinate(
            location2.X - location1.X,
            location2.Y - location1.Y,
            (sbyte)(location2.Z - location1.Z)
        );
    }
}