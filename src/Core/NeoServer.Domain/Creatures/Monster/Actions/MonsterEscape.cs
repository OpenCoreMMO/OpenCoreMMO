using NeoServer.Domain.Combat;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Monster.Combat;

namespace NeoServer.Domain.Creatures.Monster.Actions;

internal static class MonsterEscape
{
    public static void Escape(Monster monster)
    {
        monster.StopFollowing();

        var targets = monster.Targets;

        var escapeFrom = FindTarget(monster, targets);

        if (escapeFrom is null) return;

        monster.Escape(escapeFrom.Location);
    }

    private static ICreature FindTarget(ICombatActor monster, TargetList targets)
    {
        if (targets.TryGetTarget(monster.AutoAttackTargetId, out var creature)) return creature.Creature;

        ICreature escapeFrom = null;

        foreach (CombatTarget target in targets)
        {
            if (target.CanReachCreature)
            {
                escapeFrom = target.Creature;
                break;
            }

            escapeFrom = target.Creature;
        }

        return escapeFrom;
    }
}