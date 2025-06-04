using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items.Types.Containers;
using NeoServer.Domain.Creatures.Monster.Loot;

namespace NeoServer.Domain.Common.Contracts.Items;

public interface ILootContainer : IContainer
{
    Loot Loot { get; }
    bool LootCreated { get; }

    bool CanBeOpenedBy(IPlayer player);
    void MarkAsLootCreated();
}