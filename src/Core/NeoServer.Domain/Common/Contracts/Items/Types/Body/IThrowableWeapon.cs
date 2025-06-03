using NeoServer.Domain.Common.Contracts.Items.Weapons.Attributes;

namespace NeoServer.Domain.Common.Contracts.Items.Types.Body;

public interface IThrowableWeapon : ICumulative, IWeapon, IHasAttack, IHasRange
{
    byte AttackPower { get; }
    byte ExtraHitChance { get; }
    bool ShouldBreak { get; }
}