using NeoServer.BuildingBlocks.Application.Contracts;
using NeoServer.BuildingBlocks.Application.IntegrationEvents.Creatures;
using NeoServer.BuildingBlocks.Infrastructure;
using NeoServer.BuildingBlocks.Infrastructure.Threading.Scheduler;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Game.World.Models.Spawns;
using NeoServer.Modules.Creatures.Monster;
using NeoServer.Modules.Creatures.Monster.Routines;
using NeoServer.Modules.Creatures.Npc.Routines;
using NeoServer.Modules.Players.Routines;
using NeoServer.Modules.Session.Ping;

namespace NeoServer.Modules.Creatures.Routines;

public class GameCreatureJob(
    IGameServer game,
    SpawnManager spawnManager,
    ISummonService summonService,
    PingRoutine pingRoutine, IEventBus eventBus)
{
    private const ushort EVENT_CHECK_CREATURE_INTERVAL = 500;

    public void StartChecking()
    {
        game.Scheduler.AddEvent(new SchedulerEvent(EVENT_CHECK_CREATURE_INTERVAL, StartChecking));

        foreach (var creature in game.CreatureManager.GetCreatures())
        {
            if (creature is null or ICombatActor { IsDead: true }) continue;

            CheckPlayer(creature);

            CheckCreature(creature);

            CheckMonster(creature);

            CheckNpc(creature);

            RespawnRoutine.Execute(spawnManager);
            eventBus.Publish(new CreatureCheckedIntegrationEvent(creature));
        }
    }

    private static void CheckCreature(ICreature creature)
    {
        if (creature is ICombatActor combatActor) CreatureConditionRoutine.Execute(combatActor);
    }

    private static void CheckNpc(ICreature creature)
    {
        if (creature is INpc npc) NpcRoutine.Execute(npc);
    }

    private void CheckMonster(ICreature creature)
    {
        if (creature is not IMonster monster) return;

        MonsterDefenseJob.Execute(monster, game);
        MonsterStateRoutine.Execute(monster, summonService);
        MonsterYellRoutine.Execute(monster);
    }

    private void CheckPlayer(ICreature creature)
    {
        if (creature is not IPlayer player) return;

        pingRoutine.Execute(player);
        PlayerRecoveryRoutine.Execute(player);
    }
}