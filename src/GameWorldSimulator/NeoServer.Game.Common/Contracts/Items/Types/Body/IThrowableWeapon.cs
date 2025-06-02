using NeoServer.Game.Common.Contracts.Items.Weapons.Attributes;

namespace NeoServer.Game.Common.Contracts.Items.Types.Body;

public interface IThrowableWeapon : ICumulative, IWeapon, IHasAttack, IHasRange
{
    byte AttackPower { get; }
    byte ExtraHitChance { get; }
    bool ShouldBreak { get; }
}