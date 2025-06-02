using System;
using System.Collections.Generic;
using NeoServer.Game.Common.Contracts;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types.Containers;
using NeoServer.Game.Common.Item;

namespace NeoServer.Game.Creatures.Events.Player;

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

    private void CreateLootItems(ILootItem[] items, IContainer container)
    {
        foreach (var item in items)
        {
            var attributes = new Dictionary<ItemAttribute, IConvertible>();

            if (item.Amount > 1) attributes.TryAdd(ItemAttribute.Count, item.Amount);

            var itemToDrop = itemFactory.Create(item.ItemType.ServerId, container.Location, attributes);

            if (itemToDrop is IContainer && item.Items?.Length == 0) continue;

            if (itemToDrop is IContainer c && item.Items?.Length > 0) CreateLootItems(item.Items, c);

            container?.AddItem(itemToDrop);
        }
    }
}