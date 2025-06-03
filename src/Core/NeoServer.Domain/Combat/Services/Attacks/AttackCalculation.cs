using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Item;

namespace NeoServer.Domain.Combat.Services.Attacks;

public class AttackCalculation
{
    public static CombatDamage Calculate(ushort minDamage, ushort maxDamage, DamageType damageType)
    {
        var damageValue = (ushort)GameRandom.Random.NextInRange(minDamage, maxDamage);

        return new CombatDamage(damageValue, damageType);
    }
}