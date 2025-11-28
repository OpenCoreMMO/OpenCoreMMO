using NeoServer.Domain.Combat.Attacks;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Effects.Magical;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Monster;
using NeoServer.Domain.Spells;
using Serilog;

namespace NeoServer.Domain.Combat.Monster;

public class MonsterCombatService(IAttackService attackService, SpellService spellService, ILogger logger)
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
                logger.Warning("Combat attack not found for monster: {MonsterName}", monster.Name);
                continue;
            }

            if (!PerformAttack(monster, target, attack)) continue;

            monster.PostAttack(attack);

            numberOfAttacks++;

            if (GameRandom.Random.Next(0, maxValue: 100) > comboChance) break;
        }
    }

    private bool PerformAttack(IMonster monster, ICombatActor target, MonsterCombatType type)
    {
        if (type.Spell is not null)
            return spellService.Cast(monster, type.NeedTarget ? target : null, type.Spell, false);

        var combatParameter = type.CombatParameter;

        combatParameter.CoordinateArea = CreateArea(type, monster, target);

        monster.TurnTo(target);

        return attackService.Execute(new AttackInput(monster, type.NeedTarget ? target : null, combatParameter))
            .Result.Succeeded;
    }

    private static Coordinate[] CreateArea(MonsterCombatType type, IMonster monster, ICombatActor target)
    {
        if (type.CombatParameter.Radius == 0 && type.CombatParameter.Length == 0 &&
            type.CombatParameter.Spread == 0)
            return null;

        var range = type.CombatParameter.Range;

        var origin = type.NeedTarget ? target.Location : monster.Location;

        if (range > 0 && !type.NeedTarget && target is null)
        {
            var x = (ushort)GameRandom.Random.Next(-range.Value, maxValue: range.Value);
            var y = (ushort)GameRandom.Random.Next(-range.Value, maxValue: range.Value);

            origin = new Location((ushort)(origin.X + x), (ushort)(origin.Y + y), origin.Z);
        }

        if (type.CombatParameter.Radius > 0)
            return type.CombatParameter.CoordinateArea =
                ExplosionEffect.Create(origin, type.CombatParameter.Radius).ToArray();

        return type.CombatParameter.CoordinateArea = SpreadEffect.Create(origin, monster.Direction,
            type.CombatParameter.Length, type.CombatParameter.Spread);
    }
}