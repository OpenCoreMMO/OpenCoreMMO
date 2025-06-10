using NeoServer.Domain.Combat.Services.Spells;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Combat.Attacks;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Effects.Magical;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Combat.Services.Attacks;

public class MonsterCombatService(IAttackService attackService, SpellService spellService)
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

            if (!PerformAttack(monster, target, attack)) continue;

            monster.PostAttack(attack);

            numberOfAttacks++;

            if (GameRandom.Random.Next(0, maxValue: 100) > comboChance) break;
        }
    }

    private bool PerformAttack(IMonster monster, ICombatActor target, IMonsterCombatAttack attack)
    {
        if (attack.Spell is not null)
            return spellService.Cast(monster, attack.NeedTarget ? target : null, attack.Spell, false);

        var combatParameter = attack.CombatParameter;

        combatParameter.CoordinateArea = CreateArea(attack, monster, target);

        return attackService.Execute(new AttackInput(monster, attack.NeedTarget ? target : null, combatParameter))
            .Succeeded;
    }

    private static Coordinate[] CreateArea(IMonsterCombatAttack attack, IMonster monster, ICombatActor target)
    {
        if (attack.CombatParameter.Radius == 0 && attack.CombatParameter.Length == 0 &&
            attack.CombatParameter.Spread == 0)
            return null;

        var range = attack.CombatParameter.Range;

        var origin = attack.NeedTarget ? target.Location : monster.Location;

        if (range > 0 && !attack.NeedTarget && target is null)
        {
            var x = (ushort)GameRandom.Random.Next(-range.Value, maxValue: range.Value);
            var y = (ushort)GameRandom.Random.Next(-range.Value, maxValue: range.Value);

            origin = new Location((ushort)(origin.X + x), (ushort)(origin.Y + y), origin.Z);
        }

        if (attack.CombatParameter.Radius > 0)
            return attack.CombatParameter.CoordinateArea =
                ExplosionEffect.Create(origin, attack.CombatParameter.Radius).ToArray();

        return attack.CombatParameter.CoordinateArea = SpreadEffect.Create(origin, monster.Direction,
            attack.CombatParameter.Length, attack.CombatParameter.Spread);
    }
}