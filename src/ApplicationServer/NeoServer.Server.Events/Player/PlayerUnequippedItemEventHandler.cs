using NeoServer.Domain.Common;
using NeoServer.Domain.Creatures.Events.Player;

namespace NeoServer.Server.Events.Player;

public class PlayerUnequippedItemEventHandler : IApplicationEventHandler<PlayerUnequippedItemEvent>
{
    public void Handle(PlayerUnequippedItemEvent @event)
    {
        if (@event is null) return;

        // Application-layer handling for player item unequip.
        // Add networking or scripting callbacks here as needed.
    }
}
