using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;

namespace NeoServer.Domain.Common.Contracts.Items.Types.Usable;

public interface IUsableOn : IItem
{
    public bool AllowFarUse => Metadata.Attributes.GetAttribute<bool>(ItemAttribute.AllowFarUse);
    public EffectT Effect => Metadata.Attributes.GetEffect();

    public int CooldownTime => Metadata.Attributes.HasAttribute(ItemAttribute.CooldownTime)
        ? Metadata.Attributes.GetAttribute<int>(ItemAttribute.CooldownTime)
        : 1000;
}