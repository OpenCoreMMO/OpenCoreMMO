using System;
using Dapper;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.World.Models.Spawns;
using NeoServer.Server.Commands.Player;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Routines.Creatures.Monster;
using NeoServer.Server.Routines.Creatures.Npc;
using NeoServer.Server.Routines.Creatures.Player;
using NeoServer.Server.Tasks;

namespace NeoServer.Server.Routines.Creatures;

public class GameCreatureRoutine(
    IGameServer game,
    SpawnManager spawnManager,
    PlayerLogOutCommand playerLogOutCommand,
    PlayerStatusRoutine playerStatusRoutine,
    MonsterStateRoutine monsterStateRoutine)
{
    private const ushort EVENT_CREATURE_COUNT = 10;
    private const ushort EVENT_CREATURE_THINK_INTERVAL = 1000;
    private const ushort EVENT_CHECK_CREATURE_INTERVAL = EVENT_CREATURE_THINK_INTERVAL / EVENT_CREATURE_COUNT;
    private int _currentCreatureBlock;

    public void StartChecking()
    {
        game.Scheduler.AddEvent(new SchedulerEvent(EVENT_CHECK_CREATURE_INTERVAL, StartChecking));

        _currentCreatureBlock++;
        
        var creatureBlockIndex = _currentCreatureBlock % EVENT_CREATURE_COUNT;

        var creatureList = game.CreatureManager.GetCreatures().AsList();
        var totalCreatures = creatureList.Count;
        
        if (totalCreatures == 0) return;

        var groupSize = totalCreatures / EVENT_CREATURE_COUNT;
        var remainder = totalCreatures % EVENT_CREATURE_COUNT;
        
        var startIndex = creatureBlockIndex * groupSize + Math.Min(creatureBlockIndex, remainder);
        var endIndex = startIndex + groupSize + (creatureBlockIndex < remainder ? 1 : 0);

        // If the current group index is beyond the available groups, reset to block 0 and process first group
        if (startIndex >= totalCreatures)
        {
            _currentCreatureBlock = 0;
            startIndex = 0;
            endIndex = groupSize + (remainder > 0 ? 1 : 0);
        }

        for (var i = startIndex; i < endIndex && i < totalCreatures; i++)
        {
            var creature = creatureList[i];
            
            if (creature is null or ICombatActor { IsDead: true }) continue;
            if (!creature.IsThinking()) continue;

            creature.Think(EVENT_CREATURE_THINK_INTERVAL);

            CheckPlayer(creature);
            CheckCreature(creature);
            CheckMonster(creature);
            CheckNpc(creature);
        }

        RespawnRoutine.Execute(spawnManager);
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

        CreatureDefenseRoutine.Execute(monster, game);
        monsterStateRoutine.Execute(monster);
        MonsterYellRoutine.Execute(monster);
    }

    private void CheckPlayer(ICreature creature)
    {
        if (creature is not IPlayer player) return;

        PlayerPingRoutine.Execute(player, playerLogOutCommand, game);
        PlayerRecoveryRoutine.Execute(player);
        PlayerSkullRoutine.Execute(player);
        playerStatusRoutine.Execute(player);
    }
}