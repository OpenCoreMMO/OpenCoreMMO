using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Player.Outfit;

namespace NeoServer.Domain.Creatures.Events;

public record CreatureChangedOutfitEvent(
    ICreature Creature,
    Outfit Outfit) : IEvent;
