using System;
using System.Collections.Generic;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Services;

namespace NeoServer.Extensions.Items.Doors;

public class LevelDoor : Door
{
    public LevelDoor(IItemType metadata, Location location, IDictionary<ItemTypeAttribute, IConvertible> attributes) :
        base(metadata, location, attributes)
    {
    }

    public override void Use(IPlayer usedBy)
    {
        Metadata.Attributes.TryGetAttribute(ItemTypeAttribute.LevelDoor, out _);
        Attributes.TryGetAttribute(ItemAttribute.ActionId, out int actionId);

        if (usedBy.Level < actionId - 1000)
        {
            OperationFailService.Send(usedBy.CreatureId, "Only the worthy may pass.");
            return;
        }

        var directionTo = Location.DirectionTo(usedBy.Location, true);

        if (!Metadata.Attributes.TryGetAttribute<string>("orientation", out var doorOrientation)) return;

        Teleport(usedBy, doorOrientation, directionTo);
    }

    private void Teleport(IPlayer player, string doorOrientation, Direction directionTo)
    {
        if (doorOrientation is "top" or "bottom") TeleportNorthOrSouth(player, directionTo);

        if (doorOrientation is "left" or "right") TeleportEastOrWest(player, directionTo);
    }

    private void TeleportEastOrWest(IPlayer player, Direction directionTo)
    {
        Console.WriteLine(directionTo);
        if (directionTo is Direction.East or Direction.SouthEast or Direction.NorthEast)
            player.TeleportTo((ushort)(Location.X - 1), Location.Y, Location.Z);

        if (directionTo is Direction.West or Direction.NorthWest or Direction.SouthWest)
            player.TeleportTo((ushort)(Location.X + 1), Location.Y, Location.Z);
    }

    private void TeleportNorthOrSouth(IPlayer player, Direction directionTo)
    {
        if (directionTo is Direction.South or Direction.SouthEast or Direction.SouthWest)
            player.TeleportTo(Location.X, (ushort)(Location.Y - 1), Location.Z);

        if (directionTo is Direction.North or Direction.NorthEast or Direction.NorthWest)
            player.TeleportTo(Location.X, (ushort)(Location.Y + 1), Location.Z);
    }

    public override string GetLookText(
        bool isClose = false, bool showInternalDetails = false)
    {
        Attributes.TryGetAttribute(ItemAttribute.ActionId, out int actionId);

        var minLevel = Math.Max(0, actionId - 1000);

        return minLevel == 0
            ? "You see a gate of expertise for any level."
            : $"You see a gate of expertise for level {minLevel}.\nOnly the worthy may pass.";
    }

    public new static bool IsApplicable(IItemType type)
    {
        return Door.IsApplicable(type) && type.Attributes.HasAttribute(ItemTypeAttribute.LevelDoor);
    }
}