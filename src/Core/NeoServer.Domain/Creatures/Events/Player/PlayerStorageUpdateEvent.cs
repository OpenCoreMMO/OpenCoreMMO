using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Creatures.Events.Player;

public record PlayerStorageUpdateEvent(IPlayer Player, uint Key, int Value, int OldValue, ulong CurrentTime) : IEvent;