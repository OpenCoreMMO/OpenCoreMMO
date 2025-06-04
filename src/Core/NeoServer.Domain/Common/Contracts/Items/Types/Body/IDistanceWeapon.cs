using NeoServer.Domain.Common.Contracts.Items.Weapons.Attributes;
using NeoServer.Domain.Common.Item;

namespace NeoServer.Domain.Common.Contracts.Items.Types.Body;

public interface IDistanceWeapon : IWeapon, IHasRange
{
    sbyte ExtraHitChance => Metadata.Attributes.GetAttribute<sbyte>(ItemAttribute.HitChance);
    new byte Range => Metadata.Attributes.GetAttribute<byte>(ItemAttribute.Range);
}