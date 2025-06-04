using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Items.Items.Cumulatives;

public class Coin : Cumulative
{
    public Coin(IItemType type, Location location, IDictionary<ItemAttribute, IConvertible> attributes) : base(type,
        location, attributes)
    {
    }

    public Coin(IItemType type, Location location, byte amount) : base(type, location, amount)
    {
    }

    private uint WorthMultiplier => Metadata.Attributes.GetAttribute<uint>(ItemAttribute.Worth);
    public uint Worth => Amount * WorthMultiplier;

    public static bool IsApplicable(IItemType type)
    {
        return type.Group is ItemGroup.Coin;
    }
}