using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Houses.Events;
using NeoServer.Server.Services;

namespace NeoServer.Server.Events.Houses;

public class PlayerKickedFromHouseEventHandler : IApplicationEventHandler<PlayerKickedFromHouseEvent>
{
    public void Handle(PlayerKickedFromHouseEvent @event)
    {
        EffectService.Send(@event.From, EffectT.Puff);
        EffectService.Send(@event.To, EffectT.BubbleBlue);
    }
}
