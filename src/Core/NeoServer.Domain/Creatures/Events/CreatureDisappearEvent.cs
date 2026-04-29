using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Creatures.Events;

public record CreatureDisappearEvent(
    ICreature Self,
    ICreature Creature) : IEvent;
