using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Creatures.Events.Player;

public record PlayerExhaustedEvent(IPlayer Player) : IEvent;
