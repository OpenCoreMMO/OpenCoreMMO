using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Server.Events.Combat;

namespace NeoServer.Server.Events.Subscribers;

public class MonsterEventSubscriber(
    CreatureAttackEventHandler creatureAttackEventHandler
) : ICreatureEventSubscriber
{
    public void Subscribe(ICreature creature)
    {
        if (creature is not IMonster monster) return;

        monster.OnAttackEnemy += creatureAttackEventHandler.Execute; // TODO: NTN - Create a use case
    }

    public void Unsubscribe(ICreature creature)
    {
        if (creature is not IMonster monster) return;

        monster.OnAttackEnemy -= creatureAttackEventHandler.Execute;
    }
}