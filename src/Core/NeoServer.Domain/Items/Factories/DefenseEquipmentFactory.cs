using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items.Factories.AttributeFactory;
using NeoServer.Domain.Items.Items;

namespace NeoServer.Domain.Items.Factories;

public class DefenseEquipmentFactory : IFactory
{
    private readonly ChargeCounterFactory _chargeCounterFactory;
    private readonly IItemTypeStore _itemTypeStore;

    public DefenseEquipmentFactory(IItemTypeStore itemTypeStore, ChargeCounterFactory chargeCounterFactory)
    {
        _itemTypeStore = itemTypeStore;
        _chargeCounterFactory = chargeCounterFactory;
    }

    public BodyDefenseEquipment Create(IItemType itemType, Location location, ushort? overrideCharges = null)
    {
        if (!BodyDefenseEquipment.IsApplicable(itemType)) return null;

        var chargeCounter = _chargeCounterFactory.Create(itemType, overrideCharges);

        return new BodyDefenseEquipment(itemType, location)
        {
            Charges = chargeCounter,
            ItemTypeFinder = _itemTypeStore.Get
        };
    }
}