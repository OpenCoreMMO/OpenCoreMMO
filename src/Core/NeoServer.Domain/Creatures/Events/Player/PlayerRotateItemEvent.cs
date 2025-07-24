using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Creatures.Events.Player;

public record PlayerRotateItemEvent(IPlayer Player, IItem Item, Location Position) : IEvent;