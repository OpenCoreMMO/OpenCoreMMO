using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types.Usable;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items.Items.Cumulatives;

namespace NeoServer.Domain.Items.Items.UsableItems;

public class HealingItem : Cumulative, IConsumable
{
    public HealingItem(IItemType type, Location location) : base(type, location)
    {
    }

    public ushort Min => Metadata.Attributes.GetInnerAttributes(ItemTypeAttribute.Healing)
        ?.GetAttribute<ushort>(ItemTypeAttribute.Min) ?? 0;

    public ushort Max => Metadata.Attributes.GetInnerAttributes(ItemTypeAttribute.Healing)
        ?.GetAttribute<ushort>(ItemTypeAttribute.Max) ?? 0;

    public string Type => Metadata.Attributes.GetAttribute(ItemTypeAttribute.Healing);

    public void Use(IPlayer usedBy, ICreature creature)
    {
        if (creature is not ICombatActor actor) return;
        if (Max == 0) return;

        var value = (ushort)GameRandom.Random.Next(Min, maxValue: Max);

        if (Type.Equals("hp", StringComparison.InvariantCultureIgnoreCase))
            actor.Heal(value, usedBy);
        else if (creature is IPlayer player) player.IncreaseMana(value);

        Reduce();

        IConsumable.RaiseOnUsed(usedBy, creature, this);
    }

    public static bool IsApplicable(IItemType type)
    {
        return type.Group is ItemGroup.Healing;
    }
}