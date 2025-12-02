using System.Text;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Monster.Loot;

namespace NeoServer.Domain.Items.Items.Containers;

public class LootContainer : Container.Container, ILootContainer
{
    private readonly DateTime _createdAt;

    public LootContainer(IItemType type, Location location, Loot loot) : base(type, location)
    {
        Loot = loot;
        _createdAt = DateTime.UtcNow;
    }

    public Loot Loot { get; }
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
        if (Loot?.Owners is null || Loot.Owners.Count == 0) return true;

        if (Loot.Owners.Contains(player)) return true;

        if ((DateTime.UtcNow - _createdAt).TotalSeconds > 10) return true; //todo: add 10 seconds to game configuration

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

    private string GetStringContent(LootItem[] items)
    {
        if (Loot is null) return null;
        if (items.Length == 0) return null;

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