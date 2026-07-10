using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Items.Items.Attributes;

namespace NeoServer.Domain.Items.Factories.AttributeFactory;

public class DecayableFactory
{
    public static DecayTracker CreateIfItemIsDecayable(IItem item)
    {
        if (Guard.AnyNull(item)) return null;

        return item.HasDecayBehavior ? new DecayTracker(item) : null;
    }
}