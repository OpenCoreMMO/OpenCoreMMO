using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Creatures.Events;

public record CreatureChangedSpeedEvent(
    IWalkableCreature Creature,
    ushort Speed) : IEvent;
