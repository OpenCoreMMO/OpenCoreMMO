using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location;

namespace NeoServer.Domain.Common.Contracts.World;

public delegate void RemoveThing(IThing thing, INormalTile tile, byte stackPosition);

public interface INormalTile : ITile
{
    bool HandlesCollision { get; }

    IEnumerable<IItem> ItemsWithCollision { get; }

    bool HandlesSeparation { get; }

    IEnumerable<IItem> ItemsWithSeparation { get; }

    new Location.Structs.Location Location { get; }

    byte Flags { get; }

    bool IsHouse { get; }

    bool BlocksThrow { get; }

    bool BlocksPass { get; }

    bool BlocksLay { get; }

    IItem Ground { get; }

    IEnumerable<uint> CreatureIds { get; }

    IEnumerable<IItem> TopItems1 { get; }

    IEnumerable<IItem> TopItems2 { get; }

    IEnumerable<IItem> DownItems { get; }

    bool CannotLogout { get; }
    uint GroundStepSpeed { get; }
    bool HasAnyFloorDestination { get; }
    bool HasCollision { get; }
    InvalidOperation PathError { get; }

    void AddThing(ref IThing thing, byte count = 1);

    void RemoveThing(ref IThing thing, byte count = 1);

    IThing GetThingAtStackPosition(byte stackPosition);

    byte GetStackPositionOfThing(IThing thing);

    void SetFlag(TileFlag flag);

    bool HasThing(IThing thing, byte count = 1);

    IItem BruteFindItemWithId(ushort typeId);

    IItem BruteRemoveItemWithId(ushort id);

    bool CanBeWalked(byte avoidDamageType = 0);

    bool HasFloorDestination(FloorChangeDirection direction);
}