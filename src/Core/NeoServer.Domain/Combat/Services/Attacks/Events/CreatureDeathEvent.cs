using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;

namespace NeoServer.Domain.Combat.Services.Attacks.Events;

public record CreatureDeathEvent(ICombatActor DeadCreature, IThing Attacker) : IEvent;