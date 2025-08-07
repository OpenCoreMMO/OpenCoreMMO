using NeoServer.Data.Interfaces;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Services;
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
    GameConfiguration gameConfiguration)
    : IApplicationEventHandler<CreatureDeathEvent>
{
    public void Handle(CreatureDeathEvent @event)
    {
        var deadCreature = @event.DeadCreature;
        var by = @event.Attacker;
        //lua script can be added here to handle loot creation

        _ = lootService.CreateLootContainer(deadCreature, by);

        var damageRecordResult = deadCreature.ReceivedDamages.GetDamageRecords(gameConfiguration.Death);

        creatureDeathService.Handle(deadCreature, by, damageRecordResult.DamageRecords);

        experienceSharingService.Share(deadCreature);

        // bloodPoolService.CreateSplash(deadCreature);

        switch (deadCreature)
        {
            case IMonster monster:
                OnMonsterKilled(monster);
                break;
            case IPlayer player:
                player.MoveToTemple();
                playerRepository.SavePlayer(player);
                playerDeathRepository.Save(player, damageRecordResult);
                break;
        }
    }

    private void OnMonsterKilled(ICombatActor creature)
    {
        if (creature is Summon summon)
        {
            creatureManager.RemoveCreature(summon);
            return;
        }

        if (creature is not IMonster monster) return;
        creatureManager.AddKilledMonsters(monster);
    }
}