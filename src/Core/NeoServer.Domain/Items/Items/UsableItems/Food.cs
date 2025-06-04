using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.Items.Types.Usable;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items.Items.Cumulatives;

namespace NeoServer.Domain.Items.Items.UsableItems;

public class Food : Cumulative, IConsumable
{
    public Food(IItemType type, Location location, IDictionary<ItemAttribute, IConvertible> attributes) : base(type,
        location, attributes)
    {
    }

    public Food(IItemType type, Location location, byte amount) : base(type,
        location, amount)
    {
    }
    public ushort Duration => Metadata.Attributes.GetAttribute<ushort>(ItemAttribute.Duration);

    public int CooldownTime => 0;

    public void Use(IPlayer usedBy, ICreature creature)
    {
        if (creature is not IPlayer player) return;

        if (!player.Feed(this)) return;

        Reduce();
        IConsumable.RaiseOnUsed(usedBy, creature, this);
    }

    public static bool IsApplicable(IItemType type)
    {
        return type.Group is ItemGroup.Food;
    }
}