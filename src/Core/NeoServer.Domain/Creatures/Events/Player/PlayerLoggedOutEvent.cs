using NeoServer.Domain.Common;

namespace NeoServer.Domain.Creatures.Events.Player;

public record PlayerLoggedOutEvent(Creatures.Player.Player Player) : IEvent;