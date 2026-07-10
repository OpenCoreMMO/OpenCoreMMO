using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items.Factories.AttributeFactory;
using NeoServer.Domain.Items.Items;

namespace NeoServer.Domain.Items.Factories;

public class DefenseEquipmentFactory : IFactory
{
    private readonly ChargeableFactory _chargeableFactory;
    private readonly IItemTypeStore _itemTypeStore;

    public DefenseEquipmentFactory(IItemTypeStore itemTypeStore, ChargeableFactory chargeableFactory)
    {
        _itemTypeStore = itemTypeStore;
        _chargeableFactory = chargeableFactory;
    }

    public BodyDefenseEquipment Create(IItemType itemType, Location location, ushort? overrideCharges = null)
    {
        if (!BodyDefenseEquipment.IsApplicable(itemType)) return null;

        var chargeable = _chargeableFactory.Create(itemType, overrideCharges);

        return new BodyDefenseEquipment(itemType, location)
        {
            Chargeable = chargeable,
            ItemTypeFinder = _itemTypeStore.Get
        };
    }
}