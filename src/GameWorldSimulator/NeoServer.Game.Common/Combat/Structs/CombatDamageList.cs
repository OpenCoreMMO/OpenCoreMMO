using System.Collections.Immutable;
using NeoServer.Game.Common.Item;

namespace NeoServer.Game.Common.Combat.Structs;

public readonly struct CombatDamageList
{
    private readonly CombatDamage? _singleDamage;
    private readonly ImmutableArray<CombatDamage> _multipleDamages;

    public CombatDamageList(CombatDamage damage)
    {
        _singleDamage = damage;
        _multipleDamages = default;
        Unjustified = damage.Unjustified;
    }

    public CombatDamageList(ImmutableArray<CombatDamage> damages)
    {
        _singleDamage = null;
        _multipleDamages = damages;

        foreach (var damage in damages)
        {
            if (damage.Unjustified)
            {
                Unjustified = true;
                break;
            }
        }
    }

    public bool IsSingle => _singleDamage.HasValue;
    public int Count => IsSingle ? 1 : _multipleDamages.Length;

    public bool Unjustified { get; }
    public Damage TotalDamage
    {
        get
        {
            ushort health = 0, mana = 0;

            foreach (var damage in this)
            {
                if (damage.Type is DamageType.ManaDrain)
                    mana += (ushort)damage.Damage;
                else
                    health += (ushort)damage.Damage;
            }

            return new Damage(health, mana);
        }
    }

    public CombatDamage ElementalDamage
    {
        get
        {
            foreach (var damage in this)
            {
                if (damage is { IsElementalDamage: true, NoEffect: false, Damage: > 0 })
                    return damage;
            }

            return new CombatDamage();
        }
    }

    // Enumerator struct — no allocation
    public Enumerator GetEnumerator() => new(this);

    public ref struct Enumerator(CombatDamageList list)
    {
        private int _index = -1;

        public CombatDamage Current
        {
            get
            {
                if (list.IsSingle)
                    return list._singleDamage!.Value;
                return list._multipleDamages[_index];
            }
        }

        public bool MoveNext()
        {
            _index++;
            return list.IsSingle ? _index == 0 : _index < list._multipleDamages.Length;
        }
    }
}
