using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items.Types;

namespace NeoServer.Domain.Creatures.Events.Player;

public record PlayerReadTextEvent(IPlayer Player, IReadable Readable, string Text) : IEvent;