using NeoServer.Domain.Common;

namespace NeoServer.Domain.Houses.Events;

/// <summary>Raised when a house gains or loses an owner.</summary>
public record HouseOwnerChangedEvent(House House, uint OldOwnerGuid, uint NewOwnerGuid) : IEvent;
