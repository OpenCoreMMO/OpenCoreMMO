using NeoServer.Domain.Common.Creatures;

namespace NeoServer.Domain.Common.Contracts.Items.Types.Usable;

public interface IUsableOn : IItem
{
    public bool AllowFarUse => Metadata.Attributes.GetAttribute<bool>(ItemTypeAttribute.AllowFarUse);
    public EffectT Effect => Metadata.Attributes.GetEffect();

    public int CooldownTime => Metadata.Attributes.HasAttribute(ItemTypeAttribute.CooldownTime)
        ? Metadata.Attributes.GetAttribute<int>(ItemTypeAttribute.CooldownTime)
        : 1000;
}