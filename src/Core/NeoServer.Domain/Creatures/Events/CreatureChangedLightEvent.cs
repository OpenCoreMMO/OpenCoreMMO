using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Creatures.Events;

public record CreatureChangedLightEvent(
    ICreature Creature) : IEvent;