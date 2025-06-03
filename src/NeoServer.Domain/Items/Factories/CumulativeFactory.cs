using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items.Items.Cumulatives;
using NeoServer.Domain.Items.Items.UsableItems;

namespace NeoServer.Domain.Items.Factories;

public class CumulativeFactory : IFactory
{
    public event CreateItem OnItemCreated;


    public IItem Create(IItemType itemType, Location location, IDictionary<ItemAttribute, IConvertible> attributes)
    {
        if (!ICumulative.IsApplicable(itemType)) return null;

        if (Coin.IsApplicable(itemType)) return new Coin(itemType, location, attributes);
        if (HealingItem.IsApplicable(itemType)) return new HealingItem(itemType, location, attributes);
        if (Food.IsApplicable(itemType)) return new Food(itemType, location, attributes);

        return new Cumulative(itemType, location, attributes);
    }
}