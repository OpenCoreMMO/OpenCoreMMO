using System;
using System.Linq;
using System.Text;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Location.Structs;

namespace NeoServer.Game.Items.Items.Containers;

public class LootContainer : Container.Container, ILootContainer
{
    private readonly DateTime _createdAt;

    public LootContainer(IItemType type, Location location, ILoot loot) : base(type, location)
    {
        Loot = loot;
        _createdAt = DateTime.Now;
    }

    public ILoot Loot { get; }
    public bool LootCreated { get; private set; }

    public bool CanBeOpenedBy(IPlayer player)
    {
        return Allowed(player);
    }

    public void MarkAsLootCreated()
    {
        LootCreated = true;
    }

    private bool Allowed(IPlayer player)
    {
        if (Loot?.Owners is null || !Loot.Owners.Any()) return true;

        if (Loot.Owners.Contains(player)) return true;

        if ((DateTime.Now - _createdAt).TotalSeconds > 10) return true; //todo: add 10 seconds to game configuration

        return false;
    }

    public bool CanBeMovedBy(IPlayer player)
    {
        return Allowed(player);
    }

    public override string ToString()
    {
        if (LootCreated) return base.ToString();

        var content = GetStringContent(Loot?.Items);
        return string.IsNullOrWhiteSpace(content) ? "nothing" : content;
    }

    private string GetStringContent(ILootItem[] items)
    {
        if (Loot is null) return null;
        if (!items.Any()) return null;

        var stringBuilder = new StringBuilder();

        foreach (var item in items)
        {
            var itemType = item.ItemType;

            if (itemType is null) continue;

            if (item.Amount > 1) stringBuilder.Append($"{item.Amount} {itemType.PluralName}");
            else stringBuilder.Append($"{itemType.FullName}");

            stringBuilder.Append(", ");

            if (!(item.Items?.Any() ?? false)) continue;

            stringBuilder.Append(GetStringContent(item.Items));
            stringBuilder.Append(", ");
        }

        return stringBuilder.Remove(stringBuilder.Length - 2, 2).ToString();
    }
}