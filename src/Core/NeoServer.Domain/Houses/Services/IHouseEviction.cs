using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Houses.Services;

/// <summary>Teleports a player out of a house to the exit position.</summary>
public interface IHouseEviction
{
    void TeleportToExit(IPlayer player, Location exit);

    /// <summary>
    ///     After guest/subowner list changes, evicts every occupant no longer invited.
    ///     Door-list edits do not kick.
    /// </summary>
    void KickUninvited(House house, uint listId);
}
