using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Game.Creatures.Models.Bases.Events;

public record CreatureHealthChangedEvent(ICombatActor Actor, uint OldHealth, uint NewHealth) : IEvent;