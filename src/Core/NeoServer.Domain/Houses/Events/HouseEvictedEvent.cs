using NeoServer.Domain.Common;

namespace NeoServer.Domain.Houses.Events;

/// <summary>Raised when a house is evicted after 7 unpaid rent warnings.</summary>
public record HouseEvictedEvent(House House) : IEvent;
