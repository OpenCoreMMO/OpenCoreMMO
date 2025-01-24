using NeoServer.Game.Combat.Events;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Game.Creatures.Models.Bases.Events;

public class CreatureKillEvent : IEvent
{
    public ICombatActor Killer { get; set; }
    public ICombatActor Victim {get;set;} 
    public bool LastHit {get;set;} 
    public bool Unjustified {get;set;}
    
    public static CreatureKillEvent SetValues(ICombatActor killer, ICombatActor victim, bool lastHit, bool unjustified)
    {
        var instance = SharedEvent.Get<CreatureKillEvent>();
   
        instance.Killer = killer;
        instance.Victim = victim;
        instance.LastHit = lastHit;
        instance.Unjustified = unjustified;
        
        return instance;
    }
}