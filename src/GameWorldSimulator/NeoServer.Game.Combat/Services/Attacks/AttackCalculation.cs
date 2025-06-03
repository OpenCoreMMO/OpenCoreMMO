using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Item;

namespace NeoServer.Game.Combat.Services.Attacks;

public class AttackCalculation
{
    public static CombatDamage Calculate(ushort minDamage, ushort maxDamage, DamageType damageType)
    {
        var damageValue = (ushort)GameRandom.Random.NextInRange(minDamage, maxDamage);

        return new CombatDamage(damageValue, damageType);
    }
}