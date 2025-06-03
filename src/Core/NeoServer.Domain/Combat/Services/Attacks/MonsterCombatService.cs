using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Combat.Attacks;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Effects.Magical;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Combat.Services.Attacks;

public class MonsterCombatService(IAttackService attackService)
{
    public void Attack(IMonster monster, ICombatActor target)
    {
        if (!monster.IsHostile) return;
        if (monster.Metadata.Attacks.Length == 0) return;

        var maxNumberOfAttacks = (int)Math.Min(2, Math.Ceiling(monster.Metadata.Attacks.Length / 1.5));
        const int comboChance = 50;

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

            combatParameter.CoordinateArea = CreateArea(attack, monster, target);

            var result = attackService.Execute(new AttackInput(monster, target, combatParameter));

            if (result.Failed) continue;

            numberOfAttacks++;

            if (GameRandom.Random.Next(0, maxValue: 100) > comboChance) break;
        }
    }

    private static Coordinate[] CreateArea(IMonsterCombatAttack attack, IMonster monster, ICombatActor target)
    {
        if (attack.CombatParameter.Radius == 0 && attack.CombatParameter.Length == 0 &&
            attack.CombatParameter.Spread == 0)
            return null;

        var origin = attack.HasTarget ? target.Location : monster.Location;

        if (attack.CombatParameter.Radius > 0)
            return attack.CombatParameter.CoordinateArea =
                ExplosionEffect.Create(origin, attack.CombatParameter.Radius).ToArray();

        return attack.CombatParameter.CoordinateArea = SpreadEffect.Create(origin, monster.Direction,
            attack.CombatParameter.Length, attack.CombatParameter.Spread);
    }
}