using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Tasks;

namespace NeoServer.Server.Routines.Creatures;

public static class CreatureDefenseRoutine
{
    public static void Execute(IMonster monster, IGameServer game)
    {
        if (monster.IsDead) return;

        if (!monster.IsInCombat || monster.Defending) return;

        var interval = monster.Defend();

        ScheduleDefense(game, monster, interval);
    }

    private static void ScheduleDefense(IGameServer game, IMonster monster, ushort interval)
    {
        if (monster.Defending)
            game.Scheduler.AddEvent(new SchedulerEvent(interval, () =>
            {
                var defendInterval = monster.Defend();
                ScheduleDefense(game, monster, defendInterval);
            }));
    }
}