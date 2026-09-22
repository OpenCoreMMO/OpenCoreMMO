using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World.Tiles;

namespace NeoServer.Domain.Houses;

/// <summary>Gates item throw/move operations on house tiles.</summary>
public interface IHouseItemMovementPolicy
{
    bool CanMoveItem(IPlayer player, ITile tile);
}
