using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Items.Items.Attributes;

namespace NeoServer.Domain.Items.Factories.AttributeFactory;

public class ChargeCounterFactory : IFactory
{
    public ChargeCounter Create(IItemType itemType, ushort? overrideCharges = null)
    {
        ushort charges;
        if (overrideCharges.HasValue)
        {
            charges = overrideCharges.Value;
        }
        else
        {
            if (!itemType.Attributes.TryGetAttribute(ItemTypeAttribute.Charges, out charges)) return null;
        }

        if (!itemType.Attributes.TryGetAttribute<ushort>(ItemTypeAttribute.ShowCharges, out var showCharges))
            return new ChargeCounter(charges, true);

        return new ChargeCounter(charges, showCharges == 1);
    }
}