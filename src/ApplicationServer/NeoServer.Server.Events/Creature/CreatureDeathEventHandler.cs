using NeoServer.Data.Interfaces;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Services;
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
    //IScriptManager scriptManager)
    : IEventHandler
{
    public void Execute(ICombatActor deadCreature, IThing by)
    {
        //lua script can be added here to handle loot creation
        _ = lootService.CreateLootContainer(deadCreature);

        var damageRecordResult = deadCreature.ReceivedDamages.GetDamageRecords(gameConfiguration.Death);

        creatureDeathService.Handle(deadCreature, by, damageRecordResult.DamageRecords);

        experienceSharingService.Share(deadCreature);

        switch (deadCreature)
        {
            case IMonster monster:
                OnMonsterKilled(monster);
                break;
            case IPlayer player:
                playerRepository.SavePlayer(player);
                playerDeathRepository.Save(player, damageRecordResult);
                break;
        }

        //    scriptManager.CreatureEvents.ExecuteOnCreatureDeath(deadCreature, by);
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