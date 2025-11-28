using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;

namespace NeoServer.Domain.Common.Contracts.Items.Types.Usable;

public interface IUsableOn : IItem
{
    public EffectT Effect => Metadata.Attributes.GetEffect();

    public int CooldownTime => Metadata.Attributes.HasAttribute(ItemTypeAttribute.CooldownTime)
        ? Metadata.Attributes.GetAttribute<int>(ItemTypeAttribute.CooldownTime)
        : 1000;
}