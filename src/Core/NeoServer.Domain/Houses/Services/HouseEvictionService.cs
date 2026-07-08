using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Services;

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
        movementService.MoveCreature(player, exit, forced: true, isTeleport: true);
    }
}
