using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Monster.Loot;

namespace NeoServer.Domain.Creatures.Events;

public record CreatureDroppedLootEvent(
    ICombatActor Actor,
    Loot Loot) : IEvent;
