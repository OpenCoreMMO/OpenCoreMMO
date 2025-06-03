using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Spells;

namespace NeoServer.Domain.Combat.Services.Attacks.Events;

public record SpellFailedToCastEvent(ICombatActor Caster, ISpell Spell, InvalidOperation Error) : IEvent;