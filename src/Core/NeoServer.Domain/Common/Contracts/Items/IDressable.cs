using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Common.Contracts.Items;

public interface IDressable
{
    void DressedIn(IPlayer player);
    void UndressFrom(IPlayer player);
}