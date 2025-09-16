using NeoServer.Domain.Common;

namespace NeoServer.Domain.Creatures.Events.Player;

public record PlayerLoggedInEvent(Creatures.Player.Player Player) : IEvent;