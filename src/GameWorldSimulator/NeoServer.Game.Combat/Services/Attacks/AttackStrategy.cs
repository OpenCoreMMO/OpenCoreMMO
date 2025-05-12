using System;
using NeoServer.Game.Common.Combat.Structs;

namespace NeoServer.Game.Combat.Services.Attacks;

public class AttackStrategy(RegularAttackService regularAttackService)
{
    public IAttackService GetAttackService(AttackType attackType) => attackType switch
    {
        AttackType.Regular => regularAttackService,
        // AttackType.Distance => new DistanceAttackService(),
        // AttackType.Rune => new RuneAttackService(),
        // AttackType.Spell => new SpellAttackService(),
        // AttackType.Field => new FieldAttackService(),
        _ => throw new ArgumentOutOfRangeException(nameof(attackType), attackType, null)
    };
}