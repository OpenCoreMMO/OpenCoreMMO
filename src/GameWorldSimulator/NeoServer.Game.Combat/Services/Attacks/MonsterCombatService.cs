using System;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Helpers;

namespace NeoServer.Game.Combat.Services.Attacks;

public class MonsterCombatService(IAttackService attackService)
{
    public void Attack(IMonster monster, ICombatActor target)
    {
        if (!monster.IsHostile) return;
        if (monster.Metadata.Attacks.Length == 0) return;

        var maxNumberOfAttacks = (int)Math.Min(2, Math.Ceiling(monster.Metadata.Attacks.Length / 1.5));
        const int comboChance = 30;

        var numberOfAttacks = 0;
        foreach (var attack in monster.Metadata.Attacks)
        {
            if (numberOfAttacks > maxNumberOfAttacks) break;
            
            if (attack.AttackChance < GameRandom.Random.Next(0, maxValue: 100))
                continue;

            if (attack.CombatParameter is null)
            {
                Console.WriteLine($"Combat attack not found for monster: {monster.Name}");
                continue;
            }

            var combatParameter = attack.CombatParameter;

            var result = attackService.Execute(new AttackInput(monster, target, combatParameter));

            if (result.Failed)
            {
                continue;
            }

            numberOfAttacks++;

            if (GameRandom.Random.Next(0, maxValue: 100) > comboChance)
            {
                break;
            }
        }
    }
}