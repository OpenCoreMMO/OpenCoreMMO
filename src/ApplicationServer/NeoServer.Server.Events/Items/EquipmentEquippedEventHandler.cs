using NeoServer.Domain.Items.Events;
using NeoServer.Domain.Common;

namespace NeoServer.Server.Events.Items;

public class EquipmentEquippedEventHandler : IApplicationEventHandler<EquipmentEquippedEvent>
{
    public void Handle(EquipmentEquippedEvent @event)
    {
        if (@event is null) return;

        // Application-layer subscription for equipment equipped.
        // Keep lightweight: additional application logic (networking, scripting, etc.) can be added here.
    }
}
