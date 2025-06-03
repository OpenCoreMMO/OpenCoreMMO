using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Extensions.Events.Creatures;

namespace NeoServer.Extensions.Events;

public class CreatureEventSubscriber(CreatureDroppedLootEventHandler creatureDroppedLootEventHandler)
    : ICreatureEventSubscriber, IGameEventSubscriber
{
    public void Subscribe(ICreature creature)
    {
        if (creature is ICombatActor actor) actor.OnDroppedLoot += creatureDroppedLootEventHandler.Execute;
    }

    public void Unsubscribe(ICreature creature)
    {
        if (creature is ICombatActor actor) actor.OnDroppedLoot -= creatureDroppedLootEventHandler.Execute;
    }
}