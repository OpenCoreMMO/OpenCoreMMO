using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;

namespace NeoServer.Domain.Creatures.Events.Player;

public record PlayerLookedAtEvent(IPlayer Player, IThing Thing, bool IsClose) : IEvent;
