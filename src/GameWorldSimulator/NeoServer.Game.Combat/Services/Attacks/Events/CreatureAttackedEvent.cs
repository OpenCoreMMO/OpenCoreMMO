using NeoServer.Game.Common;
using NeoServer.Game.Common.Combat.Structs;

namespace NeoServer.Game.Combat.Services.Attacks.Events;

public record CreatureAttackedEvent(AttackInput AttackInput, bool AttackMissed): IEvent;