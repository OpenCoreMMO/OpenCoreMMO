using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Location;

namespace NeoServer.Domain.Creatures.Events.Player;

public record PlayerWalkEvent(IPlayer Player, Direction Direction) : IEvent;