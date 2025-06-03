using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;

namespace NeoServer.Domain.Combat.Services.Attacks.Events;

public record CreatureInjuredEvent(IThing Enemy, ICreature Victim, CombatDamageList DamageList) : IEvent;