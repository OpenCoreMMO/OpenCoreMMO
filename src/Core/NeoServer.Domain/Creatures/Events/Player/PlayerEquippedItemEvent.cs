using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common;

namespace NeoServer.Domain.Creatures.Events.Player;

public record PlayerEquippedItemEvent(IPlayer Player, IItem Item) : IEvent;
