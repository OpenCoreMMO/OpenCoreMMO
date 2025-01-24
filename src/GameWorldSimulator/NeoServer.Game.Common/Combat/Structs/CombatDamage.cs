using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Item;

namespace NeoServer.Game.Common.Combat.Structs;

public ref struct CombatDamage
{
    public CombatDamage(ushort damage, DamageType type)
    {
        Damage = damage;
        Type = type;
        Effect = EffectT.None;
        NoEffect = false;
    }

    public CombatDamage(ushort damage, DamageType type, EffectT effect)
    {
        Damage = damage;
        Type = type;
        Effect = effect;
        NoEffect = false;
    }

    public CombatDamage(ushort damage, DamageType type, bool noEffect)
    {
        Damage = damage;
        Type = type;
        Effect = EffectT.None;
        NoEffect = noEffect;
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
    public DamageType Type { get; }

    public EffectT Effect { get; set; }

    public bool Unjustified { get; set; }

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