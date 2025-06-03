using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items.Types.Containers;

namespace NeoServer.Domain.Common.Contracts.Items;

public interface ILootContainer : IContainer
{
    ILoot Loot { get; }
    bool LootCreated { get; }

    bool CanBeOpenedBy(IPlayer player);
    void MarkAsLootCreated();
}