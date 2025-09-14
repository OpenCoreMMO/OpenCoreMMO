using NeoServer.Domain.Common;

namespace NeoServer.Domain.Creatures.Events.Player;

public record PlayerLogoutEvent(Creatures.Player.Player Player) : IEvent;