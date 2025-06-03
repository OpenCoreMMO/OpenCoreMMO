using NeoServer.Extensions.Events.Creatures;
using NeoServer.Game.Common.Contracts;
using NeoServer.Game.Common.Contracts.Creatures;

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