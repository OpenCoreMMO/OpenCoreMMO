using NeoServer.Data.Interfaces;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Game.Creatures.Monster.Summon;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Scripts;

namespace NeoServer.Server.Events.Creature;

public class CreatureDeathEventHandler(
    IPlayerRepository playerRepository,
    IGameCreatureManager creatureManager,
    ICreatureDeathService creatureDeathService,
    IExperienceSharingService experienceSharingService,
    ILootService lootService,
    IScriptManager scriptManager)
    : IEventHandler
{
    public void Execute(ICombatActor deadCreature, IThing by)
    {
        //lua script can be added here to handle loot creation
        _ = lootService.CreateLootContainer(deadCreature);

        creatureDeathService.Handle(deadCreature, by);

        experienceSharingService.Share(deadCreature);

        switch (deadCreature)
        {
            case IMonster monster:
                OnMonsterKilled(monster);
                break;
            case IPlayer player:
                playerRepository.SavePlayer(player);
                break;
        }
        
        scriptManager.CreatureEvents.ExecuteOnCreatureDeath(deadCreature, by);
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