using NeoServer.Data.Interfaces;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Scripts;

namespace NeoServer.Server.Events.Creature;

public class CreatureDeathEventHandler(
    IPlayerRepository playerRepository,
    IGameCreatureManager creatureManager,
    ICreatureDeathService creatureDeathService,
    IExperienceSharingService experienceSharingService,
    IScriptGameManager scriptGameManager,
    ILootService lootService)
    : IEventHandler
{
    public void Execute(ICombatActor deadCreature, IThing by)
    {
        //lua script can be added here to handle loot creation
        _ = lootService.CreateLootContainer(deadCreature);
        
        creatureDeathService.Handle(deadCreature, by);
        
        experienceSharingService.Share(deadCreature);
        
        if (deadCreature is IMonster monster)
        {
            creatureManager.AddKilledMonsters(monster);
        }

        if (deadCreature is IPlayer player)
        {
            playerRepository.SavePlayer(player);
        }
        
    }
}