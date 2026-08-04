using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Combat;
using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Creatures.Events;

public record CreatureBlockedAttackEvent(
    ICombatActor Actor,
    BlockType BlockType) : IEvent;
