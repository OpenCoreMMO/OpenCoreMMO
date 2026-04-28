using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types.Usable;

namespace NeoServer.Domain.Creatures.Events.Player;

public record PlayerUsedItemEvent(IPlayer Player, IThing Thing, IUsableOn Item) : IEvent;
