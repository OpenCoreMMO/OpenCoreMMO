using NeoServer.Domain.Items.Events;
using NeoServer.Domain.Common;

namespace NeoServer.Server.Events.Items;

public class EquipmentUnequippedEventHandler : IApplicationEventHandler<EquipmentUnequippedEvent>
{
    public void Handle(EquipmentUnequippedEvent @event)
    {
        if (@event is null) return;

        // Application-layer subscription for equipment unequipped.
        // Add application-specific behavior here (networking, scripting, etc.).
    }
}
