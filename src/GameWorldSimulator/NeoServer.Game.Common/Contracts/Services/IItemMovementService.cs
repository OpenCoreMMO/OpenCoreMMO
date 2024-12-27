using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Results;

namespace NeoServer.Game.Common.Contracts.Services;

public interface IItemMovementService
{
    Result<OperationResultList<IItem>> Move(IPlayer player, IItem item, IHasItem from, IHasItem destination,
        byte amount,
        byte fromPosition, byte? toPosition);

    Result<OperationResultList<IItem>> Move(IItem item, IHasItem from, IHasItem destination,
        byte amount,
        byte fromPosition, byte? toPosition);
}