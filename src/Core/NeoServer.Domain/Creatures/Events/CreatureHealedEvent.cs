using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Creatures.Events;

public record CreatureHealedEvent(ICombatActor HealedCreature, ICreature HealerCreature, ushort Amount) : IEvent;
