using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Game.World.Models.Spawns;
using NeoServer.Server.Commands.Player;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Routines.Creatures.Npc;
using NeoServer.Server.Tasks;

namespace NeoServer.Server.Routines.Creatures;

public class GameCreatureRoutine
{
    private const ushort EVENT_CHECK_CREATURE_INTERVAL = 500;
    private readonly IGameServer _game;
    private readonly PlayerLogOutCommand _playerLogOutCommand;
    private readonly SpawnManager _spawnManager;
    private readonly ISummonService _summonService;

    public GameCreatureRoutine(IGameServer game, SpawnManager spawnManager, PlayerLogOutCommand playerLogOutCommand,
        ISummonService summonService)
    {
        _game = game;
        _spawnManager = spawnManager;
        _playerLogOutCommand = playerLogOutCommand;
        _summonService = summonService;
    }

    public void StartChecking()
    {
        _game.Scheduler.AddEvent(new SchedulerEvent(EVENT_CHECK_CREATURE_INTERVAL, StartChecking));

        foreach (var creature in _game.CreatureManager.GetCreatures())
        {
            if (creature is null or ICombatActor { IsDead: true }) continue;

            CheckPlayer(creature);

            CheckCreature(creature);

            CheckMonster(creature);

            CheckNpc(creature);

            RespawnRoutine.Execute(_spawnManager);
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

        CreatureDefenseRoutine.Execute(monster, _game);
        MonsterStateRoutine.Execute(monster, _summonService);
        MonsterYellRoutine.Execute(monster);
    }

    private void CheckPlayer(ICreature creature)
    {
        if (creature is not IPlayer player) return;

        PlayerPingRoutine.Execute(player, _playerLogOutCommand, _game);
        PlayerRecoveryRoutine.Execute(player);
    }
}