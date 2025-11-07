using NeoServer.Data.Interfaces;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Creatures.Events;
using NeoServer.Domain.Creatures.Monster.Summon;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Events.Creature;

public class CreatureDeathEventHandler(
    IPlayerRepository playerRepository,
    IPlayerDeathRepository playerDeathRepository,
    IGameCreatureManager creatureManager,
    ICreatureDeathService creatureDeathService,
    IExperienceSharingService experienceSharingService,
    ILootService lootService,
    ITradeService tradeService,
    GameConfiguration gameConfiguration,
    IMap map)
    : IApplicationEventHandler<CreatureDeathEvent>
{
    public void Handle(CreatureDeathEvent @event)
    {
        var deadCreature = @event.DeadCreature;
        var by = @event.Attacker;
        //lua script can be added here to handle loot creation

        foreach (var spectator in map.GetSpectators(deadCreature.Location))
        {
            spectator.OnSpectatorDies(deadCreature);
        }

        _ = lootService.CreateLootContainer(deadCreature, by);

        var damageRecordResult = deadCreature.ReceivedDamages.GetDamageRecords(gameConfiguration.Death);

        creatureDeathService.Handle(deadCreature, by, damageRecordResult.DamageRecords);

        experienceSharingService.Share(deadCreature);
        
        switch (deadCreature)
        {
            case IMonster monster:
                OnMonsterKilled(monster, by);
                break;
            case IPlayer player:
                tradeService.Cancel(player);
                player.MoveToTemple();
                playerRepository.SavePlayer(player);
                playerDeathRepository.Save(player, damageRecordResult);
                break;
        }
    }

    private void OnMonsterKilled(ICombatActor deadCreature, IThing by)
    {
        if (deadCreature is Summon summon)
        {
            creatureManager.RemoveCreature(summon);
            return;
        }
        
        //do not create blood or corpse for monsters that are killed by another monster and remove from map
        if (deadCreature is IMonster && by is IMonster and not Summon { Master: IPlayer })
        {
            map.RemoveCreature(deadCreature);
        }

        if (deadCreature is not IMonster monster) return;
        creatureManager.AddKilledMonsters(monster);
    }
}