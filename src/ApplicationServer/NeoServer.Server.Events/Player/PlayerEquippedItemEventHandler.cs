using NeoServer.Domain.Common;
using NeoServer.Domain.Creatures.Events.Player;

namespace NeoServer.Server.Events.Player;

public class PlayerEquippedItemEventHandler : IApplicationEventHandler<PlayerEquippedItemEvent>
{
    public void Handle(PlayerEquippedItemEvent @event)
    {
        if (@event is null) return;
    }
}
