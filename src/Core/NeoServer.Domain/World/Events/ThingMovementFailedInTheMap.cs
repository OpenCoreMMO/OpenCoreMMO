using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Items;

namespace NeoServer.Domain.World.Events;

public record ThingMovementFailedInTheMap(IThing Thing, InvalidOperation Error): IEvent;