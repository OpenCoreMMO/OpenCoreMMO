using System.Collections;
using System.Collections.Immutable;
using NeoServer.Domain.Common.Item;

namespace NeoServer.Domain.Common.Combat.Structs;

public class CombatDamageList : IEnumerable<CombatDamage>
{
    private readonly Dictionary<DamageType, CombatDamage> _damages = new(3);

    public CombatDamageList()
    {
    }

    public CombatDamageList(CombatDamage damage)
    {
        Unjustified = damage.Unjustified;
        AddDamage(damage);
    }

    public CombatDamageList(CombatDamage[] damages)
    {
        foreach (var damage in damages)
        {
            AddDamage(damage);

            if (damage is { Unjustified: true })
            {
                Unjustified = true;
            }
        }
    }

    public void AddDamage(CombatDamage damage)
    {
        if (_damages.TryGetValue(damage.Type, out var existingDamage))
        {
            existingDamage.IncreaseDamage(damage.Damage);
        }
        else
        {
            _damages[damage.Type] = damage;
        }

        if (damage.Type is DamageType.ManaDrain)
        {
            return;
        }
    }

    public void ReduceHealthDamage(int damage)
    {
        foreach (var damageRecord in _damages.Values)
        {
            if (damageRecord.Type is not DamageType.ManaDrain)
            {
                damageRecord.IncreaseDamage(-damage);
            }
        }
    }

    public int Count => _damages.Count;

    public bool Unjustified { get; }

    public Damage TotalDamage {

        get
        {
            var manaDamage = 0;
            var healthDamage = 0;
            
            foreach (var damage in _damages.Values)
            {
                if (damage.Type is DamageType.ManaDrain)
                {
                    manaDamage += damage.Damage;
                    continue;
                }
                
                healthDamage += damage.Damage;
            }
            
            return new Damage((ushort)Math.Max(0, healthDamage), (ushort)Math.Max(0, manaDamage)); 
        }
    }

public CombatDamage RegularDamage
    {
        get
        {
            foreach (var damage in this)
            {
                if (damage is { IsElementalDamage: false, Damage: > 0 })
                {
                    return damage;
                }
            }

            return new CombatDamage();
        }
    }

    public CombatDamage ElementalDamage
    {
        get
        {
            foreach (var damage in this)
                if (damage is { IsElementalDamage: true, NoEffect: false, Damage: > 0 })
                    return damage;

            return new CombatDamage();
        }
    }

    public IEnumerator<CombatDamage> GetEnumerator()
    {
        return _damages.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}