using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Items;

namespace NeoServer.Domain.Items.Events;

public record ItemCreatedEvent(IItem Item) : IEvent;