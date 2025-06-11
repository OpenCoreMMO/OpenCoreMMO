using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;

namespace NeoServer.Domain.Creatures.Events;

public record CreatureDeathEvent(ICombatActor DeadCreature, IThing Attacker) : IEvent;