using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types.Containers;
using NeoServer.Domain.Common.Contracts.Items.Types.Usable;

namespace NeoServer.Domain.Common.Contracts.Services;

public interface IPlayerUseService
{
    void Use(IPlayer player, IItem item);
    void Use(IPlayer player, IUsableOn usableItem, IThing usedOn);
    void Use(IPlayer player, IContainer container, byte openAtIndex);
}