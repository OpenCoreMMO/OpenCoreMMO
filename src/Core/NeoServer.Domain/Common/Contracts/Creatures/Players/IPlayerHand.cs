using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Results;

namespace NeoServer.Domain.Common.Contracts.Creatures.Players;

public interface IPlayerHand
{
    Result<OperationResultList<IItem>> Move(IItem item, IHasItem from, IHasItem destination, byte amount,
        byte fromPosition, byte? toPosition);

    Result<OperationResultList<IItem>> PickItemFromGround(IItem item, ITile tile, byte amount = 1);
}