using System.Text;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Parsers;

namespace NeoServer.Domain.Items.Items.Attributes;

public sealed class Protection : IProtection
{
    private readonly IItem _item;

    public Protection(IItem item)
    {
        _item = item;
    }

    private Dictionary<DamageType, sbyte> DamageProtection => _item.Metadata.Attributes.DamageProtection;


    public bool Protect(CombatDamage damage)
    {
        var protection = GetProtection(damage);
        if (protection == 0) return false;

        var protectionValue = Math.Max((sbyte)-100, Math.Min((byte)100, protection));
        damage.ReduceDamageByPercent(protectionValue);
        return true;
    }

    private sbyte GetProtection(CombatDamage combatDamage)
    {
        var damageType = combatDamage.Type;
        if (DamageProtection is null || DamageProtection.Count == 0 || damageType == DamageType.None) return 0;

        if (DamageProtection.TryGetValue(damageType, out var value)) return value;

        if (damageType == DamageType.Melee)
            if (DamageProtection.TryGetValue(DamageType.Physical, out var meleeProtection))
                return meleeProtection;

        if (combatDamage.IsElementalDamage)
            if (DamageProtection.TryGetValue(DamageType.Elemental, out var elementalProtection))
                return elementalProtection;

        if (DamageProtection.TryGetValue(DamageType.All, out var protectionValue)) return protectionValue;

        return 0;
    }

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("protection ");
        foreach (var (damageType, value) in DamageProtection)
        {
            if (value == 0) continue;

            var protectionValue = Math.Max((sbyte)-100, Math.Min((byte)100, value));
            var damage = DamageTypeParser.Parse(damageType);

            if (damageType == DamageType.LifeDrain) damage = "life drain";
            if (damageType == DamageType.ManaDrain) damage = "mana drain";

            stringBuilder.Append($"{damage} {(protectionValue >= 0 ? "+" : string.Empty)}{protectionValue}%, ");
        }

        stringBuilder.Remove(stringBuilder.Length - 2, 2);
        return stringBuilder.ToString();
    }
}