using System;

namespace NeoServer.Game.Common.Location;

/// <summary>
///     This enum is meant to store in a memory-efficient way a collection
///     of properties that a Tile can have.
/// </summary>
[Flags]
public enum TileFlags : uint
{
    None = 0,
    FloorChangeDown = 1 << 0,
    FloorChangeNorth = 1 << 1,
    FloorChangeSouth = 1 << 2,
    FloorChangeEast = 1 << 3,
    FloorChangeWest = 1 << 4,
    FloorChangeSouthAlternative = 1 << 5,
    FloorChangeEastAlternative = 1 << 6,
    ProtectionZone = 1 << 7,
    NoPvpZone = 1 << 8,
    NoLogout = 1 << 9,
    PvpZone = 1 << 10,
    Teleport = 1 << 11,
    MagicField = 1 << 12,
    MailBox = 1 << 13,
    TrashHolder = 1 << 14,
    Bed = 1 << 15,
    Depot = 1 << 16,
    BLockSolid = 1 << 17,
    BlockPath = 1 << 18,
    ImmovableBlockSolid = 1 << 19,
    ImmovableBlockPath = 1 << 20,
    ImmovableNoFieldBlockPath = 1 << 21,
    NoFieldBlockPath = 1 << 22,
    SupportsHangable = 1 << 23,
    Moveable = 1 << 24,
    IsHorizontal = 1 << 25,
    IsVertical = 1 << 26,
    BlockProjecTile = 1 << 27,
    HasHeight = 1 << 28,

    FloorChange = FloorChangeNorth | FloorChangeDown | FloorChangeSouth | FloorChangeEast | FloorChangeWest |
                  FloorChangeEastAlternative | FloorChangeSouthAlternative
}