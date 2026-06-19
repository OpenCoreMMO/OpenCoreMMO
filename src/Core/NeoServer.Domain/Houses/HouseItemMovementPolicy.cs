using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.World.Tiles;

namespace NeoServer.Domain.Houses;

/// <summary>
/// Movement policy for house tiles.
/// Rules:
/// <list type="bullet">
///   <item>Non-house tile or null tile: unrestricted (return true).</item>
///   <item>House tile with null player: blocked (return false).</item>
///   <item>House tile with a player: allowed only if the player is invited.</item>
/// </list>
/// DI note: wire <see cref="IHouseItemMovementPolicy"/> → <see cref="HouseItemMovementPolicy"/>
/// at the movement-layer registration (Phase 2/3); do not invoke movement commands from Phase 1.
/// </summary>
public class HouseItemMovementPolicy(IHouseStore houseStore) : IHouseItemMovementPolicy
{
    public bool CanMoveItem(IPlayer player, ITile tile)
    {
        if (tile is null) return true;           // no tile context — don't block

        var house = houseStore.GetByTile(tile);
        if (house is null) return true;          // not a house tile — unrestricted

        if (player is null) return false;        // house tile requires an acting player
        return house.IsInvited(player);
    }
}
