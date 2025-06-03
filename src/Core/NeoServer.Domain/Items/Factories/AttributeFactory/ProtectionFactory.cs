using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Items.Items.Attributes;

namespace NeoServer.Domain.Items.Factories.AttributeFactory;

public class ProtectionFactory
{
    public static IProtection Create(IItem item)
    {
        if (item.Metadata.Attributes.DamageProtection is not { } damageProtection) return null;
        if (!damageProtection.Any()) return null;

        return new Protection(item);
    }
}