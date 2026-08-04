using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;

namespace NeoServer.Domain.Common.Combat.Structs;

public readonly ref struct Damage(ushort healthDamage, ushort manaDamage)
{
    public ushort HealthDamage { get; } = healthDamage;
    public ushort ManaDamage { get; } = manaDamage;
    private ushort Total => (ushort)(HealthDamage + ManaDamage);

    public static implicit operator ushort(Damage damage)
    {
        return damage.Total;
    }
}

public struct CalculatedAttackDamage
{
    public CombatDamage MainDamage { get; set; }
    public CombatDamage ExtraDamage { get; set; }
}

public class CombatDamage
{
    public CombatDamage()
    {
    }

    public CombatDamage(ushort damage, DamageType type, DamageOrigin origin = DamageOrigin.None)
    {
        Damage = damage;
        Type = type;
        Effect = EffectT.None;
        NoEffect = false;
        Origin = origin;
    }

    public CombatDamage(ushort damage, DamageType type, EffectT effect, DamageOrigin origin = DamageOrigin.None)
    {
        Damage = damage;
        Type = type;
        Effect = effect;
        NoEffect = false;
        Origin = origin;
    }

    public CombatDamage(ushort damage, DamageType type, bool noEffect, DamageOrigin origin = DamageOrigin.None)
    {
        Damage = damage;
        Type = type;
        Effect = EffectT.None;
        NoEffect = noEffect;
        Origin = origin;
    }

    public bool NoEffect { get; }

    /// <summary>
    ///     Check if damage is elemental
    /// </summary>
    public bool IsElementalDamage => Type != DamageType.Melee && Type != DamageType.Physical;

    /// <summary>
    ///     Damage value to health or mana
    /// </summary>
    public ushort Damage { get; private set; }

    /// <summary>
    ///     Type of the damage (physical, fire...)
    /// </summary>
    public DamageType Type { get; private set; }

    /// <summary>
    ///     Origin of the damage (condition, spell...)
    /// </summary>
    public DamageOrigin Origin { get; }

    public EffectT Effect { get; set; }

    public bool Unjustified { get; set; }

    public void ChangeDamageType(DamageType newType)
    {
        Type = newType;
    }

    /// <summary>
    ///     Sets a new damage
    /// </summary>
    /// <param name="newDamage"></param>
    public void SetNewDamage(ushort newDamage)
    {
        Damage = newDamage;
    }

    /// <summary>
    ///     Sets a new damage
    /// </summary>
    public void ReduceDamageByPercent(short percent)
    {
        Damage = (ushort)((100 - percent) * Damage / 100);
    }

    /// <summary>
    ///     Increase damage by value of param
    /// </summary>
    /// <param name="damage"></param>
    public void IncreaseDamage(int damage)
    {
        if (Damage + damage < 0) damage = Damage;

        Damage += (ushort)damage;
    }

    /// <summary>
    ///     Converts damage value to string
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return Damage.ToString();
    }
}