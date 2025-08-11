using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items.Items.Cumulatives;
using NeoServer.Domain.Items.Items.UsableItems;

namespace NeoServer.Domain.Items.Factories;

public class CumulativeFactory : IFactory
{
    public event CreateItem OnItemCreated;


    public IItem Create(IItemType itemType, Location location)
    {
        if (!ICumulative.IsApplicable(itemType)) return null;

        if (Coin.IsApplicable(itemType)) return new Coin(itemType, location);
        if (HealingItem.IsApplicable(itemType)) return new HealingItem(itemType, location);
        if (Food.IsApplicable(itemType)) return new Food(itemType, location);

        return new Cumulative(itemType, location);
    }
}