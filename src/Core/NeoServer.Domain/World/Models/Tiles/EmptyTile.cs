using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.World.Models.Tiles;

/// <summary>
/// Class representing an empty tile in the game world.
/// An empty tile contains no items or creatures.
/// </summary>
public class EmptyTile : BaseTile
{
    public EmptyTile(Location location)
    {
        SetNewLocation(location);
    }
    public override int ItemsCount => 0;
    public override IItem[] AllItems => [];
    public override IItem TopTopItemOnStack => null;
    public override IItem TopDownItemOnStack => null;
    public override ICreature TopCreatureOnStack => null;
    public override int ThingsCount => 0;

    public override bool TryGetStackPositionOfThing(IPlayer player, IThing thing, out byte stackPosition)
    {
        stackPosition = 0;
        return false;
    }

    public override byte GetCreatureStackPositionIndex(IPlayer observer)
    {
        return 0;
    }

    public override IItem GetItemByIndex(int index)
    {
        return null;
    }
}