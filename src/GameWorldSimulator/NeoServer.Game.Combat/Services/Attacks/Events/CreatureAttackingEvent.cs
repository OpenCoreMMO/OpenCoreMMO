using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location.Structs;

namespace NeoServer.Game.Combat.Services.Attacks.Events;

public record CreatureAttackingEvent(
    IThing Aggressor,
    IThing Target,
    ShootType ShootType,
    EffectT Effect,
    bool AttackMissed,
    Location[] Area = null) : IEvent;