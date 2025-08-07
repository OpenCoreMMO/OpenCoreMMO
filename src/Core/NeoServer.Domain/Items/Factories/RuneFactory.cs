using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items.Items.UsableItems.Runes;

namespace NeoServer.Domain.Items.Factories;

public class RuneFactory : IFactory
{
    public event CreateItem OnItemCreated;

    public IItem Create(IItemType itemType, Location location,
        IDictionary<ItemTypeAttribute, IConvertible> attributes)
    {
        if (!ICumulative.IsApplicable(itemType)) return null;
        if (!Rune.IsApplicable(itemType)) return null;


        if (FieldRune.IsApplicable(itemType)) return new FieldRune(itemType, location, attributes);

        return new Rune(itemType, location, attributes);
    }
}