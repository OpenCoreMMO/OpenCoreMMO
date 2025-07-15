using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Creatures.Monster.Loot;

namespace NeoServer.Domain.Creatures.Events.Player;

public class PlayerOpenedContainerEventHandler(IItemFactory itemFactory) : IGameEventHandler
{
    public void Execute(IPlayer player, byte containerId, IContainer container)
    {
        if (container is not ILootContainer lootContainer || lootContainer.Loot is null) return;
        if (lootContainer.LootCreated) return;

        CreateLoot(lootContainer);
        lootContainer.MarkAsLootCreated();
    }

    private void CreateLoot(ILootContainer lootContainer)
    {
        CreateLootItems(lootContainer.Loot.Items, lootContainer);
    }

    private void CreateLootItems(LootItem[] items, IContainer container)
    {
        foreach (var item in items)
        {
            var attributes = new Dictionary<ItemTypeAttribute, IConvertible>();

            if (item.Amount > 1) attributes.TryAdd(ItemTypeAttribute.Count, item.Amount);

            var itemToDrop = itemFactory.Create(item.ItemType.ServerId, container.Location, attributes);

            if (itemToDrop is IContainer && item.Items?.Length == 0) continue;

            if (itemToDrop is IContainer c && item.Items?.Length > 0) CreateLootItems(item.Items, c);

            container?.AddItem(itemToDrop);
        }
    }
}