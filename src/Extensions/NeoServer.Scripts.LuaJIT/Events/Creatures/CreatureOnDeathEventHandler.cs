using NeoServer.Game.Common.Contracts;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Events.Creatures;

public class CreatureOnDeathEventHandler : IGameEventHandler
{
    private readonly ICreatureEvents _creatureEvents;

    public CreatureOnDeathEventHandler(ICreatureEvents creatureEvents)
    {
        _creatureEvents = creatureEvents;
    }

    public void Execute(ICombatActor actor, IThing by, ILoot loot)
    {
        var deathEvents = _creatureEvents
            .GetCreatureEvents(CreatureEventType.CREATURE_EVENT_DEATH);

        foreach (var deathEvent in deathEvents)
            deathEvent.ExecuteOnDeath(actor, actor.Corpse as IItem, by as ICreature, null, false, false);
    }
}