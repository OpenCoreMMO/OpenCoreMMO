using NeoServer.Game.Common.Combat;

namespace NeoServer.Game.Common.Contracts.Items.Weapons.Attributes;

public interface IHasAttack
{
    WeaponAttack WeaponAttack { get; }
}