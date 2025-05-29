using NeoServer.Extensions.Events.Creatures;
using NeoServer.Game.Common.Contracts;
using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Extensions.Events;

public class CreatureEventSubscriber : ICreatureEventSubscriber, IGameEventSubscriber
{
    private readonly CreatureDroppedLootEventHandler creatureDroppedLootEventHandler;

    public CreatureEventSubscriber(CreatureDroppedLootEventHandler creatureDroppedLootEventHandler)
    {
        this.creatureDroppedLootEventHandler = creatureDroppedLootEventHandler;
    }

    public void Subscribe(ICreature creature)
    {
        if (creature is ICombatActor actor)
        {
            actor.OnDeath += creatureDroppedLootEventHandler.Execute;
        }
    }

    public void Unsubscribe(ICreature creature)
    {
        if (creature is ICombatActor actor)
        {
            actor.OnDeath -= creatureDroppedLootEventHandler.Execute;
        }
    }
}