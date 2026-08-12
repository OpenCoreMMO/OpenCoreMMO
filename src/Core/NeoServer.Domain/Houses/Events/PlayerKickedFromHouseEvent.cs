using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Houses.Events;

/// <summary>Raised when a player is kicked out of a house to its exit.</summary>
public record PlayerKickedFromHouseEvent(IPlayer Player, Location From, Location To) : IEvent;
