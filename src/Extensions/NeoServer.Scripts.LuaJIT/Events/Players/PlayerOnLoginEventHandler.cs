using NeoServer.Domain.Common;
using NeoServer.Domain.Creatures.Events.Player;
using NeoServer.Scripts.LuaJIT.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Events.Players;

public class PlayerOnLoginEventHandler(ICreatureEvents creatureEvents) : IApplicationEventHandler<PlayerLoggedInEvent>
{
    public void Handle(PlayerLoggedInEvent @event)
    {
        creatureEvents.PlayerLogin(@event.Player);
    }
}