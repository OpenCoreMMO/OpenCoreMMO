using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Houses.Events;

/// <summary>Raised when rent is due and the owner lacks funds.</summary>
public record HouseRentWarningEvent(House House, IPlayer Owner, byte WarningNumber) : IEvent;
