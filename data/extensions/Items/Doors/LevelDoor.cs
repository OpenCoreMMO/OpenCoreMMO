using System;
using System.Collections.Generic;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Common.Services;

namespace NeoServer.Extensions.Items.Doors;

public class LevelDoor : Door
{
    public LevelDoor(IItemType metadata, Location location, IDictionary<ItemAttribute, IConvertible> attributes) :
        base(metadata, location, attributes)
    {
    }

    public override void Use(IPlayer usedBy)
    {
        Metadata.Attributes.TryGetAttribute(ItemAttribute.LevelDoor, out _);

        Metadata.Attributes.TryGetAttribute(ItemAttribute.ActionId, out int actionId);

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
        Metadata.Attributes.TryGetAttribute(ItemAttribute.ActionId, out int actionId);

        var minLevel = Math.Max(0, actionId - 1000);

        return minLevel == 0
            ? "You see a gate of expertise for any level."
            : $"You see a gate of expertise for level {minLevel}.\nOnly the worthy may pass.";
    }

    public new static bool IsApplicable(IItemType type)
    {
        return Door.IsApplicable(type) && type.Attributes.HasAttribute(ItemAttribute.LevelDoor);
    }
}