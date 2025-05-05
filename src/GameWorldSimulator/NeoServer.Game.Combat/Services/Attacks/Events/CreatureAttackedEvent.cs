using NeoServer.Game.Common;

namespace NeoServer.Game.Combat.Services.Attacks.Events;

public record CreatureAttackedEvent(AttackInput AttackInput): IEvent;