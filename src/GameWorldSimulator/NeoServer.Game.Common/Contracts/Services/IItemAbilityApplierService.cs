using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Results;

namespace NeoServer.Game.Common.Contracts.Services;

public interface IItemAbilityApplierService
{
    Result ApplyAbilities(IPlayer player, IItem item);
}