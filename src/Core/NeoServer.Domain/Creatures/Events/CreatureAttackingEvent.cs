using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Creatures.Events;

public record CreatureAttackingEvent(
    IThing Aggressor,
    IThing Target,
    ShootType ShootType,
    EffectT Effect,
    bool AttackMissed,
    Location[] Area = null) : IEvent;