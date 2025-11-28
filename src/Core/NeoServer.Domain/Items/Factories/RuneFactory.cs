using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items.Items.UsableItems.Runes;

namespace NeoServer.Domain.Items.Factories;

public class RuneFactory : IFactory
{
    public IItem Create(IItemType itemType, Location location)
    {
        if (!ICumulative.IsApplicable(itemType)) return null;
        if (!Rune.IsApplicable(itemType)) return null;


        if (FieldRune.IsApplicable(itemType)) return new FieldRune(itemType, location);

        return new Rune(itemType, location);
    }
}