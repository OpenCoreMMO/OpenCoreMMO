using NeoServer.Domain.Common.Combat;

namespace NeoServer.Domain.Common.Contracts.Items.Weapons.Attributes;

public interface IHasAttack
{
    WeaponAttack WeaponAttack { get; }
}