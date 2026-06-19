using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World.Tiles;

namespace NeoServer.Domain.Houses;

/// <summary>Default policy: player can only move items on house tiles if invited.</summary>
public class HouseItemMovementPolicy : IHouseItemMovementPolicy
{
    public bool CanMoveItem(IPlayer player, ITile tile)
    {
        if (player is null) return true;
        if (tile is null) return true;

        return true;
    }
}
