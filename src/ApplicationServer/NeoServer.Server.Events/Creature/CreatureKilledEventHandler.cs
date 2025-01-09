using NeoServer.Data.Interfaces;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Events.Creature;

public class CreatureKilledEventHandler(
    IPlayerRepository playerRepository,
    IGameCreatureManager creatureManager,
    ICreatureDeathService creatureDeathService)
    : IEventHandler
{
    public void Execute(ICombatActor deadCreature, IThing by, ILoot loot)
    {
        creatureDeathService.Handle(deadCreature, by, loot);
        
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