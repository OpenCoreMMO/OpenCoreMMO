using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Common.Contracts.Items.Types.Containers;

public interface IDepot : IContainer
{
    bool IsAlreadyOpened { get; }
    void SetAsOpened(IPlayer openedBy);
    bool CanBeOpenedBy(IPlayer player);
}