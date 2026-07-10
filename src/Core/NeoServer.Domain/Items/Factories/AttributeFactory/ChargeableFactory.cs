using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Items.Items.Attributes;

namespace NeoServer.Domain.Items.Factories.AttributeFactory;

public class ChargeableFactory : IFactory
{
    public IChargeable Create(IItemType itemType, ushort? overrideCharges = null)
    {
        ushort charges;
        if (overrideCharges.HasValue)
        {
            charges = overrideCharges.Value;
        }
        else
        {
            if (!itemType.Attributes.TryGetAttribute<ushort>(ItemTypeAttribute.Charges, out charges)) return null;
        }

        if (!itemType.Attributes.TryGetAttribute<ushort>(ItemTypeAttribute.ShowCharges, out var showCharges))
            return new Chargeable(charges, true);

        return new Chargeable(charges, showCharges == 1);
    }
}