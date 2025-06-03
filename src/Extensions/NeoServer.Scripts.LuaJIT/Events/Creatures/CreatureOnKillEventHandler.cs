using NeoServer.Game.Common;
using NeoServer.Game.Creatures.Models.Bases.Events;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Events.Creatures;

public class CreatureOnKillEventHandler(ICreatureEvents creatureEvents) : IApplicationEventHandler<CreatureKillEvent>
{
    public void Handle(CreatureKillEvent @event)
    {
        foreach (var creatureEvent in creatureEvents.GetCreatureEvents(@event.Killer.CreatureId,
                     CreatureEventType.CREATURE_EVENT_KILL))
            creatureEvent.ExecuteOnKill(@event.Killer, @event.Victim, @event.LastHit);
    }
}