using NeoServer.Extensions.Events.Creatures;
using NeoServer.Extensions.Npcs;
using NeoServer.Game.Common.Contracts;
using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Extensions.Events;

public class CreatureEventSubscriber : ICreatureEventSubscriber, IGameEventSubscriber
{
    private readonly CreatureDroppedLootEventHandler creatureDroppedLootEventHandler;
    private readonly CreatureKilledEventHandler creatureKilledEventHandler;

    public CreatureEventSubscriber(CreatureDroppedLootEventHandler creatureDroppedLootEventHandler,
        CreatureKilledEventHandler creatureKilledEventHandler)
    {
        this.creatureDroppedLootEventHandler = creatureDroppedLootEventHandler;
        this.creatureKilledEventHandler = creatureKilledEventHandler;
    }

    public void Subscribe(ICreature creature)
    {
        if (creature is ICombatActor actor)
        {
            actor.OnDeath += creatureKilledEventHandler.Execute;
            actor.OnDeath += creatureDroppedLootEventHandler.Execute;
        }

        if (creature is INpc npc) npc.OnAnswer += NpcActionHandler.OnAnswer;
    }

    public void Unsubscribe(ICreature creature)
    {
        if (creature is ICombatActor actor)
        {
            actor.OnDeath -= creatureKilledEventHandler.Execute;
            actor.OnDeath -= creatureDroppedLootEventHandler.Execute;
        }

        if (creature is INpc npc) npc.OnAnswer -= NpcActionHandler.OnAnswer;
    }
}