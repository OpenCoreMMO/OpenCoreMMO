using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items.Events;
using NeoServer.Domain.Items.Items;

namespace NeoServer.Domain.Items.Factories;

public class LiquidPoolFactory : ILiquidPoolFactory
{
    private readonly IItemTypeStore _itemTypeStore;

    public LiquidPoolFactory(IItemTypeStore itemTypeStore)
    {
        _itemTypeStore = itemTypeStore;
    }
    
    public ILiquid Create(Location location, LiquidColor color)
    {
        if (!_itemTypeStore.TryGetValue(2016, out var itemType)) return null;

        if (itemType.Group == ItemGroup.Deprecated) return null;

        var item = new LiquidPool(itemType, location, color);
        EventAggregator.Invoke(new ItemCreatedEvent(item));
        return item;
    }

    public ILiquid CreateDamageLiquidPool(Location location, LiquidColor color)
    {
        if (!_itemTypeStore.TryGetValue(2019, out var itemType)) return null;

        if (itemType.Group == ItemGroup.Deprecated) return null;

        var item = new LiquidPool(itemType, location, color);
        EventAggregator.Invoke(new ItemCreatedEvent(item));
        
        return item;
    }
}