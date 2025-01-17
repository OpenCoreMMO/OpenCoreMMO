using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures;

namespace NeoServer.Server.Events.Creature.Monsters;

public class MonsterChangedStateEventHandler
{
    private readonly ICreatureGameInstance creatureGameInstance;

    public MonsterChangedStateEventHandler(ICreatureGameInstance creatureGameInstance)
    {
        this.creatureGameInstance = creatureGameInstance;
    }

    public void Execute(IMonster monster, MonsterState oldState, MonsterState toState)
    {
        // TODO: NTN - See with caiovidal and Muniz
        // if (toState == MonsterState.Sleeping)
        //     creatureGameInstance.TryRemove(monster.CreatureId);
        // else if (oldState == MonsterState.Sleeping) creatureGameInstance.Add(monster);
    }
}