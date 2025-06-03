using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Game.Creatures.Models.Bases.Events;

public record CreatureKillEvent(ICombatActor Killer, ICombatActor Victim, bool LastHit, bool Unjustified) : IEvent;