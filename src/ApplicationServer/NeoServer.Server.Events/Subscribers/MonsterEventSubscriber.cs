using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Server.Events.Combat;
using NeoServer.Server.Events.Creature;

namespace NeoServer.Server.Events.Subscribers;

public class MonsterEventSubscriber(
    CreatureWasBornEventHandler creatureWasBornEventHandler,
    CreatureAttackEventHandler creatureAttackEventHandler
) : ICreatureEventSubscriber
{
    public void Subscribe(ICreature creature)
    {
        if (creature is not IMonster monster) return;

        monster.OnWasBorn += creatureWasBornEventHandler.Execute;
        monster.OnAttackEnemy += creatureAttackEventHandler.Execute;
    }

    public void Unsubscribe(ICreature creature)
    {
        if (creature is not IMonster monster) return;

        monster.OnWasBorn -= creatureWasBornEventHandler.Execute;
        monster.OnAttackEnemy -= creatureAttackEventHandler.Execute;
    }
}