using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Results;

namespace NeoServer.Domain.Common.Contracts.Services;

public interface IItemAbilityApplierService
{
    Result ApplyAbilities(IPlayer player, IItem item);
    Result RemoveAbilities(IPlayer player, IItem item);
}