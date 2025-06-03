using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Results;

namespace NeoServer.Domain.Common.Contracts.Services;

public interface IItemMovementService
{
    Result<OperationResultList<IItem>> Move(IPlayer player, IItem item, IHasItem from, IHasItem destination,
        byte amount,
        byte fromPosition, byte? toPosition, bool walkTo = true);

    Result<OperationResultList<IItem>> Move(IItem item, IHasItem from, IHasItem destination,
        byte amount,
        byte fromPosition, byte? toPosition);
}