using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Spells;

namespace NeoServer.Game.Combat.Services.Attacks.Events;

public record SpellFailedToCastEvent(ICombatActor Caster, ISpell Spell, InvalidOperation Error) : IEvent;