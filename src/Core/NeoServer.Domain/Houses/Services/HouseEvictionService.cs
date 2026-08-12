using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Services;
using NeoServer.Domain.Houses.Events;

namespace NeoServer.Domain.Houses.Services;

/// <summary>
///     Teleports a player out of a house to the exit position.
///     Used when ownership changes or a player is kicked.
/// </summary>
public class HouseEvictionService(ICreatureMovementService movementService) : IHouseEviction
{
    /// <summary>
    ///     Instantly moves the given player to the exit location,
    ///     bypassing pathfinding and collision checks.
    /// </summary>
    public void TeleportToExit(IPlayer player, Location exit)
    {
        var oldPosition = player.Location;
        if (!movementService.MoveCreature(player, exit, forced: true, isTeleport: true))
        {
            return;
        }

        EventAggregator.Invoke(new PlayerKickedFromHouseEvent(player, oldPosition, exit));
    }

    /// <summary>
    ///     Evicts every occupant who is no longer invited after a guest/subowner list change.
    ///     Door-list edits do not kick.
    /// </summary>
    public void KickUninvited(House house, uint listId)
    {
        if (listId != HouseListId.GuestList && listId != HouseListId.SubOwnerList)
        {
            return;
        }

        var exit = house.EntryPosition.GetValueOrDefault();

        foreach (var occupant in house.GetUninvitedOccupants())
        {
            TeleportToExit(occupant, exit);
        }
    }
}
