using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Item;

namespace NeoServer.Domain.Common.Combat;

public readonly struct WeaponAttack(IItemType metadata) : IHasElementalDamage
{
    public byte AttackPowerPercentage => TotalAttackPower is 0 ? (byte)0 : (byte)(AttackPower * 100 / TotalAttackPower);

    public byte ElementalAttackPowerPercentage =>
        TotalAttackPower is 0 ? (byte)0 : (byte)(ElementalDamage.AttackPower * 100 / TotalAttackPower);

    public ushort TotalAttackPower => (ushort)(AttackPower + ElementalDamage.AttackPower);
    public ushort AttackPower => metadata.Attributes.GetAttribute<ushort>(ItemAttribute.Attack);
    public ElementalDamage ElementalDamage => metadata.Attributes.GetWeaponElementDamage();
}

public readonly struct ElementalDamage(DamageType damageType, byte attackPower)
{
    public DamageType DamageType => damageType;
    public byte AttackPower => attackPower;
}